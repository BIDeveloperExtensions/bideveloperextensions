using Extensibility;
using EnvDTE;
using EnvDTE80;
using System.Xml;
using Microsoft.VisualStudio.CommandBars;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.ComponentModel.Design;
using Microsoft.DataWarehouse.Design;
using Microsoft.DataWarehouse.Controls;
using Microsoft.DataWarehouse.ComponentModel;
using System;
using Microsoft.Win32;
using System.Collections;

namespace BIDSHelper
{
    public class TriStatePerspectivesPlugin : BIDSHelperWindowActivatedPluginBase
    {
        
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.Dictionary<string,EditorWindow> windowHandlesFixedForPerspectives = new System.Collections.Generic.Dictionary<string,EditorWindow>();
        private System.Collections.Generic.Dictionary<string,EditorWindow> windowHandlesFixedForGridEvents = new System.Collections.Generic.Dictionary<string,EditorWindow>();

        public TriStatePerspectivesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {

        }

        public override bool ShouldHookWindowCreated
        {
            get
            {
                return true;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            foreach (EditorWindow win in windowHandlesFixedForGridEvents.Values)
            {
                win.ActiveViewChanged -= win_ActiveViewChanged;            
                // get toolbar and remove click handlers
                Control perspectiveBuilder = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null); //Microsoft.AnalysisServices.Design.PerspectivesBuilder
                Control grid = perspectiveBuilder.Controls[0]; //Microsoft.SqlServer.Management.UI.Grid.DlgGridControl
                grid.MouseClick -= grid_MouseClick;
                grid.KeyPress -= grid_KeyPress;

            }

            foreach (EditorWindow win in windowHandlesFixedForPerspectives.Values)
            {
                win.ActiveViewChanged -= win_ActiveViewChanged;
            }
        }


