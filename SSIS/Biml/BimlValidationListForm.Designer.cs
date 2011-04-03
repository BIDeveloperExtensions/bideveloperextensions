namespace BIDSHelper.SSIS.Biml
{
    partial class BimlValidationListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BimlValidationListForm));
            this.listViewValidationItems = new System.Windows.Forms.ListView();
            this.columnHeaderSeverity = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderDescription = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderRecommendation = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderLine = new System.Windows.Forms.ColumnHeader();
            this.columnHeaderColumn = new System.Windows.Forms.ColumnHeader();
            this.buttonClose = new System.Windows.Forms.Button();
            this.helpButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewValidationItems
            // 
            this.listViewValidationItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewValidationItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSeverity,
            this.columnHeaderDescription,
            this.columnHeaderRecommendation,
            this.columnHeaderLine,
            this.columnHeaderColumn});
            this.listViewValidationItems.Location = new System.Drawing.Point(12, 12);
            this.listViewValidationItems.Name = "listViewValidationItems";
            this.listViewValidationItems.Size = new System.Drawing.Size(635, 253);
            this.listViewValidationItems.TabIndex = 1;
            this.listViewValidationItems.UseCompatibleStateImageBehavior = false;
            this.listViewValidationItems.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderSeverity
            // 
            this.columnHeaderSeverity.Text = "Severity";
            // 
            // columnHeaderDescription
            // 
            this.columnHeaderDescription.Text = "Description";
            // 
            // columnHeaderRecommendation
            // 
            this.columnHeaderRecommendation.Text = "Recommendation";
            // 
            // columnHeaderLine
            // 
            this.columnHeaderLine.Text = "Line";
            // 
            // columnHeaderColumn
            // 
            this.columnHeaderColumn.Text = "Column";
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonClose.Location = new System.Drawing.Point(572, 271);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            // 
            // helpButton
            // 
            this.helpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.helpButton.Location = new System.Drawing.Point(13, 272);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(75, 23);
            this.helpButton.TabIndex = 3;
            this.helpButton.Text = "Help";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // BimlValidationListForm
            // 
            this.AcceptButton = this.buttonClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(659, 306);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.listViewValidationItems);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BimlValidationListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Biml Validation Items";
            this.Load += new System.EventHandler(this.BimlValidationListForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewValidationItems;
        private System.Windows.Forms.ColumnHeader columnHeaderSeverity;
        private System.Windows.Forms.ColumnHeader columnHeaderDescription;
        private System.Windows.Forms.ColumnHeader columnHeaderRecommendation;
        private System.Windows.Forms.ColumnHeader columnHeaderLine;
        private System.Windows.Forms.ColumnHeader columnHeaderColumn;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button helpButton;
    }
}