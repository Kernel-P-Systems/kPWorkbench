namespace KpUi
{
    partial class PropertyControl
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
            this.cbType = new System.Windows.Forms.ComboBox();
            this.lProperty = new System.Windows.Forms.Label();
            this.cbCheck = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.bBrowse = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbType
            // 
            this.cbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbType.FormattingEnabled = true;
            this.cbType.Location = new System.Drawing.Point(54, 8);
            this.cbType.Margin = new System.Windows.Forms.Padding(15, 8, 3, 3);
            this.cbType.Name = "cbType";
            this.cbType.Size = new System.Drawing.Size(70, 21);
            this.cbType.TabIndex = 2;
            // 
            // lProperty
            // 
            this.lProperty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lProperty.AutoSize = true;
            this.lProperty.Location = new System.Drawing.Point(43, 0);
            this.lProperty.Name = "lProperty";
            this.lProperty.Padding = new System.Windows.Forms.Padding(10, 10, 0, 0);
            this.lProperty.Size = new System.Drawing.Size(1, 23);
            this.lProperty.TabIndex = 1;
            this.lProperty.Text = "Property ";
            // 
            // cbCheck
            // 
            this.cbCheck.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cbCheck.Location = new System.Drawing.Point(1, 11);
            this.cbCheck.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.cbCheck.Name = "cbCheck";
            this.cbCheck.Size = new System.Drawing.Size(34, 14);
            this.cbCheck.TabIndex = 0;
            this.cbCheck.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 287F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tableLayoutPanel1.Controls.Add(this.cbCheck, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lProperty, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cbType, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tbPath, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.bBrowse, 4, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(520, 37);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tbPath
            // 
            this.tbPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPath.Location = new System.Drawing.Point(149, 10);
            this.tbPath.Margin = new System.Windows.Forms.Padding(10, 7, 10, 3);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(267, 20);
            this.tbPath.TabIndex = 3;
            // 
            // bBrowse
            // 
            this.bBrowse.Location = new System.Drawing.Point(436, 8);
            this.bBrowse.Margin = new System.Windows.Forms.Padding(10, 8, 3, 3);
            this.bBrowse.Name = "bBrowse";
            this.bBrowse.Size = new System.Drawing.Size(75, 23);
            this.bBrowse.TabIndex = 4;
            this.bBrowse.Text = "Browse...";
            this.bBrowse.UseVisualStyleBackColor = true;
            this.bBrowse.Click += new System.EventHandler(this.bBrowse_Click);
            // 
            // PropertyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PropertyControl";
            this.Size = new System.Drawing.Size(520, 37);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cbType;
        private System.Windows.Forms.Label lProperty;
        private System.Windows.Forms.CheckBox cbCheck;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Button bBrowse;

    }
}