        public override void OnWindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = GotFocus.Object as IDesignerHost;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if ((pi==null) || (!(pi.Object is Cube))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);
                Cube cube = (Cube)pi.Object;


                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedForPerspectives.ContainsKey(sHandle))
                {
                    windowHandlesFixedForPerspectives.Add(sHandle,win);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                //if (win.SelectedView.Caption == "Perspectives")
                if (win.SelectedView.MenuItemCommandID.ID == (int) BIDSViewMenuItemCommandID.Perspectives)
                {
                    Microsoft.AnalysisServices.Design.Scripts mdxScriptCache = new Microsoft.AnalysisServices.Design.Scripts(cube);

                    Control perspectiveBuilder = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null); //Microsoft.AnalysisServices.Design.PerspectivesBuilder
                    Control grid = perspectiveBuilder.Controls[0]; //Microsoft.SqlServer.Management.UI.Grid.DlgGridControl

                    if (!windowHandlesFixedForGridEvents.ContainsKey(sHandle))
                    {
                        grid.MouseClick += new MouseEventHandler(grid_MouseClick);
                        grid.KeyPress += new KeyPressEventHandler(grid_KeyPress);
                        windowHandlesFixedForGridEvents.Add(sHandle,win);
                    }
                    
                    System.Reflection.BindingFlags getpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;

                    object dlgStorage = null;
#if KATMAI || DENALI
                    dlgStorage = grid.GetType().BaseType.BaseType.InvokeMember("DlgStorage", getpropertyflags, null, grid, null);
#else
                    dlgStorage = grid.GetType().BaseType.InvokeMember("DlgStorage", getpropertyflags, null, grid, null); //Microsoft.SqlServer.Management.UI.Grid.IDlgStorage
#endif

                    object storage = dlgStorage.GetType().InvokeMember("Storage", getpropertyflags, null, dlgStorage, null); //Microsoft.SqlServer.Management.UI.Grid.MemDataStorage

                    System.Reflection.BindingFlags getfieldflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                    ArrayList rows = (ArrayList)storage.GetType().BaseType.InvokeMember("m_arrRows", getfieldflags, null, storage, new object[] { });
                    //ArrayList columns = (ArrayList)storage.GetType().BaseType.InvokeMember("m_arrColumns", getfieldflags, null, storage, new object[] { });
                    object[] perspectivesColumns = (object[])rows[0];

                    ArrayList allGridCubeObjects = (ArrayList)perspectiveBuilder.GetType().InvokeMember("allGridCubeObjects", getfieldflags, null, perspectiveBuilder, null);

                    for (int i = 3; i < rows.Count; i++)
                    {
                        object[] columns = (object[])rows[i];
                        object cell = columns[2]; //Microsoft.SqlServer.Management.UI.Grid.GridCell
                        string rowObjectType = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                        if (rowObjectType == "MeasureGroup")
                        {
                            cell = columns[1];
                            string rowObjectName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                            MeasureGroup mg = cube.MeasureGroups.GetByName(rowObjectName);
                            for (int j = 3; j < columns.Length; j += 2)
                            {
                                cell = perspectivesColumns[j + 1];
                                string perspectiveName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                                Perspective perspective = cube.Perspectives.GetByName(perspectiveName);
                                cell = columns[j];
                                bool bHighlight = false;
                                if (perspective.MeasureGroups.Contains(mg.ID))
                                {
                                    PerspectiveMeasureGroup pmg = perspective.MeasureGroups[mg.ID];
                                    foreach (Measure m in mg.Measures)
                                    {
                                        if (m.Visible && !pmg.Measures.Contains(m.ID))
                                        {
                                            bHighlight = true;
                                            break;
                                        }
                                    }
                                }
                                System.Reflection.BindingFlags setpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                System.Drawing.SolidBrush brush = null;
                                if (bHighlight)
                                {
                                    brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                                }
                                else
                                {
                                    brush = (System.Drawing.SolidBrush)columns[j + 1].GetType().InvokeMember("BkBrush", getpropertyflags, null, columns[j + 1], null);
                                }
                                lock (cell)
                                {
                                    cell.GetType().InvokeMember("BkBrush", setpropertyflags, null, cell, new object[] { brush });
                                }
                            }
                        }
                        else if (rowObjectType == "CubeDimension")
                        {
                            cell = columns[1];
                            string rowObjectName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                            CubeDimension cd = cube.Dimensions.GetByName(rowObjectName);
                            for (int j = 3; j < columns.Length; j += 2)
                            {
                                cell = perspectivesColumns[j + 1];
                                string perspectiveName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                                Perspective perspective = cube.Perspectives.GetByName(perspectiveName);
                                cell = columns[j];
                                bool bHighlight = false;
                                if (perspective.Dimensions.Contains(cd.ID))
                                {
                                    PerspectiveDimension pcd = perspective.Dimensions[cd.ID];
                                    foreach (CubeHierarchy h in cd.Hierarchies)
                                    {
                                        if (h.Visible && h.Enabled && !pcd.Hierarchies.Contains(h.HierarchyID))
                                        {
                                            bHighlight = true;
                                            break;
                                        }
                                    }
                                    if (!bHighlight)
                                    {
                                        foreach (CubeAttribute a in cd.Attributes)
                                        {
                                            if (a.AttributeHierarchyVisible && a.AttributeHierarchyEnabled && a.Attribute.AttributeHierarchyVisible && a.Attribute.AttributeHierarchyEnabled && !pcd.Attributes.Contains(a.AttributeID))
                                            {
                                                bHighlight = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                System.Reflection.BindingFlags setpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                System.Drawing.SolidBrush brush = null;
                                if (bHighlight)
                                {
                                    brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                                }
                                else
                                {
                                    brush = (System.Drawing.SolidBrush)columns[j + 1].GetType().InvokeMember("BkBrush", getpropertyflags, null, columns[j + 1], null);
                                }
                                lock (cell)
                                {
                                    cell.GetType().InvokeMember("BkBrush", setpropertyflags, null, cell, new object[] { brush });
                                }
                            }
                        }
                        else if (rowObjectType == "Kpi")
                        {
                            cell = columns[1];
                            string rowObjectName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                            rowObjectName = rowObjectName.Trim();
                            Kpi kpi = cube.Kpis.GetByName(rowObjectName);
                            for (int j = 3; j < columns.Length; j += 2)
                            {
                                cell = perspectivesColumns[j + 1];
                                string perspectiveName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                                Perspective perspective = cube.Perspectives.GetByName(perspectiveName);
                                cell = columns[j];
                                bool bHighlight = false;
                                if (perspective.Kpis.Contains(kpi.ID))
                                {
                                    PerspectiveKpi pkpi = perspective.Kpis[kpi.ID];
                                    bHighlight = ShouldPerspectiveKpiBeHighlighted(pkpi, mdxScriptCache);
                                }
                                System.Reflection.BindingFlags setpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                System.Drawing.SolidBrush brush = null;
                                if (bHighlight)
                                {
                                    brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                                }
                                else
                                {
                                    brush = (System.Drawing.SolidBrush)columns[j + 1].GetType().InvokeMember("BkBrush", getpropertyflags, null, columns[j + 1], null);
                                }
                                lock (cell)
                                {
                                    cell.GetType().InvokeMember("BkBrush", setpropertyflags, null, cell, new object[] { brush });
                                }
                            }
                        }
                        else if (rowObjectType.EndsWith("Action"))
                        {
                            cell = columns[1];
                            string rowObjectName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                            rowObjectName = rowObjectName.Trim();
                            Microsoft.AnalysisServices.Action action = cube.Actions.GetByName(rowObjectName);
                            for (int j = 3; j < columns.Length; j += 2)
                            {
                                cell = perspectivesColumns[j + 1];
                                string perspectiveName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                                Perspective perspective = cube.Perspectives.GetByName(perspectiveName);
                                cell = columns[j];
                                bool bHighlight = false;
                                if (perspective.Actions.Contains(action.ID))
                                {
                                    bHighlight = true;
                                    if (action.TargetType == ActionTargetType.DimensionMembers)
                                    {
                                        foreach (PerspectiveDimension dim in perspective.Dimensions)
                                        {
                                            if (string.Compare(action.Target, "[" + dim.CubeDimension.Name + "]", true) == 0)
                                            {
                                                bHighlight = false;
                                                break;
                                            }
                                        }
                                    }
                                    else if (action.TargetType == ActionTargetType.AttributeMembers || action.TargetType == ActionTargetType.HierarchyMembers)
                                    {
                                        foreach (PerspectiveDimension dim in perspective.Dimensions)
                                        {
                                            foreach (PerspectiveAttribute attr in dim.Attributes)
                                            {
                                                if (string.Compare(action.Target, "[" + attr.Parent.CubeDimension.Name + "].[" + attr.Attribute.Name + "]", true) == 0)
                                                {
                                                    bHighlight = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else if (action.TargetType == ActionTargetType.Level || action.TargetType == ActionTargetType.LevelMembers)
                                    {
                                        foreach (PerspectiveDimension dim in perspective.Dimensions)
                                        {
                                            foreach (PerspectiveHierarchy hier in dim.Hierarchies)
                                            {
                                                foreach (Level level in hier.Hierarchy.Levels)
                                                {
                                                    if (string.Compare(action.Target, "[" + hier.Parent.CubeDimension.Name + "].[" + hier.Hierarchy.Name + "].[" + level.Name + "]", true) == 0)
                                                    {
                                                        bHighlight = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (action.TargetType == ActionTargetType.HierarchyMembers)
                                    {
                                        foreach (PerspectiveDimension dim in perspective.Dimensions)
                                        {
                                            foreach (PerspectiveHierarchy hier in dim.Hierarchies)
                                            {
                                                if (string.Compare(action.Target, "[" + hier.Parent.CubeDimension.Name + "].[" + hier.Hierarchy.Name + "]", true) == 0)
                                                {
                                                    bHighlight = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bHighlight = false;
                                    }
                                }

                                System.Reflection.BindingFlags setpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                System.Drawing.SolidBrush brush = null;
                                if (bHighlight)
                                {
                                    brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                                }
                                else
                                {
                                    brush = (System.Drawing.SolidBrush)columns[j + 1].GetType().InvokeMember("BkBrush", getpropertyflags, null, columns[j + 1], null);
                                }
                                lock (cell)
                                {
                                    cell.GetType().InvokeMember("BkBrush", setpropertyflags, null, cell, new object[] { brush });
                                }
                            }
                        }
                        else if (rowObjectType == "CalculatedMember")
                        {
                            cell = columns[1];
                            string rowObjectName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                            rowObjectName = rowObjectName.Trim();

                            for (int j = 3; j < columns.Length; j += 2)
                            {
                                cell = perspectivesColumns[j + 1];
                                string perspectiveName = (string)cell.GetType().InvokeMember("CellData", getpropertyflags, null, cell, null);
                                Perspective perspective = cube.Perspectives.GetByName(perspectiveName);
                                cell = columns[j];

                                Microsoft.AnalysisServices.Design.Script calc = (Microsoft.AnalysisServices.Design.Script)allGridCubeObjects[i].GetType().InvokeMember("Object", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField, null, allGridCubeObjects[i], null);
                                if (CalcIsHidden(calc)) continue;
                                                                
                                bool bHighlight = false;
                                if (calc != null && calc.CalculationProperty != null && perspective.Calculations.Contains(calc.CalculationProperty.CalculationReference) && !CalcIsHidden(calc))
                                {
                                    if (!string.IsNullOrEmpty(calc.CalculationProperty.AssociatedMeasureGroupID))
                                    {
                                        if (!perspective.MeasureGroups.Contains(calc.CalculationProperty.AssociatedMeasureGroupID))
                                            bHighlight = true;
                                    }
                                }

                                System.Reflection.BindingFlags setpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                                System.Drawing.SolidBrush brush = null;
                                if (bHighlight)
                                {
                                    brush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
                                }
                                else
                                {
                                    brush = (System.Drawing.SolidBrush)columns[j + 1].GetType().InvokeMember("BkBrush", getpropertyflags, null, columns[j + 1], null);
                                }
                                lock (cell)
                                {
                                    cell.GetType().InvokeMember("BkBrush", setpropertyflags, null, cell, new object[] { brush });
                                }
                            }
                        }
                    }
                    grid.Refresh();
                }
            }
            catch { }
        }

        private bool ShouldPerspectiveKpiBeHighlighted(PerspectiveKpi pkpi, Microsoft.AnalysisServices.Design.Scripts mdxScript)
        {
            return ShouldPerspectiveKpiColumnBeHighlighted(pkpi, mdxScript, pkpi.Kpi.Value)
            || ShouldPerspectiveKpiColumnBeHighlighted(pkpi, mdxScript, pkpi.Kpi.Goal)
            || ShouldPerspectiveKpiColumnBeHighlighted(pkpi, mdxScript, pkpi.Kpi.Status)
            || ShouldPerspectiveKpiColumnBeHighlighted(pkpi, mdxScript, pkpi.Kpi.Trend);
        }

        //return true if the kpi column is a reference to a measure, and if that measure isn't in the perspective or is hidden
        private bool ShouldPerspectiveKpiColumnBeHighlighted(PerspectiveKpi pkpi, Microsoft.AnalysisServices.Design.Scripts mdxScript, string column)
        {
            if (string.IsNullOrEmpty(column)) return false;
            foreach (MeasureGroup mg in pkpi.ParentCube.MeasureGroups)
            {
                foreach (Measure m in mg.Measures)
                {
                    if (string.Compare("[Measures].[" + m.Name + "]", column, true) == 0)
                    {
                        if (pkpi.Parent.MeasureGroups.Contains(mg.ID) && pkpi.Parent.MeasureGroups[mg.ID].Measures.Contains(m.ID))
                            return false;
                        else
                            return true;
                    }
                }
            }

            //foreach (object calcMember in mdxScript.CalculatedMembers)
            //{
            //    Microsoft.AnalysisServices.Design.Script calc = calcMember as Microsoft.AnalysisServices.Design.Script;
            //    if (calc != null && calc.CalculationProperty != null && string.Compare(calc.CalculationProperty.CalculationReference, column, true) == 0)
            //    {
            //        if (CalcIsHidden(calc)) return false;
            //        if (pkpi.Parent.Calculations.Contains(column))
            //            return false;
            //        else //found the calc, it's visible, but it isn't in the perspective
            //            return true;
            //    }
            //}
            return false;
        }

        public static bool CalcIsHidden(Microsoft.AnalysisServices.Design.Script calc)
        {
            //calc.CalculationProperty.Visible doesn't seem to work
            object mdxCodeCalc = calc.GetType().InvokeMember("mdxCodeCalc", System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, calc, null);
            bool bIsHidden = (bool)mdxCodeCalc.GetType().InvokeMember("IsHidden", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, null, mdxCodeCalc, null);
            return bIsHidden;
        }

        void grid_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch { }
        }

        void grid_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch { }
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            try
            {
                OnWindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch { }
        }

        public override string ShortName
        {
            get { return "TriStatePerspectives"; }
        }

        public override int Bitmap
        {
            get { return 0; }
        }

        public override string ButtonText
        {
            get { return "Tri-State Perspectives"; }
        }

        public override string ToolTip
        {
            get { return string.Empty; }
        }

        public override string MenuName
        {
            get { return string.Empty; } //no need to have a menu command
        }

        /// <summary>
        /// Gets the feature category used to organise the plug-in in the enabled features list.
        /// </summary>
        /// <value>The feature category.</value>
        public override BIDSFeatureCategories FeatureCategory
        {
            get { return BIDSFeatureCategories.SSAS; }
        }

        /// <summary>
        /// Gets the full description used for the features options dialog.
        /// </summary>
        /// <value>The description.</value>
        public override string FeatureDescription
        {
            get { return "An addition to the Perspectives tab of the cube designer, which highlights any measure groups or dimensions in which not all visible children are part of the perspective."; }
        }

        /// <summary>
        /// Determines if the command should be displayed or not.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool DisplayCommand(UIHierarchyItem item)
        {
            return false;
        }


        public override void Exec()
        {
        }
    }
}