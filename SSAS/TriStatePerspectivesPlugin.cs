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
    public class TriStatePerspectivesPlugin : BIDSHelperPluginBase
    {
        private WindowEvents windowEvents;
        private const System.Reflection.BindingFlags getflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
        private System.Collections.Generic.List<string> windowHandlesFixedForPerspectives = new System.Collections.Generic.List<string>();
        private System.Collections.Generic.List<string> windowHandlesFixedForGridEvents = new System.Collections.Generic.List<string>();

        public TriStatePerspectivesPlugin(Connect con, DTE2 appObject, AddIn addinInstance)
            : base(con, appObject, addinInstance)
        {
            windowEvents = appObject.Events.get_WindowEvents(null);
            windowEvents.WindowActivated += new _dispWindowEvents_WindowActivatedEventHandler(windowEvents_WindowActivated);
            windowEvents.WindowCreated += new _dispWindowEvents_WindowCreatedEventHandler(windowEvents_WindowCreated);
        }

        void windowEvents_WindowCreated(Window Window)
        {
            windowEvents_WindowActivated(Window, null);
        }

        void windowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            try
            {
                if (GotFocus == null) return;
                IDesignerHost designer = (IDesignerHost)GotFocus.Object;
                if (designer == null) return;
                ProjectItem pi = GotFocus.ProjectItem;
                if ((pi==null) || (!(pi.Object is Cube))) return;
                EditorWindow win = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
                VsStyleToolBar toolbar = (VsStyleToolBar)win.SelectedView.GetType().InvokeMember("ToolBar", getflags, null, win.SelectedView, null);
                Cube cube = (Cube)pi.Object;

                IntPtr ptr = win.Handle;
                string sHandle = ptr.ToInt64().ToString();

                if (!windowHandlesFixedForPerspectives.Contains(sHandle))
                {
                    windowHandlesFixedForPerspectives.Add(sHandle);
                    win.ActiveViewChanged += new EventHandler(win_ActiveViewChanged);
                }

                if (win.SelectedView.Caption == "Perspectives")
                {
                    Control perspectiveBuilder = (Control)win.SelectedView.GetType().InvokeMember("ViewControl", getflags, null, win.SelectedView, null); //Microsoft.AnalysisServices.Design.PerspectivesBuilder
                    Control grid = perspectiveBuilder.Controls[0]; //Microsoft.SqlServer.Management.UI.Grid.DlgGridControl

                    if (!windowHandlesFixedForGridEvents.Contains(sHandle))
                    {
                        grid.MouseClick += new MouseEventHandler(grid_MouseClick);
                        grid.KeyPress += new KeyPressEventHandler(grid_KeyPress);
                        windowHandlesFixedForGridEvents.Add(sHandle);
                    }
                    
                    System.Reflection.BindingFlags getpropertyflags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                    object dlgStorage = grid.GetType().BaseType.InvokeMember("DlgStorage", getpropertyflags, null, grid, null); //Microsoft.SqlServer.Management.UI.Grid.IDlgStorage
                    object storage = dlgStorage.GetType().InvokeMember("Storage", getpropertyflags, null, dlgStorage, null); //Microsoft.SqlServer.Management.UI.Grid.MemDataStorage

                    System.Reflection.BindingFlags getfieldflags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance;
                    ArrayList rows = (ArrayList)storage.GetType().BaseType.InvokeMember("m_arrRows", getfieldflags, null, storage, new object[] { });
                    //ArrayList columns = (ArrayList)storage.GetType().BaseType.InvokeMember("m_arrColumns", getfieldflags, null, storage, new object[] { });
                    object[] perspectivesColumns = (object[])rows[0];

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
                    }
                    grid.Refresh();
                }
            }
            catch { }
        }

        void grid_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch { }
        }

        void grid_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
            }
            catch { }
        }

        void win_ActiveViewChanged(object sender, EventArgs e)
        {
            try
            {
                windowEvents_WindowActivated(this.ApplicationObject.ActiveWindow, null);
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
            get { return ""; }
        }

        public override string MenuName
        {
            get { return ""; } //no need to have a menu command
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