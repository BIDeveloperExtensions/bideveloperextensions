namespace AggManager
{
    partial class QueryLogForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryLogForm));
            this.checkBoxConnction = new System.Windows.Forms.CheckBox();
            this.buttonConnectToSQL = new System.Windows.Forms.Button();
            this.dataGrid1 = new System.Windows.Forms.DataGrid();
            this.textBoxSQLConnectionString = new System.Windows.Forms.TextBox();
            this.textBoxSQLQuery = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.sqlDataAdapter1 = new System.Data.SqlClient.SqlDataAdapter();
            this.sqlInsertCommand1 = new System.Data.SqlClient.SqlCommand();
            this.sqlConnection1 = new System.Data.SqlClient.SqlConnection();
            this.sqlSelectCommand1 = new System.Data.SqlClient.SqlCommand();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxNewAggDesign = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxAggregationPrefix = new System.Windows.Forms.TextBox();
            this.txtServerNote = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBoxConnction
            // 
            this.checkBoxConnction.AutoSize = true;
            this.checkBoxConnction.Checked = true;
            this.checkBoxConnction.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxConnction.Location = new System.Drawing.Point(149, 37);
            this.checkBoxConnction.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxConnction.Name = "checkBoxConnction";
            this.checkBoxConnction.Size = new System.Drawing.Size(184, 17);
            this.checkBoxConnction.TabIndex = 36;
            this.checkBoxConnction.Text = "Use Query Log Connection String";
            this.checkBoxConnction.UseVisualStyleBackColor = true;
            this.checkBoxConnction.CheckedChanged += new System.EventHandler(this.checkBoxConnction_CheckedChanged);
            // 
            // buttonConnectToSQL
            // 
            this.buttonConnectToSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnectToSQL.Location = new System.Drawing.Point(695, 82);
            this.buttonConnectToSQL.Margin = new System.Windows.Forms.Padding(2);
            this.buttonConnectToSQL.Name = "buttonConnectToSQL";
            this.buttonConnectToSQL.Size = new System.Drawing.Size(85, 56);
            this.buttonConnectToSQL.TabIndex = 33;
            this.buttonConnectToSQL.Text = "Execute SQL";
            this.buttonConnectToSQL.Click += new System.EventHandler(this.buttonConnectToSQL_Click);
            // 
            // dataGrid1
            // 
            this.dataGrid1.AlternatingBackColor = System.Drawing.Color.LightGray;
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BackColor = System.Drawing.Color.DarkGray;
            this.dataGrid1.CaptionBackColor = System.Drawing.Color.White;
            this.dataGrid1.CaptionFont = new System.Drawing.Font("Verdana", 10F);
            this.dataGrid1.CaptionForeColor = System.Drawing.Color.Navy;
            this.dataGrid1.CaptionVisible = false;
            this.dataGrid1.DataMember = "";
            this.dataGrid1.ForeColor = System.Drawing.Color.Black;
            this.dataGrid1.GridLineColor = System.Drawing.Color.Black;
            this.dataGrid1.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
            this.dataGrid1.HeaderBackColor = System.Drawing.Color.Silver;
            this.dataGrid1.HeaderForeColor = System.Drawing.Color.Black;
            this.dataGrid1.LinkColor = System.Drawing.Color.Navy;
            this.dataGrid1.Location = new System.Drawing.Point(10, 145);
            this.dataGrid1.Margin = new System.Windows.Forms.Padding(2);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.ParentRowsBackColor = System.Drawing.Color.White;
            this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.Black;
            this.dataGrid1.ReadOnly = true;
            this.dataGrid1.SelectionBackColor = System.Drawing.Color.Navy;
            this.dataGrid1.SelectionForeColor = System.Drawing.Color.White;
            this.dataGrid1.Size = new System.Drawing.Size(770, 230);
            this.dataGrid1.TabIndex = 28;
            // 
            // textBoxSQLConnectionString
            // 
            this.textBoxSQLConnectionString.AcceptsReturn = true;
            this.textBoxSQLConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSQLConnectionString.Enabled = false;
            this.textBoxSQLConnectionString.Location = new System.Drawing.Point(149, 11);
            this.textBoxSQLConnectionString.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSQLConnectionString.Name = "textBoxSQLConnectionString";
            this.textBoxSQLConnectionString.Size = new System.Drawing.Size(542, 20);
            this.textBoxSQLConnectionString.TabIndex = 29;
            // 
            // textBoxSQLQuery
            // 
            this.textBoxSQLQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSQLQuery.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxSQLQuery.Location = new System.Drawing.Point(148, 82);
            this.textBoxSQLQuery.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxSQLQuery.Multiline = true;
            this.textBoxSQLQuery.Name = "textBoxSQLQuery";
            this.textBoxSQLQuery.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxSQLQuery.Size = new System.Drawing.Size(543, 56);
            this.textBoxSQLQuery.TabIndex = 32;
            this.textBoxSQLQuery.Text = "select distinct dataset from OLAPQueryLog";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 15);
            this.label1.TabIndex = 30;
            this.label1.Text = "SQL Server Connection";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(44, 87);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 15);
            this.label5.TabIndex = 31;
            this.label5.Text = "SQL Query";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(597, 379);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(94, 31);
            this.buttonOK.TabIndex = 38;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(695, 379);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(85, 31);
            this.buttonCancel.TabIndex = 37;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // sqlDataAdapter1
            // 
            this.sqlDataAdapter1.InsertCommand = this.sqlInsertCommand1;
            this.sqlDataAdapter1.SelectCommand = this.sqlSelectCommand1;
            this.sqlDataAdapter1.TableMappings.AddRange(new System.Data.Common.DataTableMapping[] {
            new System.Data.Common.DataTableMapping("Table", "OlapQueryLog", new System.Data.Common.DataColumnMapping[] {
                        new System.Data.Common.DataColumnMapping("MSOLAP_Database", "MSOLAP_Database"),
                        new System.Data.Common.DataColumnMapping("MSOLAP_Cube", "MSOLAP_Cube"),
                        new System.Data.Common.DataColumnMapping("MSOLAP_MeasureGroup", "MSOLAP_MeasureGroup"),
                        new System.Data.Common.DataColumnMapping("MSOLAP_User", "MSOLAP_User"),
                        new System.Data.Common.DataColumnMapping("Dataset", "Dataset"),
                        new System.Data.Common.DataColumnMapping("StartTime", "StartTime"),
                        new System.Data.Common.DataColumnMapping("Duration", "Duration")})});
            // 
            // sqlInsertCommand1
            // 
            this.sqlInsertCommand1.CommandText = resources.GetString("sqlInsertCommand1.CommandText");
            this.sqlInsertCommand1.Connection = this.sqlConnection1;
            this.sqlInsertCommand1.Parameters.AddRange(new System.Data.SqlClient.SqlParameter[] {
            new System.Data.SqlClient.SqlParameter("@MSOLAP_Database", System.Data.SqlDbType.NVarChar, 255, "MSOLAP_Database"),
            new System.Data.SqlClient.SqlParameter("@MSOLAP_Cube", System.Data.SqlDbType.NVarChar, 255, "MSOLAP_Cube"),
            new System.Data.SqlClient.SqlParameter("@MSOLAP_MeasureGroup", System.Data.SqlDbType.NVarChar, 255, "MSOLAP_MeasureGroup"),
            new System.Data.SqlClient.SqlParameter("@MSOLAP_User", System.Data.SqlDbType.NVarChar, 255, "MSOLAP_User"),
            new System.Data.SqlClient.SqlParameter("@Dataset", System.Data.SqlDbType.NVarChar, 255, "Dataset"),
            new System.Data.SqlClient.SqlParameter("@StartTime", System.Data.SqlDbType.DateTime, 8, "StartTime"),
            new System.Data.SqlClient.SqlParameter("@Duration", System.Data.SqlDbType.Int, 4, "Duration")});
            // 
            // sqlConnection1
            // 
            this.sqlConnection1.ConnectionString = "data source=edwardm0;initial catalog=Northwind;integrated security=SSPI;persist s" +
                "ecurity info=False;workstation id=EDWARDM1;packet size=4096";
            this.sqlConnection1.FireInfoMessageEventOnUserErrors = false;
            // 
            // sqlSelectCommand1
            // 
            this.sqlSelectCommand1.CommandText = "SELECT MSOLAP_Database, MSOLAP_Cube, MSOLAP_MeasureGroup, MSOLAP_User, Dataset, S" +
                "tartTime, Duration FROM OlapQueryLog";
            this.sqlSelectCommand1.Connection = this.sqlConnection1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 61);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 39;
            this.label2.Text = "Aggregation Design Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // textBoxNewAggDesign
            // 
            this.textBoxNewAggDesign.Location = new System.Drawing.Point(148, 58);
            this.textBoxNewAggDesign.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxNewAggDesign.Name = "textBoxNewAggDesign";
            this.textBoxNewAggDesign.Size = new System.Drawing.Size(185, 20);
            this.textBoxNewAggDesign.TabIndex = 40;
            this.textBoxNewAggDesign.Text = "New Aggregation Design";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(363, 61);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 41;
            this.label3.Text = "Aggregation Prefix";
            // 
            // textBoxAggregationPrefix
            // 
            this.textBoxAggregationPrefix.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAggregationPrefix.Location = new System.Drawing.Point(458, 58);
            this.textBoxAggregationPrefix.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxAggregationPrefix.Name = "textBoxAggregationPrefix";
            this.textBoxAggregationPrefix.Size = new System.Drawing.Size(233, 20);
            this.textBoxAggregationPrefix.TabIndex = 42;
            this.textBoxAggregationPrefix.Text = "QueryLogAgg_";
            // 
            // txtServerNote
            // 
            this.txtServerNote.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerNote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtServerNote.Location = new System.Drawing.Point(10, 379);
            this.txtServerNote.Multiline = true;
            this.txtServerNote.Name = "txtServerNote";
            this.txtServerNote.ReadOnly = true;
            this.txtServerNote.Size = new System.Drawing.Size(582, 37);
            this.txtServerNote.TabIndex = 43;
            // 
            // QueryLogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 421);
            this.Controls.Add(this.txtServerNote);
            this.Controls.Add(this.textBoxAggregationPrefix);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxNewAggDesign);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.checkBoxConnction);
            this.Controls.Add(this.buttonConnectToSQL);
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.textBoxSQLConnectionString);
            this.Controls.Add(this.textBoxSQLQuery);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label5);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "QueryLogForm";
            this.Text = "Add Aggregations From QueryLog";
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxConnction;
        private System.Windows.Forms.Button buttonConnectToSQL;
        private System.Windows.Forms.DataGrid dataGrid1;
        private System.Windows.Forms.TextBox textBoxSQLConnectionString;
        private System.Windows.Forms.TextBox textBoxSQLQuery;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Data.SqlClient.SqlDataAdapter sqlDataAdapter1;
        private System.Data.SqlClient.SqlCommand sqlInsertCommand1;
        private System.Data.SqlClient.SqlConnection sqlConnection1;
        private System.Data.SqlClient.SqlCommand sqlSelectCommand1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxNewAggDesign;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxAggregationPrefix;
        private System.Windows.Forms.TextBox txtServerNote;
    }
}