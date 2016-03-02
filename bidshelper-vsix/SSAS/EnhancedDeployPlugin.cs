using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.AnalysisServices;

namespace BIDSHelper.SSAS
{
    class EnhancedDeployPlugin: BIDSHelperPluginBase
    {
        public EnhancedDeployPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
        }

        public override string ShortName
        {
            get { return "EnhancedDeployPlugin"; }
        }

        public override string ButtonText
        {
            get { return "Enhanced Deploy (BIDS Helper)..."; }
        }

        public override string ToolTip
        {
            get { return "Deploy (BIDSHelper)"; }
        }

        public override int Bitmap
        {
            get { return 1; }
        }

        public override string FriendlyName
        {
            get
            {
                return "Enhanced SSAS Deploy";
            }
        }

        public override string MenuName
        {
            get { return "Project,Solution"; }
        }

        public override void Exec()
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            //SolutionClass solution = hierItem.Object as SolutionClass;

            DeploymentSettings ds = new DeploymentSettings( (Project)hierItem.Object);
            EnhancedDeployWindow dlg = new EnhancedDeployWindow();
            dlg.TargetDatabase = ds.TargetDatabase;
            dlg.TargetServer = ds.TargetServer;
            dlg.SourceObject = (Database)((Project)hierItem.Object).Object;
            dlg.ShowDialog();
        }

        public override bool DisplayCommand(EnvDTE.UIHierarchyItem item)
        {
            try
            {
                UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
                if (((System.Array)solExplorer.SelectedItems).Length != 1)
                    return false;

                UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
                SolutionClass solution = hierItem.Object as SolutionClass;
                if (hierItem.Object is Project)
                {
                    Project p = (Project)hierItem.Object;
                    if (!(p.Object is Database)) return false;
                    Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt projExt = (Microsoft.DataWarehouse.VsIntegration.Shell.Project.Extensibility.ProjectExt)p;
                    return (projExt.Kind == BIDSProjectKinds.SSAS) ;
                }
                
            }
            catch { }
            return false;
        }
    }
}
