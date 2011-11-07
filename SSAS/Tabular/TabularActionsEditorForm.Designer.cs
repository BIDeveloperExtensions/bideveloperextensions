namespace BIDSHelper.SSAS
{
    partial class TabularActionsEditorForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewDrillthroughColumns = new System.Windows.Forms.DataGridView();
            this.DrillthroughDataGridCubeDimension = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.DrillthroughDataGridAttribute = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.drillthroughColumnBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.cmbAction = new System.Windows.Forms.ComboBox();
            this.lblAction = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbActionType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.txtCaption = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkCaptionIsMdx = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbTargetType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbTarget = new System.Windows.Forms.ComboBox();
            this.lblChangingLabel = new System.Windows.Forms.Label();
            this.txtCondition = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtExpression = new System.Windows.Forms.TextBox();
            this.listPerspectives = new System.Windows.Forms.CheckedListBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnDrillthroughColumnMoveUp = new System.Windows.Forms.Button();
            this.btnDrillthroughColumnMoveDown = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbInvocation = new System.Windows.Forms.ComboBox();
            this.lblDefault = new System.Windows.Forms.Label();
            this.cmbDefault = new System.Windows.Forms.ComboBox();
            this.txtMaxRows = new System.Windows.Forms.TextBox();
            this.lblMaxRows = new System.Windows.Forms.Label();
            this.txtReportServer = new System.Windows.Forms.TextBox();
            this.dataGridViewReportParameters = new System.Windows.Forms.DataGridView();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.reportParameterBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.linkHelp = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDrillthroughColumns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.drillthroughColumnBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReportParameters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.reportParameterBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewDrillthroughColumns
            // 
            this.dataGridViewDrillthroughColumns.AllowUserToResizeRows = false;
            this.dataGridViewDrillthroughColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewDrillthroughColumns.AutoGenerateColumns = false;
            this.dataGridViewDrillthroughColumns.CausesValidation = false;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDrillthroughColumns.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dataGridViewDrillthroughColumns.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewDrillthroughColumns.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DrillthroughDataGridCubeDimension,
            this.DrillthroughDataGridAttribute});
            this.dataGridViewDrillthroughColumns.DataSource = this.drillthroughColumnBindingSource;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewDrillthroughColumns.DefaultCellStyle = dataGridViewCellStyle8;
            this.dataGridViewDrillthroughColumns.Location = new System.Drawing.Point(123, 269);
            this.dataGridViewDrillthroughColumns.Name = "dataGridViewDrillthroughColumns";
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle9.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle9.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewDrillthroughColumns.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
            this.dataGridViewDrillthroughColumns.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewDrillthroughColumns.ShowEditingIcon = false;
            this.dataGridViewDrillthroughColumns.Size = new System.Drawing.Size(432, 99);
            this.dataGridViewDrillthroughColumns.TabIndex = 8;
            this.dataGridViewDrillthroughColumns.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridView1_EditingControlShowing);
            // 
            // DrillthroughDataGridCubeDimension
            // 
            this.DrillthroughDataGridCubeDimension.DataPropertyName = "CubeDimension";
            this.DrillthroughDataGridCubeDimension.HeaderText = "Table";
            this.DrillthroughDataGridCubeDimension.MinimumWidth = 100;
            this.DrillthroughDataGridCubeDimension.Name = "DrillthroughDataGridCubeDimension";
            this.DrillthroughDataGridCubeDimension.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DrillthroughDataGridCubeDimension.Sorted = true;
            this.DrillthroughDataGridCubeDimension.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.DrillthroughDataGridCubeDimension.Width = 180;
            // 
            // DrillthroughDataGridAttribute
            // 
            this.DrillthroughDataGridAttribute.DataPropertyName = "Attribute";
            this.DrillthroughDataGridAttribute.HeaderText = "Column";
            this.DrillthroughDataGridAttribute.MinimumWidth = 100;
            this.DrillthroughDataGridAttribute.Name = "DrillthroughDataGridAttribute";
            this.DrillthroughDataGridAttribute.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DrillthroughDataGridAttribute.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.DrillthroughDataGridAttribute.Width = 180;
            // 
            // drillthroughColumnBindingSource
            // 
            this.drillthroughColumnBindingSource.DataSource = typeof(BIDSHelper.TabularActionsEditorPlugin.DrillthroughColumn);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(506, 533);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 22;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.okButton.Location = new System.Drawing.Point(425, 533);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 21;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cmbAction
            // 
            this.cmbAction.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAction.FormattingEnabled = true;
            this.cmbAction.Location = new System.Drawing.Point(123, 13);
            this.cmbAction.Name = "cmbAction";
            this.cmbAction.Size = new System.Drawing.Size(339, 21);
            this.cmbAction.TabIndex = 0;
            this.cmbAction.SelectedIndexChanged += new System.EventHandler(this.cmbAction_SelectedIndexChanged);
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Location = new System.Drawing.Point(12, 16);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(40, 13);
            this.lblAction.TabIndex = 4;
            this.lblAction.Text = "Action:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 142);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Action Type:";
            // 
            // cmbActionType
            // 
            this.cmbActionType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbActionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbActionType.FormattingEnabled = true;
            this.cmbActionType.Location = new System.Drawing.Point(123, 139);
            this.cmbActionType.Name = "cmbActionType";
            this.cmbActionType.Size = new System.Drawing.Size(457, 21);
            this.cmbActionType.TabIndex = 5;
            this.cmbActionType.SelectedIndexChanged += new System.EventHandler(this.cmbActionType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Description:";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Location = new System.Drawing.Point(123, 40);
            this.txtName.MaxLength = 100;
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(457, 20);
            this.txtName.TabIndex = 1;
            this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
            // 
            // txtDescription
            // 
            this.txtDescription.AcceptsReturn = true;
            this.txtDescription.AcceptsTab = true;
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.Location = new System.Drawing.Point(123, 92);
            this.txtDescription.MaxLength = 100000;
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDescription.Size = new System.Drawing.Size(457, 40);
            this.txtDescription.TabIndex = 4;
            // 
            // txtCaption
            // 
            this.txtCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCaption.Location = new System.Drawing.Point(123, 66);
            this.txtCaption.Name = "txtCaption";
            this.txtCaption.Size = new System.Drawing.Size(350, 20);
            this.txtCaption.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 69);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Caption:";
            // 
            // chkCaptionIsMdx
            // 
            this.chkCaptionIsMdx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCaptionIsMdx.AutoSize = true;
            this.chkCaptionIsMdx.Location = new System.Drawing.Point(479, 68);
            this.chkCaptionIsMdx.Name = "chkCaptionIsMdx";
            this.chkCaptionIsMdx.Size = new System.Drawing.Size(105, 17);
            this.chkCaptionIsMdx.TabIndex = 3;
            this.chkCaptionIsMdx.Text = "Caption is MDX?";
            this.chkCaptionIsMdx.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 169);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(68, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "Target Type:";
            // 
            // cmbTargetType
            // 
            this.cmbTargetType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTargetType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTargetType.FormattingEnabled = true;
            this.cmbTargetType.Location = new System.Drawing.Point(123, 166);
            this.cmbTargetType.Name = "cmbTargetType";
            this.cmbTargetType.Size = new System.Drawing.Size(457, 21);
            this.cmbTargetType.TabIndex = 6;
            this.cmbTargetType.SelectedIndexChanged += new System.EventHandler(this.cmbTargetType_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 196);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Target:";
            // 
            // cmbTarget
            // 
            this.cmbTarget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTarget.FormattingEnabled = true;
            this.cmbTarget.Location = new System.Drawing.Point(123, 193);
            this.cmbTarget.Name = "cmbTarget";
            this.cmbTarget.Size = new System.Drawing.Size(457, 21);
            this.cmbTarget.Sorted = true;
            this.cmbTarget.TabIndex = 7;
            // 
            // lblChangingLabel
            // 
            this.lblChangingLabel.AutoSize = true;
            this.lblChangingLabel.Location = new System.Drawing.Point(12, 275);
            this.lblChangingLabel.Name = "lblChangingLabel";
            this.lblChangingLabel.Size = new System.Drawing.Size(106, 13);
            this.lblChangingLabel.TabIndex = 20;
            this.lblChangingLabel.Text = "Drillthrough Columns:";
            // 
            // txtCondition
            // 
            this.txtCondition.AcceptsReturn = true;
            this.txtCondition.AcceptsTab = true;
            this.txtCondition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCondition.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCondition.Location = new System.Drawing.Point(123, 220);
            this.txtCondition.MaxLength = 100000;
            this.txtCondition.Multiline = true;
            this.txtCondition.Name = "txtCondition";
            this.txtCondition.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCondition.Size = new System.Drawing.Size(457, 40);
            this.txtCondition.TabIndex = 9;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 223);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 13);
            this.label8.TabIndex = 21;
            this.label8.Text = "Condition:";
            // 
            // txtExpression
            // 
            this.txtExpression.AcceptsReturn = true;
            this.txtExpression.AcceptsTab = true;
            this.txtExpression.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExpression.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExpression.Location = new System.Drawing.Point(123, 269);
            this.txtExpression.MaxLength = 100000;
            this.txtExpression.Multiline = true;
            this.txtExpression.Name = "txtExpression";
            this.txtExpression.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtExpression.Size = new System.Drawing.Size(457, 152);
            this.txtExpression.TabIndex = 10;
            // 
            // listPerspectives
            // 
            this.listPerspectives.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listPerspectives.CheckOnClick = true;
            this.listPerspectives.ColumnWidth = 150;
            this.listPerspectives.FormattingEnabled = true;
            this.listPerspectives.Location = new System.Drawing.Point(123, 454);
            this.listPerspectives.MultiColumn = true;
            this.listPerspectives.Name = "listPerspectives";
            this.listPerspectives.ScrollAlwaysVisible = true;
            this.listPerspectives.Size = new System.Drawing.Size(457, 64);
            this.listPerspectives.TabIndex = 20;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(12, 457);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(71, 13);
            this.label11.TabIndex = 27;
            this.label11.Text = "Perspectives:";
            // 
            // btnDrillthroughColumnMoveUp
            // 
            this.btnDrillthroughColumnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDrillthroughColumnMoveUp.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDrillthroughColumnMoveUp.Location = new System.Drawing.Point(559, 297);
            this.btnDrillthroughColumnMoveUp.Name = "btnDrillthroughColumnMoveUp";
            this.btnDrillthroughColumnMoveUp.Size = new System.Drawing.Size(21, 23);
            this.btnDrillthroughColumnMoveUp.TabIndex = 28;
            this.btnDrillthroughColumnMoveUp.TabStop = false;
            this.btnDrillthroughColumnMoveUp.Text = "­";
            this.btnDrillthroughColumnMoveUp.UseVisualStyleBackColor = true;
            this.btnDrillthroughColumnMoveUp.Click += new System.EventHandler(this.btnDrillthroughColumnMoveUp_Click);
            // 
            // btnDrillthroughColumnMoveDown
            // 
            this.btnDrillthroughColumnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDrillthroughColumnMoveDown.Font = new System.Drawing.Font("Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDrillthroughColumnMoveDown.Location = new System.Drawing.Point(559, 321);
            this.btnDrillthroughColumnMoveDown.Name = "btnDrillthroughColumnMoveDown";
            this.btnDrillthroughColumnMoveDown.Size = new System.Drawing.Size(21, 23);
            this.btnDrillthroughColumnMoveDown.TabIndex = 29;
            this.btnDrillthroughColumnMoveDown.TabStop = false;
            this.btnDrillthroughColumnMoveDown.Text = "¯";
            this.btnDrillthroughColumnMoveDown.UseVisualStyleBackColor = true;
            this.btnDrillthroughColumnMoveDown.Click += new System.EventHandler(this.btnDrillthroughColumnMoveDown_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Location = new System.Drawing.Point(468, 13);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(53, 21);
            this.btnAdd.TabIndex = 30;
            this.btnAdd.TabStop = false;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDelete.Location = new System.Drawing.Point(527, 13);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(53, 21);
            this.btnDelete.TabIndex = 31;
            this.btnDelete.TabStop = false;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 430);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 33;
            this.label7.Text = "Invocation:";
            // 
            // cmbInvocation
            // 
            this.cmbInvocation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbInvocation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbInvocation.FormattingEnabled = true;
            this.cmbInvocation.Location = new System.Drawing.Point(123, 427);
            this.cmbInvocation.Name = "cmbInvocation";
            this.cmbInvocation.Size = new System.Drawing.Size(457, 21);
            this.cmbInvocation.TabIndex = 19;
            // 
            // lblDefault
            // 
            this.lblDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDefault.AutoSize = true;
            this.lblDefault.Location = new System.Drawing.Point(12, 377);
            this.lblDefault.Name = "lblDefault";
            this.lblDefault.Size = new System.Drawing.Size(44, 13);
            this.lblDefault.TabIndex = 35;
            this.lblDefault.Text = "Default:";
            // 
            // cmbDefault
            // 
            this.cmbDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbDefault.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDefault.FormattingEnabled = true;
            this.cmbDefault.Items.AddRange(new object[] {
            "True",
            "False"});
            this.cmbDefault.Location = new System.Drawing.Point(123, 374);
            this.cmbDefault.Name = "cmbDefault";
            this.cmbDefault.Size = new System.Drawing.Size(457, 21);
            this.cmbDefault.TabIndex = 17;
            // 
            // txtMaxRows
            // 
            this.txtMaxRows.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMaxRows.Location = new System.Drawing.Point(123, 401);
            this.txtMaxRows.Name = "txtMaxRows";
            this.txtMaxRows.Size = new System.Drawing.Size(457, 20);
            this.txtMaxRows.TabIndex = 18;
            // 
            // lblMaxRows
            // 
            this.lblMaxRows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMaxRows.AutoSize = true;
            this.lblMaxRows.Location = new System.Drawing.Point(12, 404);
            this.lblMaxRows.Name = "lblMaxRows";
            this.lblMaxRows.Size = new System.Drawing.Size(84, 13);
            this.lblMaxRows.TabIndex = 37;
            this.lblMaxRows.Text = "Maximum Rows:";
            // 
            // txtReportServer
            // 
            this.txtReportServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReportServer.Location = new System.Drawing.Point(123, 374);
            this.txtReportServer.Name = "txtReportServer";
            this.txtReportServer.Size = new System.Drawing.Size(457, 20);
            this.txtReportServer.TabIndex = 11;
            this.txtReportServer.Visible = false;
            // 
            // dataGridViewReportParameters
            // 
            this.dataGridViewReportParameters.AllowUserToResizeRows = false;
            this.dataGridViewReportParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewReportParameters.AutoGenerateColumns = false;
            this.dataGridViewReportParameters.CausesValidation = false;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle10.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewReportParameters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            this.dataGridViewReportParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewReportParameters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.nameDataGridViewTextBoxColumn,
            this.valueDataGridViewTextBoxColumn});
            this.dataGridViewReportParameters.DataSource = this.reportParameterBindingSource;
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle11.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewReportParameters.DefaultCellStyle = dataGridViewCellStyle11;
            this.dataGridViewReportParameters.Location = new System.Drawing.Point(124, 269);
            this.dataGridViewReportParameters.Name = "dataGridViewReportParameters";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle12.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewReportParameters.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.dataGridViewReportParameters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewReportParameters.ShowEditingIcon = false;
            this.dataGridViewReportParameters.Size = new System.Drawing.Size(456, 99);
            this.dataGridViewReportParameters.TabIndex = 10;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Parameter Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.Width = 120;
            // 
            // valueDataGridViewTextBoxColumn
            // 
            this.valueDataGridViewTextBoxColumn.DataPropertyName = "Value";
            this.valueDataGridViewTextBoxColumn.HeaderText = "Parameter Value Expression";
            this.valueDataGridViewTextBoxColumn.Name = "valueDataGridViewTextBoxColumn";
            this.valueDataGridViewTextBoxColumn.Width = 270;
            // 
            // reportParameterBindingSource
            // 
            this.reportParameterBindingSource.DataSource = typeof(Microsoft.AnalysisServices.ReportParameter);
            // 
            // linkHelp
            // 
            this.linkHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkHelp.AutoSize = true;
            this.linkHelp.Location = new System.Drawing.Point(120, 538);
            this.linkHelp.Name = "linkHelp";
            this.linkHelp.Size = new System.Drawing.Size(98, 13);
            this.linkHelp.TabIndex = 38;
            this.linkHelp.TabStop = true;
            this.linkHelp.Text = "Help and Examples";
            this.linkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkHelp_LinkClicked);
            // 
            // TabularActionsEditorForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(594, 572);
            this.ControlBox = false;
            this.Controls.Add(this.linkHelp);
            this.Controls.Add(this.dataGridViewReportParameters);
            this.Controls.Add(this.txtReportServer);
            this.Controls.Add(this.txtMaxRows);
            this.Controls.Add(this.lblMaxRows);
            this.Controls.Add(this.lblDefault);
            this.Controls.Add(this.cmbDefault);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cmbInvocation);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDrillthroughColumnMoveDown);
            this.Controls.Add(this.btnDrillthroughColumnMoveUp);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.listPerspectives);
            this.Controls.Add(this.txtCondition);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblChangingLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbTarget);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cmbTargetType);
            this.Controls.Add(this.chkCaptionIsMdx);
            this.Controls.Add(this.txtCaption);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbActionType);
            this.Controls.Add(this.lblAction);
            this.Controls.Add(this.cmbAction);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.dataGridViewDrillthroughColumns);
            this.Controls.Add(this.txtExpression);
            this.MinimizeBox = false;
            this.Name = "TabularActionsEditorForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BIDS Helper Tabular Actions Editor";
            this.Load += new System.EventHandler(this.MeasureGroupHealthCheckForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewDrillthroughColumns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.drillthroughColumnBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReportParameters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.reportParameterBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewDrillthroughColumns;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.ComboBox cmbAction;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbActionType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.TextBox txtCaption;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkCaptionIsMdx;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbTargetType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbTarget;
        private System.Windows.Forms.Label lblChangingLabel;
        private System.Windows.Forms.TextBox txtCondition;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtExpression;
        private System.Windows.Forms.CheckedListBox listPerspectives;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.BindingSource drillthroughColumnBindingSource;
        private System.Windows.Forms.Button btnDrillthroughColumnMoveUp;
        private System.Windows.Forms.Button btnDrillthroughColumnMoveDown;
        private System.Windows.Forms.DataGridViewComboBoxColumn DrillthroughDataGridCubeDimension;
        private System.Windows.Forms.DataGridViewComboBoxColumn DrillthroughDataGridAttribute;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbInvocation;
        private System.Windows.Forms.Label lblDefault;
        private System.Windows.Forms.ComboBox cmbDefault;
        private System.Windows.Forms.TextBox txtMaxRows;
        private System.Windows.Forms.Label lblMaxRows;
        private System.Windows.Forms.TextBox txtReportServer;
        private System.Windows.Forms.DataGridView dataGridViewReportParameters;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource reportParameterBindingSource;
        private System.Windows.Forms.LinkLabel linkHelp;
    }
}