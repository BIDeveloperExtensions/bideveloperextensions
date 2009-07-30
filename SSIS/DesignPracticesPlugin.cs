﻿using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using BIDSHelper.SSIS.DesignPracticeScanner;
using Microsoft.DataWarehouse.Design;
using Microsoft.SqlServer.Dts.Runtime;
using System.ComponentModel.Design;

namespace BIDSHelper.SSIS
{
    class DesignPracticesPlugin : BIDSHelperPluginBase
    {
        private static DesignPractices _practices = new DesignPractices();

        public DesignPracticesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            foreach (Type t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (typeof(DesignPractice).IsAssignableFrom(t)
                    && (!object.ReferenceEquals(t, typeof(DesignPractice)))
                    && (!t.IsAbstract))
                {
                    DesignPractice ext;
                    System.Type[] @params = { typeof(string) };
                    System.Reflection.ConstructorInfo constructor = t.GetConstructor(@params);

                    if (constructor == null)
                    {
                        System.Windows.Forms.MessageBox.Show("Problem loading type " + t.Name + ". No constructor found.");
                        continue;
                    }
                    ext = (DesignPractice)constructor.Invoke(new object[]{ PluginRegistryPath });
                    _practices.Add(ext);

                }
            }
        }

        public override string ShortName
        {
            get { return "Design Warnings Scanner"; }
        }

        public override string ButtonText
        {
            get { return "Design Warnings Scanner"; }
        }

        public override string ToolTip
        {
            get { return "This tool will scan the package for adherence to good design practices with SSIS."; }
        }

        public override int Bitmap
        {
            get { return 313; }
        }

        public override bool DisplayCommand(UIHierarchyItem item)
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length != 1) return false;

            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));

            string sFileName = ((ProjectItem)hierItem.Object).Name.ToLower();
            return (sFileName.EndsWith(".dtsx"));
        }

        public override void Exec()
        {
            UIHierarchy solExplorer = this.ApplicationObject.ToolWindows.SolutionExplorer;
            if (((System.Array)solExplorer.SelectedItems).Length != 1)
                return;

            UIHierarchyItem hierItem = ((UIHierarchyItem)((System.Array)solExplorer.SelectedItems).GetValue(0));
            ProjectItem pi = (ProjectItem)hierItem.Object;

            Window w = pi.Open(BIDSViewKinds.Designer); //opens the designer
            w.Activate();

            IDesignerHost designer = w.Object as IDesignerHost;
            if (designer == null) return;
            EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
            Package package = win.PropertiesLinkComponent as Package;
            if (package == null) return;

            Results results = new Results();

            foreach (DesignPractice practice in _practices)
            {
                if (!practice.Enabled) continue;
                practice.Check(package, pi);
                results.AddRange(practice.Results);
            }

            AddErrorsToVSErrorList(w, results);
        }

        private void AddErrorsToVSErrorList(Window window, Results errors)
        {
            ErrorList errorList = this.ApplicationObject.ToolWindows.ErrorList;
            Window2 errorWin2 = (Window2)(errorList.Parent);
            if (errors.Count > 0)
            {
                if (!errorWin2.Visible)
                {
                    this.ApplicationObject.ExecuteCommand("View.ErrorList", " ");
                }
                errorWin2.SetFocus();
            }

            IDesignerHost designer = (IDesignerHost)window.Object;
            ITaskListService service = designer.GetService(typeof(ITaskListService)) as ITaskListService;

            //remove old task items from this document and BIDS Helper class
            System.Collections.Generic.List<ITaskItem> tasksToRemove = new System.Collections.Generic.List<ITaskItem>();
            foreach (ITaskItem ti in service.GetTaskItems())
            {
                ICustomTaskItem task = ti as ICustomTaskItem;
                if (task != null && task.CustomInfo == this && task.Document == window.ProjectItem.Name)
                {
                    tasksToRemove.Add(ti);
                }
            }
            foreach (ITaskItem ti in tasksToRemove)
            {
                service.Remove(ti);
            }


            foreach (Result result in errors)
            {
                if (result.Passed) continue;
                ICustomTaskItem item = (ICustomTaskItem)service.CreateTaskItem(TaskItemType.Custom, result.ResultExplanation);
                item.Category = TaskItemCategory.Misc;
                item.Appearance = TaskItemAppearance.Squiggle;
                switch (result.Severity)
                {
                    case ResultSeverity.Low:
                        item.Priority = TaskItemPriority.Low;
                        break;
                    case ResultSeverity.Normal:
                        item.Priority = TaskItemPriority.Normal;
                        break;
                    case ResultSeverity.High:
                        item.Priority = TaskItemPriority.High;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                item.Document = window.ProjectItem.Name;
                item.CustomInfo = this;
                service.Add(item);
            }
        }

        public static DesignPractices DesignPractices
        {
            get { return _practices; }
        }

    }
}
