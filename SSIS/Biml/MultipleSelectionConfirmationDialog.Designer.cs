namespace BIDSHelper.SSIS.Biml
{
    partial class MultipleSelectionConfirmationDialog
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultipleSelectionConfirmationDialog));
            this.confirmationTextLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.commitButton = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.warningLabelImage = new System.Windows.Forms.Label();
            this.panelWarning = new System.Windows.Forms.Panel();
            this.WarningTextLabel = new System.Windows.Forms.Label();
            this.selectionList = new BIDSHelper.Core.SelectionList();
            this.panelWarning.SuspendLayout();
            this.SuspendLayout();
            // 
            // confirmationTextLabel
            // 
            this.confirmationTextLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.confirmationTextLabel.Location = new System.Drawing.Point(12, 9);
            this.confirmationTextLabel.Name = "confirmationTextLabel";
            this.confirmationTextLabel.Size = new System.Drawing.Size(625, 28);
            this.confirmationTextLabel.TabIndex = 1;
            this.confirmationTextLabel.Text = "Confirmation text label";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(562, 493);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // commitButton
            // 
            this.commitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.commitButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.commitButton.Location = new System.Drawing.Point(481, 493);
            this.commitButton.Name = "commitButton";
            this.commitButton.Size = new System.Drawing.Size(75, 23);
            this.commitButton.TabIndex = 4;
            this.commitButton.Text = "Commit";
            this.commitButton.UseVisualStyleBackColor = true;
            this.commitButton.Click += new System.EventHandler(this.commitButton_Click);
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.helpButton.Location = new System.Drawing.Point(12, 493);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 5;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // warningLabelImage
            // 
            this.warningLabelImage.Dock = System.Windows.Forms.DockStyle.Left;
            this.warningLabelImage.Image = global::BIDSHelper.Resources.Common.ProcessError;
            this.warningLabelImage.Location = new System.Drawing.Point(0, 0);
            this.warningLabelImage.Margin = new System.Windows.Forms.Padding(0);
            this.warningLabelImage.Name = "warningLabelImage";
            this.warningLabelImage.Size = new System.Drawing.Size(24, 16);
            this.warningLabelImage.TabIndex = 6;
            // 
            // panelWarning
            // 
            this.panelWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelWarning.Controls.Add(this.WarningTextLabel);
            this.panelWarning.Controls.Add(this.warningLabelImage);
            this.panelWarning.Location = new System.Drawing.Point(15, 41);
            this.panelWarning.Name = "panelWarning";
            this.panelWarning.Size = new System.Drawing.Size(622, 16);
            this.panelWarning.TabIndex = 7;
            // 
            // WarningTextLabel
            // 
            this.WarningTextLabel.AutoEllipsis = true;
            this.WarningTextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WarningTextLabel.Location = new System.Drawing.Point(24, 0);
            this.WarningTextLabel.Name = "WarningTextLabel";
            this.WarningTextLabel.Size = new System.Drawing.Size(598, 16);
            this.WarningTextLabel.TabIndex = 7;
            this.WarningTextLabel.Text = "Overwrite may fail for one or more read-only files. Please see the italic entries" +
    " in the list below.";
            this.WarningTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // selectionList
            // 
            this.selectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectionList.Location = new System.Drawing.Point(12, 63);
            this.selectionList.Name = "selectionList";
            this.selectionList.SelectionEnabled = true;
            this.selectionList.Size = new System.Drawing.Size(625, 424);
            this.selectionList.TabIndex = 0;
            // 
            // MultipleSelectionConfirmationDialog
            // 
            this.AcceptButton = this.commitButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(649, 528);
            this.Controls.Add(this.panelWarning);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.commitButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.confirmationTextLabel);
            this.Controls.Add(this.selectionList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MultipleSelectionConfirmationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Confirm Overwritten Items";
            this.panelWarning.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label confirmationTextLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button commitButton;
        private System.Windows.Forms.Button helpButton;
        private BIDSHelper.Core.SelectionList selectionList;
        private System.Windows.Forms.Label warningLabelImage;
        private System.Windows.Forms.Panel panelWarning;
        private System.Windows.Forms.Label WarningTextLabel;
    }
}
