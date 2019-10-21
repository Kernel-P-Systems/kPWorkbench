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
            this.bBrowseNusmvPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNusmvPath = new System.Windows.Forms.TextBox();
            this.bBrowseSpinPath = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbSpinPath = new System.Windows.Forms.TextBox();
            this.bBrowseGccPath = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbGccPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbSpinOptions = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // bOK
            // 
            this.bOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bOK.Location = new System.Drawing.Point(358, 414);
            this.bOK.Name = "bOK";
            this.bOK.Size = new System.Drawing.Size(75, 23);
            this.bOK.TabIndex = 0;
            this.bOK.Text = "OK";
            this.bOK.UseVisualStyleBackColor = true;
            this.bOK.Click += new System.EventHandler(this.bOK_Click);
            // 
            // bCancel
            // 
            this.bCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.bCancel.Location = new System.Drawing.Point(449, 414);
            this.bCancel.Name = "bCancel";
            this.bCancel.Size = new System.Drawing.Size(75, 23);
            this.bCancel.TabIndex = 1;
            this.bCancel.Text = "Cancel";
            this.bCancel.UseVisualStyleBackColor = true;
            // 
            // bBrowseXparserPath
            // 
            this.bBrowseXparserPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseXparserPath.Location = new System.Drawing.Point(449, 285);
            this.bBrowseXparserPath.Name = "bBrowseXparserPath";
            this.bBrowseXparserPath.Size = new System.Drawing.Size(75, 24);
            this.bBrowseXparserPath.TabIndex = 17;
            this.bBrowseXparserPath.Text = "Browse...";
            this.bBrowseXparserPath.UseVisualStyleBackColor = true;
            this.bBrowseXparserPath.Click += new System.EventHandler(this.bBrowseXparserPath_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 268);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "XParser Path";
            // 
            // tbXparserPath
            // 
            this.tbXparserPath.Location = new System.Drawing.Point(12, 287);
            this.tbXparserPath.Name = "tbXparserPath";
            this.tbXparserPath.Size = new System.Drawing.Size(421, 20);
            this.tbXparserPath.TabIndex = 15;
            // 
            // bBrowseLibmboardPath
            // 
            this.bBrowseLibmboardPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseLibmboardPath.Location = new System.Drawing.Point(449, 344);
            this.bBrowseLibmboardPath.Name = "bBrowseLibmboardPath";
            this.bBrowseLibmboardPath.Size = new System.Drawing.Size(75, 24);
            this.bBrowseLibmboardPath.TabIndex = 20;
            this.bBrowseLibmboardPath.Text = "Browse...";
            this.bBrowseLibmboardPath.UseVisualStyleBackColor = true;
            this.bBrowseLibmboardPath.Click += new System.EventHandler(this.bBrowseLibmboardPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 327);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Libmboard Path";
            // 
            // tbLibmboardPath
            // 
            this.tbLibmboardPath.Location = new System.Drawing.Point(12, 346);
            this.tbLibmboardPath.Name = "tbLibmboardPath";
            this.tbLibmboardPath.Size = new System.Drawing.Size(421, 20);
            this.tbLibmboardPath.TabIndex = 18;
            // 
            // bBrowseNusmvPath
            // 
            this.bBrowseNusmvPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseNusmvPath.Location = new System.Drawing.Point(449, 223);
            this.bBrowseNusmvPath.Name = "bBrowseNusmvPath";
            this.bBrowseNusmvPath.Size = new System.Drawing.Size(75, 24);
            this.bBrowseNusmvPath.TabIndex = 23;
            this.bBrowseNusmvPath.Text = "Browse...";
            this.bBrowseNusmvPath.UseVisualStyleBackColor = true;
            this.bBrowseNusmvPath.Click += new System.EventHandler(this.bBrowseNusmvPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 206);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "NuSMV Path";
            // 
            // tbNusmvPath
            // 
            this.tbNusmvPath.Location = new System.Drawing.Point(12, 225);
            this.tbNusmvPath.Name = "tbNusmvPath";
            this.tbNusmvPath.Size = new System.Drawing.Size(421, 20);
            this.tbNusmvPath.TabIndex = 21;
            // 
            // bBrowseSpinPath
            // 
            this.bBrowseSpinPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseSpinPath.Location = new System.Drawing.Point(449, 99);
            this.bBrowseSpinPath.Name = "bBrowseSpinPath";
            this.bBrowseSpinPath.Size = new System.Drawing.Size(75, 24);
            this.bBrowseSpinPath.TabIndex = 26;
            this.bBrowseSpinPath.Text = "Browse...";
            this.bBrowseSpinPath.UseVisualStyleBackColor = true;
            this.bBrowseSpinPath.Click += new System.EventHandler(this.bBrowseSpinPath_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 25;
            this.label4.Text = "Spin Path";
            // 
            // tbSpinPath
            // 
            this.tbSpinPath.Location = new System.Drawing.Point(12, 101);
            this.tbSpinPath.Name = "tbSpinPath";
            this.tbSpinPath.Size = new System.Drawing.Size(421, 20);
            this.tbSpinPath.TabIndex = 24;
            // 
            // bBrowseGccPath
            // 
            this.bBrowseGccPath.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.bBrowseGccPath.Location = new System.Drawing.Point(449, 39);
            this.bBrowseGccPath.Name = "bBrowseGccPath";
            this.bBrowseGccPath.Size = new System.Drawing.Size(75, 24);
            this.bBrowseGccPath.TabIndex = 29;
            this.bBrowseGccPath.Text = "Browse...";
            this.bBrowseGccPath.UseVisualStyleBackColor = true;
            this.bBrowseGccPath.Click += new System.EventHandler(this.bBrowseGccPath_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "Gcc Path";
            // 
            // tbGccPath
            // 
            this.tbGccPath.Location = new System.Drawing.Point(12, 41);
            this.tbGccPath.Name = "tbGccPath";
            this.tbGccPath.Size = new System.Drawing.Size(421, 20);
            this.tbGccPath.TabIndex = 27;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 31;
            this.label6.Text = "Spin Options";
            // 
            // tbSpinOptions
            // 
            this.tbSpinOptions.Location = new System.Drawing.Point(12, 163);
            this.tbSpinOptions.Name = "tbSpinOptions";
            this.tbSpinOptions.Size = new System.Drawing.Size(421, 20);
            this.tbSpinOptions.TabIndex = 30;
            this.tbSpinOptions.Text = "-DVECTORSZ=99999999 -DBITSTATE pan.c";
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.bOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.bCancel;
            this.ClientSize = new System.Drawing.Size(543, 449);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbSpinOptions);
            this.Controls.Add(this.bBrowseGccPath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbGccPath);
            this.Controls.Add(this.bBrowseSpinPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbSpinPath);
            this.Controls.Add(this.bBrowseNusmvPath);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbNusmvPath);
            this.Controls.Add(this.bBrowseLibmboardPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbLibmboardPath);
            this.Controls.Add(this.bBrowseXparserPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbXparserPath);
            this.Controls.Add(this.bCancel);
            this.Controls.Add(this.bOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Shown += new System.EventHandler(this.SettingsForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button bBrowseNusmvPath;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbNusmvPath;
        private System.Windows.Forms.Button bBrowseSpinPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbSpinPath;
        private System.Windows.Forms.Button bBrowseGccPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbGccPath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbSpinOptions;
    }
}