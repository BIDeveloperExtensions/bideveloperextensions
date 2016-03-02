// Copyright (c) Microsoft Corporation.  All rights reserved. 

namespace PCDimNaturalizer
{
    partial class frmSQLFlattener
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSQLFlattener));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbDatabase = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbTable = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbID = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbPID = new System.Windows.Forms.ComboBox();
            this.btnNaturalize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnOptions = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(430, 29);
            this.label1.TabIndex = 0;
            this.label1.Text = "Choose a relational table to be naturalized and columns representing the ID and P" +
                "arent ID of the parent-child hierarchy.  ID columns must be of any integer type." +
                "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(63, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Server:";
            // 
            // txtServer
            // 
            this.txtServer.Enabled = false;
            this.txtServer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtServer.Location = new System.Drawing.Point(110, 57);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(326, 22);
            this.txtServer.TabIndex = 2;
            this.txtServer.Text = ".";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(46, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Database:";
            // 
            // cmbDatabase
            // 
            this.cmbDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDatabase.Enabled = false;
            this.cmbDatabase.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbDatabase.FormattingEnabled = true;
            this.cmbDatabase.Items.AddRange(new object[] {
            "AdventureWorksDW"});
            this.cmbDatabase.Location = new System.Drawing.Point(110, 88);
            this.cmbDatabase.Name = "cmbDatabase";
            this.cmbDatabase.Size = new System.Drawing.Size(326, 21);
            this.cmbDatabase.Sorted = true;
            this.cmbDatabase.TabIndex = 4;
            this.cmbDatabase.Enter += new System.EventHandler(this.cmbDatabase_Enter);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(25, 124);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Table or View:";
            // 
            // cmbTable
            // 
            this.cmbTable.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTable.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbTable.FormattingEnabled = true;
            this.cmbTable.Items.AddRange(new object[] {
            "[dbo].[DimEmployee]"});
            this.cmbTable.Location = new System.Drawing.Point(110, 121);
            this.cmbTable.Name = "cmbTable";
            this.cmbTable.Size = new System.Drawing.Size(326, 21);
            this.cmbTable.Sorted = true;
            this.cmbTable.TabIndex = 6;
            this.cmbTable.Enter += new System.EventHandler(this.cmbTable_Enter);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(40, 162);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "ID Column:";
            // 
            // cmbID
            // 
            this.cmbID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbID.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbID.FormattingEnabled = true;
            this.cmbID.Items.AddRange(new object[] {
            "EmployeeKey"});
            this.cmbID.Location = new System.Drawing.Point(111, 159);
            this.cmbID.Name = "cmbID";
            this.cmbID.Size = new System.Drawing.Size(325, 21);
            this.cmbID.Sorted = true;
            this.cmbID.TabIndex = 8;
            this.cmbID.Enter += new System.EventHandler(this.cmbID_Enter);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(47, 196);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Parent ID:";
            // 
            // cmbPID
            // 
            this.cmbPID.AutoCompleteCustomSource.AddRange(new string[] {
            "[ManagerID]"});
            this.cmbPID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPID.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbPID.FormattingEnabled = true;
            this.cmbPID.Items.AddRange(new object[] {
            "ParentEmployeeKey"});
            this.cmbPID.Location = new System.Drawing.Point(110, 193);
            this.cmbPID.Name = "cmbPID";
            this.cmbPID.Size = new System.Drawing.Size(326, 21);
            this.cmbPID.Sorted = true;
            this.cmbPID.TabIndex = 10;
            this.cmbPID.Enter += new System.EventHandler(this.cmbPID_Enter);
            // 
            // btnNaturalize
            // 
            this.btnNaturalize.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnNaturalize.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNaturalize.Location = new System.Drawing.Point(230, 232);
            this.btnNaturalize.Name = "btnNaturalize";
            this.btnNaturalize.Size = new System.Drawing.Size(96, 30);
            this.btnNaturalize.TabIndex = 11;
            this.btnNaturalize.Text = "Naturalize";
            this.btnNaturalize.UseVisualStyleBackColor = true;
            this.btnNaturalize.Click += new System.EventHandler(this.btnNaturalize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(340, 232);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 30);
            this.btnClose.TabIndex = 18;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOptions.Location = new System.Drawing.Point(19, 232);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(150, 30);
            this.btnOptions.TabIndex = 19;
            this.btnOptions.Text = "Change &Settings...";
            this.btnOptions.UseVisualStyleBackColor = true;
            this.btnOptions.Click += new System.EventHandler(this.btnOptions_Click);
            // 
            // frmSQLFlattener
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 282);
            this.Controls.Add(this.btnOptions);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnNaturalize);
            this.Controls.Add(this.cmbPID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbID);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbTable);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbDatabase);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmSQLFlattener";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parent Child Dimension Naturalizer (SQL Source)";
            this.Load += new System.EventHandler(this.frmSQLFlattener_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox txtServer;
        public System.Windows.Forms.ComboBox cmbDatabase;
        public System.Windows.Forms.ComboBox cmbTable;
        public System.Windows.Forms.ComboBox cmbID;
        public System.Windows.Forms.ComboBox cmbPID;
        public System.Windows.Forms.Button btnNaturalize;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnOptions;
    }
}

