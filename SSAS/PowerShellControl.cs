using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Microsoft.AnalysisServices;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.DataWarehouse.Design;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;

namespace BIDSHelper.SSAS
{


    public partial class PowerShellControl : UserControl, IDisposable
    {
        bool isCancelled = false;

        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd, uint wMsg, UIntPtr wParam, IntPtr lParam);
        UIntPtr SB_BOTTOM = (UIntPtr)7;
        UIntPtr SB_ENDSCROLL = (UIntPtr)8;
        const uint WM_VSCROLL = (uint)0x115;

        private Database currentDb ;
        private Runspace myRunspace;
                
        public void SetFont(string fontFamilyName, float fontSize)
        {
            // read the editor font and use that for the powershell control
            Font _font = new Font(new FontFamily(fontFamilyName), fontSize);
            this.txtPSScript.Font = _font;
            this.rtbOutput.Font = _font;
        }
                
        public PowerShellControl()
        {
            InitializeComponent();
        }
        
        public Database CurrentDB
        {
            get {return currentDb;}
            set 
            { 
                currentDb = value;
                rtbOutput.AppendText("The variable $CurrentDB has been added to the session\nwith a reference to the database for this project (" + CurrentDB.Name + ")\n");
            }
        }

        private EditorWindow win;
        public EditorWindow EditWindow
        {
            set { win = value; }
            get { return win; }
        }

        private Windows2 wins;
        public Windows2 ToolWindows
        {
            set { wins = value;
            foreach (Window w in wins)
            {
                IDesignerHost designer = w.Object as IDesignerHost;
                if (designer == null) continue;
                EditorWindow win = designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator)) as EditorWindow;
                if (win != null)
                { this.EditWindow = win; }
            }
            }
            get { return wins; }
        }

        private void tsbRun_Click(object sender, EventArgs e)
        {
            String mPsScript;
            int mWidth = 150;

            if (this.txtPSScript.SelectedText.Length > 0)
            {
                mPsScript = this.txtPSScript.SelectedText;
            }
            else
            {
                mPsScript = this.txtPSScript.Text;
            }
        
            // maintain a single runspace for the life of the window
            if (myRunspace == null)
            {
                myRunspace = RunspaceFactory.CreateRunspace();
                myRunspace.ApartmentState = System.Threading.ApartmentState.STA;
                myRunspace.ThreadOptions = PSThreadOptions.ReuseThread;
                myRunspace.Open();
                // Add variables into the current Powershell session
                myRunspace.SessionStateProxy.SetVariable("CurrentDB", this.CurrentDB);
            }
            Pipeline psPipeline = null;
            try
            {
                psPipeline = myRunspace.CreatePipeline();

                psPipeline.Commands.AddScript(mPsScript);
                System.Management.Automation.Runspaces.Command outString = new System.Management.Automation.Runspaces.Command("out-string");
                outString.Parameters.Add("width", mWidth);
                psPipeline.Commands.Add(outString);
            }
            catch (Exception ex)
            {
                rtbOutput.AppendText("\nERROR: " + ex.Message + "\n");
            }

            try
                {



                    System.Collections.ObjectModel.Collection<PSObject> output = RunPowerShell(psPipeline);
                    //System.Collections.ObjectModel.Collection<PSObject> output = psPipeline.Invoke();
                 
                
                foreach (PSObject pso in output)
                    {
                        if (isCancelled) break;
                        rtbOutput.AppendText(pso.ToString());
                    }
                }
            catch (Exception ex)
                {
                rtbOutput.AppendText("\nERROR: " + ex.Message);
                }
            finally
                {
                //myRunspace.Close();
                    ScrollToBottom();
                }
        }



        public  System.Collections.ObjectModel.Collection<PSObject> RunPowerShell(Pipeline p)
        {
            System.Collections.ObjectModel.Collection<PSObject> output = null;
            //IDesignerHost designer = this as IDesignerHost;
            //EditorWindow editorWin = (EditorWindow)designer.GetService(typeof(Microsoft.DataWarehouse.ComponentModel.IComponentNavigator));
            if (this.EditWindow.InvokeRequired)
            {
                IAsyncResult r = EditWindow.BeginInvoke(new MethodInvoker(delegate() {output = p.Invoke(); }));
                r.AsyncWaitHandle.WaitOne();
            }
            else
            {
                //do the real work here
                output = p.Invoke();
            }
            return output;
        }


        public void ScrollToBottom()
        {
            SendMessage(rtbOutput.Handle, WM_VSCROLL, SB_BOTTOM, (IntPtr)(-1));
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            if (myRunspace != null)
            {
                myRunspace.Close();
            }
        }

        #endregion

        private void tsbCancel_Click(object sender, EventArgs e)
        {
            isCancelled = true;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            DialogResult res = this.openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                this.txtPSScript.Text = System.IO.File.ReadAllText(this.openFileDialog1.FileName);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.DefaultExt = "ps1";
            this.saveFileDialog1.Filter = "Powershell Files|*.ps1|All Files|*.*";
            DialogResult res = this.saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                System.IO.File.WriteAllText(this.saveFileDialog1.FileName, this.txtPSScript.Text);
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.saveFileDialog1.DefaultExt = "txt";
            this.saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";
            DialogResult res = this.saveFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                System.IO.File.WriteAllText(this.saveFileDialog1.FileName, this.rtbOutput.Text);
            }
        }
    }

}
