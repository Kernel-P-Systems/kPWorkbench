namespace KpUi
{
    partial class SettingsForm
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
            this.bOK = new System.Windows.Forms.Button();
            this.bCancel = new System.Windows.Forms.Button();
            this.bBrowseXparserPath = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbXparserPath = new System.Windows.Forms.TextBox();
            this.bBrowseLibmboardPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbLibmboardPath = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            // 
            // bOK
            // 
            this.bOK.Location = new System.Drawing.Point(329, 182);
            this.bOK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(88, 27);
            this.bOK.TabIndex = 0;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // bCancel
            // 
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(436, 182);
            this.bCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(88, 27);
            this.bCancel.TabIndex = 1;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // bBrowseXparserPath
            // 
            this.bBrowseXparserPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseXparserPath.Location = new System.Drawing.Point(524, 47);
            this.bBrowseXparserPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bBrowseXparserPath.Name = "bBrowseXparserPath";
            this.bBrowseXparserPath.Size = new System.Drawing.Size(88, 28);
            this.bBrowseXparserPath.TabIndex = 17;
            this.bBrowseXparserPath.Text = "Browse...";
            this.bBrowseXparserPath.UseVisualStyleBackColor = true;
            this.bBrowseXparserPath.Click += new System.EventHandler(this.bBrowseXparserPath_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 15);
            this.label1.TabIndex = 16;
            this.label1.Text = "Xparser path";
            // 
            // tbXparserPath
            // 
            this.tbXparserPath.Location = new System.Drawing.Point(12, 51);
            this.tbXparserPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbXparserPath.Name = "tbXparserPath";
            this.tbXparserPath.Size = new System.Drawing.Size(504, 23);
            this.tbXparserPath.TabIndex = 15;
            // 
            // bBrowseLibmboardPath
            // 
            this.bBrowseLibmboardPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseLibmboardPath.Location = new System.Drawing.Point(524, 115);
            this.bBrowseLibmboardPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.bBrowseLibmboardPath.Name = "bBrowseLibmboardPath";
            this.bBrowseLibmboardPath.Size = new System.Drawing.Size(88, 28);
            this.bBrowseLibmboardPath.TabIndex = 20;
            this.bBrowseLibmboardPath.Text = "Browse...";
            this.bBrowseLibmboardPath.UseVisualStyleBackColor = true;
            this.bBrowseLibmboardPath.Click += new System.EventHandler(this.bBrowseLibmboardPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 97);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 15);
            this.label2.TabIndex = 19;
            this.label2.Text = "Libmboard path";
            // 
            // tbLibmboardPath
            // 
            this.tbLibmboardPath.Location = new System.Drawing.Point(12, 119);
            this.tbLibmboardPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbLibmboardPath.Name = "tbLibmboardPath";
            this.tbLibmboardPath.Size = new System.Drawing.Size(504, 23);
            this.tbLibmboardPath.TabIndex = 18;
            // 
            // pictureBox1
            // 
            this.pictureBox1.ImageLocation = "Resources/xImage.png";
            this.pictureBox1.Location = new System.Drawing.Point(178, 182);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.TabIndex = 21;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(634, 241);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.bBrowseLibmboardPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbLibmboardPath);
            this.Controls.Add(this.bBrowseXparserPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbXparserPath);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();

        }

        #endregion

        private System.Windows.Forms.Button bOK;
        private System.Windows.Forms.Button bCancel;
        private System.Windows.Forms.Button bBrowseXparserPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbXparserPath;
        private System.Windows.Forms.Button bBrowseLibmboardPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbLibmboardPath;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}