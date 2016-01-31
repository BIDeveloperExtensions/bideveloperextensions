﻿namespace BIDSHelper.SSIS.Biml
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
            this.selectionList = new BIDSHelper.Core.SelectionList();
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
            this.confirmationTextLabel.Text = "ConfirmationText";
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
            // selectionCheckedListBox
            // 
            this.selectionList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectionList.Location = new System.Drawing.Point(12, 63);
            this.selectionList.Name = "selectionCheckedListBox";
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
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.commitButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.confirmationTextLabel);
            this.Controls.Add(this.selectionList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MultipleSelectionConfirmationDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Confirm Overwritten Items";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label confirmationTextLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button commitButton;
        private System.Windows.Forms.Button helpButton;
        private BIDSHelper.Core.SelectionList selectionList;
    }
}
