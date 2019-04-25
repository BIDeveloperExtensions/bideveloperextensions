using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.Collections.Generic;
using Microsoft.AnalysisServices;
using Microsoft.AnalysisServices.Common;
using System.Linq;

namespace BIDSHelper.SSAS
{
    [FeatureCategory(BIDSFeatureCategories.SSASTabular)]
    public class TabularSyncDescriptionsPlugin : BIDSHelperWindowActivatedPluginBase
    {
        #region Standard Plugin Overrides
        public TabularSyncDescriptionsPlugin(BIDSHelperPackage package)
            : base(package)
        {
        }

        //public override int Bitmap
        //{
        //    get { return 144; }
        //}

        public override string FeatureName
        {
            get { return "Tabular Sync Descriptions"; }
        }

        //public override string MenuName
        //{
        //    get { return "Item"; }
        //}

        public override string ToolTip
        {
            get { return string.Empty; } //not used anywhere
        }

        //public override bool ShouldPositionAtEnd
        //{
        //    get { return true; }
        //}

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSASTabular; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "Sync descriptions from extended properties on SQL Sever tables to your model table."; }
        }

        public override bool ShouldHookWindowCreated
        {
            get
            {
                return true;
            }
        }

        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {

            try
            {
                HookBimWindow(GotFocus.ProjectItem);
            }
            catch { }
        }
        #endregion

        private List<SandboxEditor> _hookedSandboxEditors = new List<SandboxEditor>();
        private void HookBimWindow(ProjectItem pi)
        {
            try
            {
                if (pi == null || pi.Name == null) return;
                string sFileName = pi.Name.ToLower();
                if (!sFileName.EndsWith(".bim")) return;

                SandboxEditor editor = TabularHelpers.GetTabularSandboxEditorFromProjectItem(pi, false);
                if (editor == null) return;
                Microsoft.AnalysisServices.Common.ERDiagram diagram = TabularHelpers.GetTabularERDiagramFromSandboxEditor(editor);
                if (diagram != null)
                {
                    SetupContextMenu(diagram);
                }
                else
                {
                    if (!_hookedSandboxEditors.Contains(editor))
                    {
                        editor.DiagramObjectsSelected += new EventHandler<ERDiagramSelectionChangedEventArgs>(editor_DiagramObjectsSelected);
                        _hookedSandboxEditors.Add(editor);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        void editor_DiagramObjectsSelected(object sender, ERDiagramSelectionChangedEventArgs e)
        {
            try
            {
                SetupContextMenu(sender as ERDiagram);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }
        }

        private void SetupContextMenu(ERDiagram diagram)
        {
            if (diagram == null) return;
#if DENALI || SQL2014
            foreach (IDiagramAction action in diagram.Actions)
#else
            foreach (IViewModelAction action in diagram.Actions)
#endif
            {
                if (action is ERDiagramActionSyncDescriptions) return; //if this context menu is already part of the diagram, then we're done
            }

            

            ERDiagramActionSyncDescriptions syncAction = new ERDiagramActionSyncDescriptions(diagram, this);
            syncAction.Text = "Sync Descriptions...";
            syncAction.DisplayIndex = 0x19f;
#if DENALI || SQL2014
            IDiagramTag tagTable = (IDiagramTag)diagram.GetType().InvokeMember("tagTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, diagram, null);
            syncAction.Key = new DiagramObjectKey(@"Actions\{0}", new object[] { "SyncDescriptions" });
            syncAction.AvailableRule = delegate(IEnumerable<IEnumerable<IDiagramTag>> tagSets)
            {
                return tagSets.All<IEnumerable<IDiagramTag>>(tagSet => tagSet.Contains<IDiagramTag>(tagTable));
            };
#else
            IViewModelTag tagTable = (IViewModelTag)diagram.GetType().InvokeMember("tagTable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance, null, diagram, null);
            syncAction.Key = new ViewModelObjectKey(@"Actions\{0}", new object[] { "SyncDescriptions" });
            syncAction.AvailableRule = delegate (IEnumerable<IEnumerable<IViewModelTag>> tagSets)
            {
                return tagSets.All<IEnumerable<IViewModelTag>>(tagSet => tagSet.Contains<IViewModelTag>(tagTable));
            };
#endif
            diagram.GetType().InvokeMember("InitializeViewStates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod, null, diagram, new object[] { syncAction });
            diagram.Actions.Add(syncAction);
        }

        internal void ExecuteSyncDescriptions(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox, IServiceProvider provider, string tableName)
        {
            try
            {
#if DENALI || SQL2014
                var db = sandbox.Database;
                Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.AMOCode code;
#else
                Database db = null;
                if (!sandbox.IsTabularMetadata)
                    db = ((Microsoft.AnalysisServices.BackEnd.DataModelingSandboxAmo)sandbox.Impl).Database;
                else
                    db = null;
                Microsoft.AnalysisServices.BackEnd.AMOCode code;
#endif
                code = delegate
                    {
                        int iDescriptionsSet;
                        Microsoft.AnalysisServices.BackEnd.SandboxTransactionProperties properties = new Microsoft.AnalysisServices.BackEnd.SandboxTransactionProperties();
                        properties.RecalcBehavior = Microsoft.AnalysisServices.BackEnd.TransactionRecalcBehavior.Default;
                        using (Microsoft.AnalysisServices.BackEnd.SandboxTransaction tran = sandbox.CreateTransaction(properties))
                        {
                            if (!TabularHelpers.EnsureDataSourceCredentials(sandbox))
                            {
                                MessageBox.Show("Cancelling Sync Descriptions because data source credentials were not entered.", "BIDS Helper Tabular Sync Descriptions - Cancelled!");
                                tran.RollbackAndContinue();
                                return;
                            }
#if !(DENALI || SQL2014)
                            Microsoft.AnalysisServices.BackEnd.DataModelingTable table = sandbox.Tables[tableName];
                            if (table.IsStructuredDataSource)
                            {
                                MessageBox.Show("BI Developer Extensions does not yet support modern (Power Query) data sources.", "BI Developer Extensions");
                                return;
                            }
                            iDescriptionsSet = SyncDescriptionsPlugin.SyncDescriptions(table, true);
                            if (iDescriptionsSet > 0)
                            {
                                table.UpdateNowOrLater();
                            }
#else
                            Dimension d = db.Dimensions.GetByName(tableName);
                            iDescriptionsSet = SyncDescriptionsPlugin.SyncDescriptions(d, true, provider, true);
                            if (iDescriptionsSet > 0)
                                db.Update(UpdateOptions.ExpandFull);
#endif
                            tran.GetType().InvokeMember("Commit", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public, null, tran, null); //The .Commit() function used to return a list of strings, but in the latest set of code it is a void method which leads to "method not found" errors
                            //tran.Commit();
                        }

                        MessageBox.Show("Set " + iDescriptionsSet + " descriptions successfully.", "BIDS Helper - Sync Descriptions");
                    };
#if DENALI || SQL2014
                sandbox.ExecuteAMOCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#else
                sandbox.ExecuteEngineCode(Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationType.Update, Microsoft.AnalysisServices.BackEnd.DataModelingSandbox.OperationCancellability.AlwaysExecute, code, true);
#endif
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
            }

        }


        public override void Exec()
        {
        }
#if DENALI || SQL2014
        internal class ERDiagramActionSyncDescriptions : SSAS.Tabular.ERDiagramActionBase, IDiagramActionBasic, IDiagramAction, IDiagramObject, System.ComponentModel.INotifyPropertyChanged, INotifyCollectionPropertyChanged
#else
        internal class ERDiagramActionSyncDescriptions : SSAS.Tabular.ERDiagramActionBase, IViewModelActionBasic, IViewModelAction, IViewModelObject, System.ComponentModel.INotifyPropertyChanged, INotifyCollectionPropertyChanged
#endif
        {
            private TabularSyncDescriptionsPlugin _plugin;
            public ERDiagramActionSyncDescriptions(ERDiagram diagramInput, TabularSyncDescriptionsPlugin plugin)
                : base(diagramInput)
            {
                _plugin = plugin;
                this.Icon = DiagramIcon.None;
            }

#if DENALI || SQL2014
            public override void Cancel(IDiagramActionInstance actionInstance) { }

            public override IShowMessageRequest Confirm(IDiagramActionInstance actionInstance) { return null; }

            public override void Consider(IDiagramActionInstance actionInstance) { }
            public override DiagramActionResult Do(IDiagramActionInstance actionInstance)
            {
                try
                {
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromActiveWindow(_plugin.package);
                    if (sandbox == null) throw new Exception("Can't get Sandbox!");
                    IServiceProvider provider = TabularHelpers.GetTabularServiceProviderFromActiveWindow(_plugin.package);

                    foreach (IDiagramNode node in actionInstance.Targets.OfType<IDiagramNode>())
                    {
                        _plugin.ExecuteSyncDescriptions(sandbox, provider, node.Text);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
                }
                return new DiagramActionResult(null, (IDiagramObject)null);
            }
#else

            public override void Cancel(IViewModelActionInstance actionInstance) { }

            public override IShowMessageRequest Confirm(IViewModelActionInstance actionInstance) { return null; }

            public override void Consider(IViewModelActionInstance actionInstance) { }

            public override ViewModelActionResult Do(IViewModelActionInstance actionInstance)
            {
                try
                {
                    Microsoft.AnalysisServices.BackEnd.DataModelingSandbox sandbox = TabularHelpers.GetTabularSandboxFromActiveWindow(_plugin.package);
                    if (sandbox == null) throw new Exception("Can't get Sandbox!");
                    IServiceProvider provider = TabularHelpers.GetTabularServiceProviderFromActiveWindow(_plugin.package);

                    foreach (IViewModelNode node in actionInstance.Targets.OfType<IViewModelNode>())
                    {
                        _plugin.ExecuteSyncDescriptions(sandbox, provider, node.Text);
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "BIDS Helper - Error");
                }
                return new ViewModelActionResult(null, (IViewModelObject)null);
            }
        
#endif
        }
    }
}
