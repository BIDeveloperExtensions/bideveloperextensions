namespace BIDSHelper.SSAS
{
    partial class SyncDescriptionsForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.listOtherProperties = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cmbDescriptionProperty = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkOverwriteExistingDescriptions = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(123, 305);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // listOtherProperties
            // 
            this.listOtherProperties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listOtherProperties.CheckOnClick = true;
            this.listOtherProperties.FormattingEnabled = true;
            this.listOtherProperties.Location = new System.Drawing.Point(13, 84);
            this.listOtherProperties.Name = "listOtherProperties";
            this.listOtherProperties.ScrollAlwaysVisible = true;
            this.listOtherProperties.Size = new System.Drawing.Size(266, 169);
            this.listOtherProperties.Sorted = true;
            this.listOtherProperties.TabIndex = 6;
            this.listOtherProperties.ThreeDCheckBoxes = true;
            this.listOtherProperties.SelectedIndexChanged += new System.EventHandler(this.listOtherProperties_SelectedIndexChanged);
            this.listOtherProperties.Click += new System.EventHandler(this.listOtherProperties_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(202, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Extended property to use as description...";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(204, 305);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cmbDescriptionProperty
            // 
            this.cmbDescriptionProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDescriptionProperty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDescriptionProperty.FormattingEnabled = true;
            this.cmbDescriptionProperty.Location = new System.Drawing.Point(16, 27);
            this.cmbDescriptionProperty.Name = "cmbDescriptionProperty";
            this.cmbDescriptionProperty.Size = new System.Drawing.Size(263, 21);
            this.cmbDescriptionProperty.TabIndex = 9;
            this.cmbDescriptionProperty.SelectedIndexChanged += new System.EventHandler(this.cmbDescriptionProperty_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(243, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Other extended properties to show in description...";
            // 
            // chkOverwriteExistingDescriptions
            // 
            this.chkOverwriteExistingDescriptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkOverwriteExistingDescriptions.AutoSize = true;
            this.chkOverwriteExistingDescriptions.Location = new System.Drawing.Point(12, 270);
            this.chkOverwriteExistingDescriptions.Name = "chkOverwriteExistingDescriptions";
            this.chkOverwriteExistingDescriptions.Size = new System.Drawing.Size(174, 17);
            this.chkOverwriteExistingDescriptions.TabIndex = 11;
            this.chkOverwriteExistingDescriptions.Text = "Overwrite existing descriptions?";
            this.chkOverwriteExistingDescriptions.UseVisualStyleBackColor = true;
            // 
            // SyncDescriptionsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 340);
            this.Controls.Add(this.chkOverwriteExistingDescriptions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbDescriptionProperty);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listOtherProperties);
            this.Controls.Add(this.btnOK);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(300, 300);
            this.Name = "SyncDescriptionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Properties To Use As Descriptions";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SyncDescriptionsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.CheckedListBox listOtherProperties;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox cmbDescriptionProperty;
        public System.Windows.Forms.CheckBox chkOverwriteExistingDescriptions;
    }
}