using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.AnalysisServices;

namespace PCDimNaturalizer
{
    partial class frmProgress : Form
    {
        private Panel panel1;
        public PictureBox pictureBox1;
        private Label label1;
        public Button btnClose;
        private Bitmap waitImage;
        public TextBox txtStatus;

        Thread thrdNat = null;
        private VScrollBar udStatus;
        private System.Drawing.Printing.PrintDocument printDocument1; // Naturalizer thread...
        IntPtr hdlCloseButton;
        delegate void NaturalizeThreadFunc(object MaxLevels);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case PCDimNaturalizer.WM_USER1:
                    txtStatus.Text = Marshal.PtrToStringAnsi(m.LParam);
                    if (txtStatus.Width < TextRenderer.MeasureText(txtStatus.Text, txtStatus.Font).Width || txtStatus.Lines.Length > 1)
                        udStatus.Visible = true;
                    else
                        udStatus.Visible = false;
                    btnClose.Select();
                    break;
                case PCDimNaturalizer.WM_USER2:
                    pictureBox1.Paint -= pictureBox1_Paint;
                    ImageAnimator.StopAnimate(waitImage, OnFrameChanged);
                    pictureBox1.Image = Bitmap.FromHbitmap(m.LParam);
                    pictureBox1.Refresh();
                    pictureBox1.Invalidate();
                    btnClose.Select();
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    
        public frmProgress()
        {
            InitializeComponent();
        }

