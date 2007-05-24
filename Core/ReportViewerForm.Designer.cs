namespace BIDSHelper
{
    partial class ReportViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Microsoft.Reporting.WinForms.ReportDataSource reportDataSource1 = new Microsoft.Reporting.WinForms.ReportDataSource();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportViewerForm));
            this.ReportBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ReportViewerControl = new Microsoft.Reporting.WinForms.ReportViewer();
            ((System.ComponentModel.ISupportInitialize)(this.ReportBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // ReportBindingSource
            // 
            this.ReportBindingSource.DataMember = "DimensionUsage";
            // 
            // reportViewer1
            // 
            this.ReportViewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReportViewerControl.Location = new System.Drawing.Point(0, 0);
            this.ReportViewerControl.Name = "reportViewer1";
            this.ReportViewerControl.Size = new System.Drawing.Size(782, 461);
            this.ReportViewerControl.TabIndex = 0;
            // 
            // ReportViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 461);
            this.Controls.Add(this.ReportViewerControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReportViewerForm";
            this.Text = "PrinterFriendlyDimensionUsage";
            this.Load += new System.EventHandler(this.PrinterFriendlyDimensionUsage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ReportBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public Microsoft.Reporting.WinForms.ReportViewer ReportViewerControl;
        public System.Windows.Forms.BindingSource ReportBindingSource;
    }
}