        private void OnFrameChanged(object o, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();
            Bitmap waitImgTmp = new Bitmap(waitImage, waitImage.Width, waitImage.Height);
            waitImgTmp.MakeTransparent(Color.White);
            e.Graphics.Clear(pictureBox1.BackColor);
            e.Graphics.DrawImage(waitImgTmp, new Point(0, 0));
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProgress));
            this.panel1 = new System.Windows.Forms.Panel();
            this.udStatus = new System.Windows.Forms.VScrollBar();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.udStatus);
            this.panel1.Controls.Add(this.txtStatus);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(12, 27);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(419, 32);
            this.panel1.TabIndex = 0;
            // 
            // udStatus
            // 
            this.udStatus.Location = new System.Drawing.Point(399, -1);
            this.udStatus.Name = "udStatus";
            this.udStatus.Size = new System.Drawing.Size(15, 30);
            this.udStatus.TabIndex = 2;
            this.udStatus.Visible = false;
            this.udStatus.Scroll += new System.Windows.Forms.ScrollEventHandler(this.udStatus_Scroll);
            // 
            // txtStatus
            // 
            this.txtStatus.AcceptsTab = true;
            this.txtStatus.BackColor = System.Drawing.SystemColors.Control;
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStatus.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.Location = new System.Drawing.Point(26, 6);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(375, 18);
            this.txtStatus.TabIndex = 1;
            this.txtStatus.WordWrap = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = BIDSHelper.Resources.Common.ProcessProgress;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(25, 28);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Status:";
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(173, 66);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Stop";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click_1);
            // 
            // frmProgress
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(443, 108);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmProgress";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Parent Child Dimension Naturalizer Progress...";
            this.Load += new System.EventHandler(this.frmProgress_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmProgress_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetCloseButtonText(string Text)
        {
            if (btnClose.InvokeRequired)
                this.btnClose.BeginInvoke(new MethodInvoker(delegate() { SetCloseButtonText(Text); }));
            else
            {
                btnClose.Text = Text;
                btnClose.Select();
            }
        }

        private void NaturalizeCompletion(IAsyncResult ar)
        {
            SetCloseButtonText("Close");
        }

        private void frmProgress_Load(object sender, EventArgs e)
        {
            try
            {
                hdlCloseButton = btnClose.Handle; // for processing update messages to change the close button's text from the async callback of the naturalization function
                waitImage = BIDSHelper.Resources.Common.ProcessProgress as Bitmap;
                ImageAnimator.Animate(waitImage, new EventHandler(OnFrameChanged));
                pictureBox1.BackColor = this.BackColor;
                txtStatus.Text = "Starting...";
                int MinLevels;

                PCDimNaturalizer nat = null;
                if (Program.ASFlattener != null)
                {
                    nat = new ASPCDimNaturalizer(Program.ASFlattener.srv, Program.ASFlattener.db, Program.ASFlattener.dim, Program.ASFlattener.ActionLevel);
                    MinLevels = Program.ASFlattener.MinLevels;
                    if (Program.ASFlattener.AddAllNonPCHierarchies)
                    {
                        foreach (Hierarchy hier in Program.ASFlattener.dim.Hierarchies)
                            ((ASPCDimNaturalizer)nat).NonPCHierarchiesToInclude.Add(hier.Name);
                        foreach (DimensionAttribute attr in Program.ASFlattener.dim.Attributes)
                            ((ASPCDimNaturalizer)nat).NonPCHierarchiesToInclude.Add(attr.Name);
                    }
                    else
                        ((ASPCDimNaturalizer)nat).NonPCHierarchiesToInclude = Program.ASFlattener.NonPCHierarchiesToInclude;
                    if (Program.ASFlattener.AddAllPCAttributes)
                    {
                        foreach (DimensionAttribute attr in Program.ASFlattener.dim.Attributes)
                            if (attr.Usage == AttributeUsage.Regular) ((ASPCDimNaturalizer)nat).PCAttributesToInclude.Add(attr.Name);
                    }
                    else
                        ((ASPCDimNaturalizer)nat).PCAttributesToInclude = Program.ASFlattener.PCAttributesToInclude;
                }
                else // if it is frmSQLFlattener, since there are only two types of calling forms for this
                {
                    nat = new SQLPCDimNaturalizer(Program.SQLFlattener.Conn, Program.SQLFlattener.cmbTable.Text, Program.SQLFlattener.cmbID.Text, Program.SQLFlattener.cmbPID.Text, Program.SQLFlattener.MinLevels);
                    MinLevels = Program.SQLFlattener.MinLevels;
                    if (Program.SQLFlattener.AddAllAttributesPC)
                    {
                        foreach (DataRow dr in Program.SQLFlattener.Columns)
                            if (Program.SQLFlattener.cmbID.Text.Trim() != dr.ItemArray[dr.Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim() &&
                                Program.SQLFlattener.cmbPID.Text.Trim() != dr.ItemArray[dr.Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim())    
                                ((SQLPCDimNaturalizer)nat).SQLColsASPCAttributes.Add(dr.ItemArray[dr.Table.Columns.IndexOf("COLUMN_NAME")].ToString());
                    }
                    else
                        ((SQLPCDimNaturalizer)nat).SQLColsASPCAttributes = Program.SQLFlattener.AttributesPC;
                    if (Program.SQLFlattener.AddAllAttributesNatural)
                    {
                        foreach (DataRow dr in Program.SQLFlattener.Columns)
                            if (Program.SQLFlattener.cmbID.Text.Trim() != dr.ItemArray[dr.Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim() &&
                                Program.SQLFlattener.cmbPID.Text.Trim() != dr.ItemArray[dr.Table.Columns.IndexOf("COLUMN_NAME")].ToString().Trim())
                                ((SQLPCDimNaturalizer)nat).SQLColsAsNonPCAttributes.Add(dr.ItemArray[dr.Table.Columns.IndexOf("COLUMN_NAME")].ToString());
                    }
                    else
                        ((SQLPCDimNaturalizer)nat).SQLColsAsNonPCAttributes = Program.SQLFlattener.AttributesNatural;
                }
                nat.SourceWindowHandle = this.Handle;
                NaturalizeThreadFunc natThreadFunc = new NaturalizeThreadFunc(nat.Naturalize);
                AsyncCallback cbNat = new AsyncCallback(NaturalizeCompletion);
                IAsyncResult ar = natThreadFunc.BeginInvoke(MinLevels, cbNat, null);
            }
            catch (Exception exc)
            {
                pictureBox1.Paint -= pictureBox1_Paint;
                ImageAnimator.StopAnimate(waitImage, OnFrameChanged);
                pictureBox1.Image = BIDSHelper.Resources.Common.ProcessError;
                pictureBox1.Refresh();
                pictureBox1.Invalidate();
                txtStatus.Text = "Error initializing naturalizer:  " + exc.ToString();
            }
        }

        private void frmProgress_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (thrdNat != null && thrdNat.ThreadState != ThreadState.Stopped)
                thrdNat.Abort();
            //this.Owner.Enabled = true;
        }

        private void udStatus_Scroll(object sender, ScrollEventArgs e)
        {
            txtStatus.Select();
            if (e.NewValue < e.OldValue)
                SendKeys.Send("{UP}");
            else
                SendKeys.Send("{DOWN}");
        }
    }
}
