namespace KpUi
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kPLinguaModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newKpLinguaModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKpLinguaModelMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kPQueriesFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newKpQueriesFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openKpQueriesFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.experimentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simulationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.verificationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpVerification = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tlpProperties = new System.Windows.Forms.TableLayoutPanel();
            this.tlpPropertyHeaders = new System.Windows.Forms.TableLayoutPanel();
            this.lQuery = new System.Windows.Forms.Label();
            this.lType = new System.Windows.Forms.Label();
            this.lOutput = new System.Windows.Forms.Label();
            this.panel6 = new System.Windows.Forms.Panel();
            this.bBrowseQueriesVerification = new System.Windows.Forms.Button();
            this.tbQueriesVerification = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.bBrowseModelVerification = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbModelVerification = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.pProgressMonitorVerification = new System.Windows.Forms.Panel();
            this.lDetailsProgressMonitorVerification = new System.Windows.Forms.Label();
            this.lMessageProgressMonitorVerification = new System.Windows.Forms.Label();
            this.pbProgressMonitorVerification = new System.Windows.Forms.ProgressBar();
            this.panel3 = new System.Windows.Forms.Panel();
            this.bPerformVerification = new System.Windows.Forms.Button();
            this.tpSimulation = new System.Windows.Forms.TabPage();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.bBrowseOutputSimulation = new System.Windows.Forms.Button();
            this.tbModelSimulation = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbOutputSimulation = new System.Windows.Forms.TextBox();
            this.bBrowseModelSimulation = new System.Windows.Forms.Button();
            this.cbSimulatorSimulation = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cbConfigurationsOnly = new System.Windows.Forms.CheckBox();
            this.cbRecordConfigurations = new System.Windows.Forms.CheckBox();
            this.cbRecordInstanceCreation = new System.Windows.Forms.CheckBox();
            this.cbRecordTargetSelection = new System.Windows.Forms.CheckBox();
            this.cbRecordRuleSelection = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.nudSeedSimulation = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.nudSkipStepsSimulation = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.nudStepsSimulation = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.pProgressMonitorSimulation = new System.Windows.Forms.Panel();
            this.lDetailsProgressMonitorSimulation = new System.Windows.Forms.Label();
            this.lMessageProgressMonitorSimulation = new System.Windows.Forms.Label();
            this.pbProgressMonitorSimulation = new System.Windows.Forms.ProgressBar();
            this.panel4 = new System.Windows.Forms.Panel();
            this.bPerformSimulation = new System.Windows.Forms.Button();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuStrip1.SuspendLayout();
            this.tcMain.SuspendLayout();
            this.tpVerification.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tlpPropertyHeaders.SuspendLayout();
            this.panel6.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.pProgressMonitorVerification.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tpSimulation.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeedSimulation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkipStepsSimulation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStepsSimulation)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            this.pProgressMonitorSimulation.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.experimentsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(905, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.kPLinguaModelToolStripMenuItem,
            this.kPQueriesFileToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator1,
            this.settingsToolStripMenuItem1,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // kPLinguaModelToolStripMenuItem
            // 
            this.kPLinguaModelToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newKpLinguaModelMenuItem,
            this.openKpLinguaModelMenuItem});
            this.kPLinguaModelToolStripMenuItem.Name = "kPLinguaModelToolStripMenuItem";
            this.kPLinguaModelToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.kPLinguaModelToolStripMenuItem.Text = "kP-Lingua Model";
            // 
            // newKpLinguaModelMenuItem
            // 
            this.newKpLinguaModelMenuItem.Name = "newKpLinguaModelMenuItem";
            this.newKpLinguaModelMenuItem.Size = new System.Drawing.Size(103, 22);
            this.newKpLinguaModelMenuItem.Text = "New";
            this.newKpLinguaModelMenuItem.Click += new System.EventHandler(this.newKpLinguaModelMenuItem_Click);
            // 
            // openKpLinguaModelMenuItem
            // 
            this.openKpLinguaModelMenuItem.Name = "openKpLinguaModelMenuItem";
            this.openKpLinguaModelMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openKpLinguaModelMenuItem.Text = "Open";
            this.openKpLinguaModelMenuItem.Click += new System.EventHandler(this.openKpLinguaModelMenuItem_Click);
            // 
            // kPQueriesFileToolStripMenuItem
            // 
            this.kPQueriesFileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newKpQueriesFileMenuItem,
            this.openKpQueriesFileMenuItem});
            this.kPQueriesFileToolStripMenuItem.Name = "kPQueriesFileToolStripMenuItem";
            this.kPQueriesFileToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.kPQueriesFileToolStripMenuItem.Text = "kP-Queries File";
            // 
            // newKpQueriesFileMenuItem
            // 
            this.newKpQueriesFileMenuItem.Name = "newKpQueriesFileMenuItem";
            this.newKpQueriesFileMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newKpQueriesFileMenuItem.Text = "New";
            this.newKpQueriesFileMenuItem.Click += new System.EventHandler(this.newKpQueriesFileMenuItem_Click);
            // 
            // openKpQueriesFileMenuItem
            // 
            this.openKpQueriesFileMenuItem.Name = "openKpQueriesFileMenuItem";
            this.openKpQueriesFileMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openKpQueriesFileMenuItem.Text = "Open";
            this.openKpQueriesFileMenuItem.Click += new System.EventHandler(this.openKpQueriesFileMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.HandleSaveClick);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.HandleExitClick);
            // 
            // experimentsToolStripMenuItem
            // 
            this.experimentsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.simulationToolStripMenuItem1,
            this.verificationToolStripMenuItem1});
            this.experimentsToolStripMenuItem.Name = "experimentsToolStripMenuItem";
            this.experimentsToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.experimentsToolStripMenuItem.Text = "Experiments";
            // 
            // simulationToolStripMenuItem1
            // 
            this.simulationToolStripMenuItem1.Name = "simulationToolStripMenuItem1";
            this.simulationToolStripMenuItem1.Size = new System.Drawing.Size(134, 22);
            this.simulationToolStripMenuItem1.Text = "Simulation";
            this.simulationToolStripMenuItem1.Click += new System.EventHandler(this.HandleSimulationClick);
            // 
            // verificationToolStripMenuItem1
            // 
            this.verificationToolStripMenuItem1.Name = "verificationToolStripMenuItem1";
            this.verificationToolStripMenuItem1.Size = new System.Drawing.Size(134, 22);
            this.verificationToolStripMenuItem1.Text = "Verification";
            this.verificationToolStripMenuItem1.Click += new System.EventHandler(this.HandleVerificationClick);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 557);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(905, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tcMain
            // 
            this.tcMain.Controls.Add(this.tpVerification);
            this.tcMain.Controls.Add(this.tpSimulation);
            this.tcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tcMain.Location = new System.Drawing.Point(0, 24);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(905, 533);
            this.tcMain.TabIndex = 3;
            this.tcMain.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tcMain_DrawItem);
            this.tcMain.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.tcMain_ControlAdded);
            this.tcMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tcMain_MouseClick);
            // 
            // tpVerification
            // 
            this.tpVerification.Controls.Add(this.panel1);
            this.tpVerification.Controls.Add(this.panel6);
            this.tpVerification.Controls.Add(this.tableLayoutPanel2);
            this.tpVerification.Location = new System.Drawing.Point(4, 22);
            this.tpVerification.Name = "tpVerification";
            this.tpVerification.Padding = new System.Windows.Forms.Padding(3);
            this.tpVerification.Size = new System.Drawing.Size(897, 507);
            this.tpVerification.TabIndex = 0;
            this.tpVerification.Text = "Verification";
            this.tpVerification.ToolTipText = "verification experiments";
            this.tpVerification.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tlpProperties);
            this.panel1.Controls.Add(this.tlpPropertyHeaders);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 124);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(891, 306);
            this.panel1.TabIndex = 14;
            // 
            // tlpProperties
            // 
            this.tlpProperties.AutoScroll = true;
            this.tlpProperties.ColumnCount = 1;
            this.tlpProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpProperties.Location = new System.Drawing.Point(0, 19);
            this.tlpProperties.Name = "tlpProperties";
            this.tlpProperties.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.tlpProperties.RowCount = 2;
            this.tlpProperties.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpProperties.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpProperties.Size = new System.Drawing.Size(891, 287);
            this.tlpProperties.TabIndex = 10;
            // 
            // tlpPropertyHeaders
            // 
            this.tlpPropertyHeaders.ColumnCount = 5;
            this.tlpPropertyHeaders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tlpPropertyHeaders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPropertyHeaders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tlpPropertyHeaders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 315F));
            this.tlpPropertyHeaders.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 94F));
            this.tlpPropertyHeaders.Controls.Add(this.lQuery, 1, 0);
            this.tlpPropertyHeaders.Controls.Add(this.lType, 2, 0);
            this.tlpPropertyHeaders.Controls.Add(this.lOutput, 3, 0);
            this.tlpPropertyHeaders.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpPropertyHeaders.Location = new System.Drawing.Point(0, 0);
            this.tlpPropertyHeaders.Name = "tlpPropertyHeaders";
            this.tlpPropertyHeaders.RowCount = 1;
            this.tlpPropertyHeaders.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpPropertyHeaders.Size = new System.Drawing.Size(891, 19);
            this.tlpPropertyHeaders.TabIndex = 9;
            // 
            // lQuery
            // 
            this.lQuery.AutoSize = true;
            this.lQuery.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lQuery.Location = new System.Drawing.Point(43, 6);
            this.lQuery.Name = "lQuery";
            this.lQuery.Size = new System.Drawing.Size(346, 13);
            this.lQuery.TabIndex = 0;
            this.lQuery.Text = "Query";
            this.lQuery.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lType
            // 
            this.lType.AutoSize = true;
            this.lType.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lType.Location = new System.Drawing.Point(395, 6);
            this.lType.Name = "lType";
            this.lType.Size = new System.Drawing.Size(84, 13);
            this.lType.TabIndex = 1;
            this.lType.Text = "Model Checker";
            this.lType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lOutput
            // 
            this.lOutput.AutoSize = true;
            this.lOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lOutput.Location = new System.Drawing.Point(485, 6);
            this.lOutput.Name = "lOutput";
            this.lOutput.Size = new System.Drawing.Size(309, 13);
            this.lOutput.TabIndex = 2;
            this.lOutput.Text = "Output Directory";
            this.lOutput.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.bBrowseQueriesVerification);
            this.panel6.Controls.Add(this.tbQueriesVerification);
            this.panel6.Controls.Add(this.label2);
            this.panel6.Controls.Add(this.bBrowseModelVerification);
            this.panel6.Controls.Add(this.label1);
            this.panel6.Controls.Add(this.tbModelVerification);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(891, 121);
            this.panel6.TabIndex = 13;
            // 
            // bBrowseQueriesVerification
            // 
            this.bBrowseQueriesVerification.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBrowseQueriesVerification.Location = new System.Drawing.Point(810, 78);
            this.bBrowseQueriesVerification.Name = "bBrowseQueriesVerification";
            this.bBrowseQueriesVerification.Size = new System.Drawing.Size(75, 24);
            this.bBrowseQueriesVerification.TabIndex = 17;
            this.bBrowseQueriesVerification.Text = "Browse...";
            this.bBrowseQueriesVerification.UseVisualStyleBackColor = true;
            this.bBrowseQueriesVerification.Click += new System.EventHandler(this.bBrowseQueriesVerification_Click);
            // 
            // tbQueriesVerification
            // 
            this.tbQueriesVerification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbQueriesVerification.Location = new System.Drawing.Point(6, 81);
            this.tbQueriesVerification.Name = "tbQueriesVerification";
            this.tbQueriesVerification.Size = new System.Drawing.Size(798, 20);
            this.tbQueriesVerification.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "kP-Queries File";
            // 
            // bBrowseModelVerification
            // 
            this.bBrowseModelVerification.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBrowseModelVerification.Location = new System.Drawing.Point(810, 32);
            this.bBrowseModelVerification.Name = "bBrowseModelVerification";
            this.bBrowseModelVerification.Size = new System.Drawing.Size(75, 24);
            this.bBrowseModelVerification.TabIndex = 14;
            this.bBrowseModelVerification.Text = "Browse...";
            this.bBrowseModelVerification.UseVisualStyleBackColor = true;
            this.bBrowseModelVerification.Click += new System.EventHandler(this.bBrowseModelVerification_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "kP-Lingua Model";
            // 
            // tbModelVerification
            // 
            this.tbModelVerification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbModelVerification.Location = new System.Drawing.Point(6, 35);
            this.tbModelVerification.Name = "tbModelVerification";
            this.tbModelVerification.Size = new System.Drawing.Size(798, 20);
            this.tbModelVerification.TabIndex = 12;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.pProgressMonitorVerification, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 430);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(891, 74);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // pProgressMonitorVerification
            // 
            this.pProgressMonitorVerification.Controls.Add(this.lDetailsProgressMonitorVerification);
            this.pProgressMonitorVerification.Controls.Add(this.lMessageProgressMonitorVerification);
            this.pProgressMonitorVerification.Controls.Add(this.pbProgressMonitorVerification);
            this.pProgressMonitorVerification.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pProgressMonitorVerification.Location = new System.Drawing.Point(103, 3);
            this.pProgressMonitorVerification.Name = "pProgressMonitorVerification";
            this.pProgressMonitorVerification.Size = new System.Drawing.Size(785, 68);
            this.pProgressMonitorVerification.TabIndex = 13;
            // 
            // lDetailsProgressMonitorVerification
            // 
            this.lDetailsProgressMonitorVerification.AutoSize = true;
            this.lDetailsProgressMonitorVerification.Location = new System.Drawing.Point(3, 29);
            this.lDetailsProgressMonitorVerification.Name = "lDetailsProgressMonitorVerification";
            this.lDetailsProgressMonitorVerification.Size = new System.Drawing.Size(86, 13);
            this.lDetailsProgressMonitorVerification.TabIndex = 2;
            this.lDetailsProgressMonitorVerification.Text = "Current Sub-task";
            // 
            // lMessageProgressMonitorVerification
            // 
            this.lMessageProgressMonitorVerification.AutoSize = true;
            this.lMessageProgressMonitorVerification.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lMessageProgressMonitorVerification.Location = new System.Drawing.Point(3, 8);
            this.lMessageProgressMonitorVerification.Name = "lMessageProgressMonitorVerification";
            this.lMessageProgressMonitorVerification.Size = new System.Drawing.Size(80, 13);
            this.lMessageProgressMonitorVerification.TabIndex = 1;
            this.lMessageProgressMonitorVerification.Text = "Current Task";
            // 
            // pbProgressMonitorVerification
            // 
            this.pbProgressMonitorVerification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbProgressMonitorVerification.Location = new System.Drawing.Point(3, 51);
            this.pbProgressMonitorVerification.Name = "pbProgressMonitorVerification";
            this.pbProgressMonitorVerification.Size = new System.Drawing.Size(779, 14);
            this.pbProgressMonitorVerification.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbProgressMonitorVerification.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.bPerformVerification);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(94, 68);
            this.panel3.TabIndex = 14;
            // 
            // bPerformVerification
            // 
            this.bPerformVerification.Location = new System.Drawing.Point(3, 24);
            this.bPerformVerification.Name = "bPerformVerification";
            this.bPerformVerification.Size = new System.Drawing.Size(88, 23);
            this.bPerformVerification.TabIndex = 0;
            this.bPerformVerification.Text = "Perform";
            this.bPerformVerification.UseVisualStyleBackColor = true;
            this.bPerformVerification.Click += new System.EventHandler(this.bPerformVerification_Click);
            // 
            // tpSimulation
            // 
            this.tpSimulation.Controls.Add(this.panel5);
            this.tpSimulation.Controls.Add(this.tableLayoutPanel3);
            this.tpSimulation.Location = new System.Drawing.Point(4, 22);
            this.tpSimulation.Name = "tpSimulation";
            this.tpSimulation.Padding = new System.Windows.Forms.Padding(3);
            this.tpSimulation.Size = new System.Drawing.Size(897, 507);
            this.tpSimulation.TabIndex = 1;
            this.tpSimulation.Text = "Simulation";
            this.tpSimulation.ToolTipText = "simulation experiments";
            this.tpSimulation.UseVisualStyleBackColor = true;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label7);
            this.panel5.Controls.Add(this.bBrowseOutputSimulation);
            this.panel5.Controls.Add(this.tbModelSimulation);
            this.panel5.Controls.Add(this.label8);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.tbOutputSimulation);
            this.panel5.Controls.Add(this.bBrowseModelSimulation);
            this.panel5.Controls.Add(this.cbSimulatorSimulation);
            this.panel5.Controls.Add(this.panel2);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(3, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(891, 426);
            this.panel5.TabIndex = 27;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 12);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(50, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Simulator";
            // 
            // bBrowseOutputSimulation
            // 
            this.bBrowseOutputSimulation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBrowseOutputSimulation.Location = new System.Drawing.Point(809, 130);
            this.bBrowseOutputSimulation.Name = "bBrowseOutputSimulation";
            this.bBrowseOutputSimulation.Size = new System.Drawing.Size(75, 24);
            this.bBrowseOutputSimulation.TabIndex = 26;
            this.bBrowseOutputSimulation.Text = "Browse...";
            this.bBrowseOutputSimulation.UseVisualStyleBackColor = true;
            this.bBrowseOutputSimulation.Click += new System.EventHandler(this.bBrowseOutputSimulation_Click);
            // 
            // tbModelSimulation
            // 
            this.tbModelSimulation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbModelSimulation.Location = new System.Drawing.Point(5, 83);
            this.tbModelSimulation.Name = "tbModelSimulation";
            this.tbModelSimulation.Size = new System.Drawing.Size(798, 20);
            this.tbModelSimulation.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 114);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(84, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Output Directory";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "kP-Lingua Model";
            // 
            // tbOutputSimulation
            // 
            this.tbOutputSimulation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbOutputSimulation.Location = new System.Drawing.Point(5, 133);
            this.tbOutputSimulation.Name = "tbOutputSimulation";
            this.tbOutputSimulation.Size = new System.Drawing.Size(798, 20);
            this.tbOutputSimulation.TabIndex = 24;
            // 
            // bBrowseModelSimulation
            // 
            this.bBrowseModelSimulation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBrowseModelSimulation.Location = new System.Drawing.Point(809, 80);
            this.bBrowseModelSimulation.Name = "bBrowseModelSimulation";
            this.bBrowseModelSimulation.Size = new System.Drawing.Size(75, 24);
            this.bBrowseModelSimulation.TabIndex = 5;
            this.bBrowseModelSimulation.Text = "Browse...";
            this.bBrowseModelSimulation.UseVisualStyleBackColor = true;
            this.bBrowseModelSimulation.Click += new System.EventHandler(this.bBrowseModelSimulation_Click);
            // 
            // cbSimulatorSimulation
            // 
            this.cbSimulatorSimulation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSimulatorSimulation.FormattingEnabled = true;
            this.cbSimulatorSimulation.Location = new System.Drawing.Point(5, 32);
            this.cbSimulatorSimulation.Name = "cbSimulatorSimulation";
            this.cbSimulatorSimulation.Size = new System.Drawing.Size(121, 21);
            this.cbSimulatorSimulation.TabIndex = 23;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cbConfigurationsOnly);
            this.panel2.Controls.Add(this.cbRecordConfigurations);
            this.panel2.Controls.Add(this.cbRecordInstanceCreation);
            this.panel2.Controls.Add(this.cbRecordTargetSelection);
            this.panel2.Controls.Add(this.cbRecordRuleSelection);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.nudSeedSimulation);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.nudSkipStepsSimulation);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.nudStepsSimulation);
            this.panel2.Location = new System.Drawing.Point(5, 166);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(342, 175);
            this.panel2.TabIndex = 21;
            // 
            // cbConfigurationsOnly
            // 
            this.cbConfigurationsOnly.AutoSize = true;
            this.cbConfigurationsOnly.Location = new System.Drawing.Point(2, 146);
            this.cbConfigurationsOnly.Name = "cbConfigurationsOnly";
            this.cbConfigurationsOnly.Size = new System.Drawing.Size(115, 17);
            this.cbConfigurationsOnly.TabIndex = 31;
            this.cbConfigurationsOnly.Text = "Configurations only";
            this.cbConfigurationsOnly.UseVisualStyleBackColor = true;
            // 
            // cbRecordConfigurations
            // 
            this.cbRecordConfigurations.AutoSize = true;
            this.cbRecordConfigurations.Checked = true;
            this.cbRecordConfigurations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRecordConfigurations.Location = new System.Drawing.Point(168, 106);
            this.cbRecordConfigurations.Name = "cbRecordConfigurations";
            this.cbRecordConfigurations.Size = new System.Drawing.Size(130, 17);
            this.cbRecordConfigurations.TabIndex = 30;
            this.cbRecordConfigurations.Text = "Record configurations";
            this.cbRecordConfigurations.UseVisualStyleBackColor = true;
            // 
            // cbRecordInstanceCreation
            // 
            this.cbRecordInstanceCreation.AutoSize = true;
            this.cbRecordInstanceCreation.Checked = true;
            this.cbRecordInstanceCreation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRecordInstanceCreation.Location = new System.Drawing.Point(2, 106);
            this.cbRecordInstanceCreation.Name = "cbRecordInstanceCreation";
            this.cbRecordInstanceCreation.Size = new System.Drawing.Size(145, 17);
            this.cbRecordInstanceCreation.TabIndex = 29;
            this.cbRecordInstanceCreation.Text = "Record instance creation";
            this.cbRecordInstanceCreation.UseVisualStyleBackColor = true;
            // 
            // cbRecordTargetSelection
            // 
            this.cbRecordTargetSelection.AutoSize = true;
            this.cbRecordTargetSelection.Checked = true;
            this.cbRecordTargetSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRecordTargetSelection.Location = new System.Drawing.Point(168, 66);
            this.cbRecordTargetSelection.Name = "cbRecordTargetSelection";
            this.cbRecordTargetSelection.Size = new System.Drawing.Size(136, 17);
            this.cbRecordTargetSelection.TabIndex = 28;
            this.cbRecordTargetSelection.Text = "Record target selection";
            this.cbRecordTargetSelection.UseVisualStyleBackColor = true;
            // 
            // cbRecordRuleSelection
            // 
            this.cbRecordRuleSelection.AutoSize = true;
            this.cbRecordRuleSelection.Checked = true;
            this.cbRecordRuleSelection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbRecordRuleSelection.Location = new System.Drawing.Point(2, 66);
            this.cbRecordRuleSelection.Name = "cbRecordRuleSelection";
            this.cbRecordRuleSelection.Size = new System.Drawing.Size(126, 17);
            this.cbRecordRuleSelection.TabIndex = 27;
            this.cbRecordRuleSelection.Text = "Record rule selection";
            this.cbRecordRuleSelection.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(234, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Seed";
            // 
            // nudSeedSimulation
            // 
            this.nudSeedSimulation.Location = new System.Drawing.Point(234, 27);
            this.nudSeedSimulation.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nudSeedSimulation.Name = "nudSeedSimulation";
            this.nudSeedSimulation.Size = new System.Drawing.Size(100, 20);
            this.nudSeedSimulation.TabIndex = 25;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(118, 8);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 24;
            this.label5.Text = "Skip steps";
            // 
            // nudSkipStepsSimulation
            // 
            this.nudSkipStepsSimulation.Location = new System.Drawing.Point(118, 27);
            this.nudSkipStepsSimulation.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nudSkipStepsSimulation.Name = "nudSkipStepsSimulation";
            this.nudSkipStepsSimulation.Size = new System.Drawing.Size(100, 20);
            this.nudSkipStepsSimulation.TabIndex = 23;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "Steps";
            // 
            // nudStepsSimulation
            // 
            this.nudStepsSimulation.Location = new System.Drawing.Point(2, 27);
            this.nudStepsSimulation.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
            this.nudStepsSimulation.Name = "nudStepsSimulation";
            this.nudStepsSimulation.Size = new System.Drawing.Size(100, 20);
            this.nudStepsSimulation.TabIndex = 21;
            this.nudStepsSimulation.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.pProgressMonitorSimulation, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel4, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 429);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(891, 75);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // pProgressMonitorSimulation
            // 
            this.pProgressMonitorSimulation.Controls.Add(this.lDetailsProgressMonitorSimulation);
            this.pProgressMonitorSimulation.Controls.Add(this.lMessageProgressMonitorSimulation);
            this.pProgressMonitorSimulation.Controls.Add(this.pbProgressMonitorSimulation);
            this.pProgressMonitorSimulation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pProgressMonitorSimulation.Location = new System.Drawing.Point(103, 3);
            this.pProgressMonitorSimulation.Name = "pProgressMonitorSimulation";
            this.pProgressMonitorSimulation.Size = new System.Drawing.Size(785, 69);
            this.pProgressMonitorSimulation.TabIndex = 27;
            // 
            // lDetailsProgressMonitorSimulation
            // 
            this.lDetailsProgressMonitorSimulation.AutoSize = true;
            this.lDetailsProgressMonitorSimulation.Location = new System.Drawing.Point(3, 30);
            this.lDetailsProgressMonitorSimulation.Name = "lDetailsProgressMonitorSimulation";
            this.lDetailsProgressMonitorSimulation.Size = new System.Drawing.Size(86, 13);
            this.lDetailsProgressMonitorSimulation.TabIndex = 2;
            this.lDetailsProgressMonitorSimulation.Text = "Current Sub-task";
            // 
            // lMessageProgressMonitorSimulation
            // 
            this.lMessageProgressMonitorSimulation.AutoSize = true;
            this.lMessageProgressMonitorSimulation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lMessageProgressMonitorSimulation.Location = new System.Drawing.Point(3, 9);
            this.lMessageProgressMonitorSimulation.Name = "lMessageProgressMonitorSimulation";
            this.lMessageProgressMonitorSimulation.Size = new System.Drawing.Size(80, 13);
            this.lMessageProgressMonitorSimulation.TabIndex = 1;
            this.lMessageProgressMonitorSimulation.Text = "Current Task";
            // 
            // pbProgressMonitorSimulation
            // 
            this.pbProgressMonitorSimulation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbProgressMonitorSimulation.Location = new System.Drawing.Point(4, 52);
            this.pbProgressMonitorSimulation.Name = "pbProgressMonitorSimulation";
            this.pbProgressMonitorSimulation.Size = new System.Drawing.Size(777, 14);
            this.pbProgressMonitorSimulation.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbProgressMonitorSimulation.TabIndex = 0;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.bPerformSimulation);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(3, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(94, 69);
            this.panel4.TabIndex = 28;
            // 
            // bPerformSimulation
            // 
            this.bPerformSimulation.Location = new System.Drawing.Point(3, 25);
            this.bPerformSimulation.Name = "bPerformSimulation";
            this.bPerformSimulation.Size = new System.Drawing.Size(88, 23);
            this.bPerformSimulation.TabIndex = 0;
            this.bPerformSimulation.Text = "Perform";
            this.bPerformSimulation.UseVisualStyleBackColor = true;
            this.bPerformSimulation.Click += new System.EventHandler(this.bPerformSimulation_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(162, 6);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(165, 22);
            this.settingsToolStripMenuItem1.Text = "Settings";
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.settingsToolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(162, 6);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(905, 579);
            this.Controls.Add(this.tcMain);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "kPWorkbench";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tcMain.ResumeLayout(false);
            this.tpVerification.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tlpPropertyHeaders.ResumeLayout(false);
            this.tlpPropertyHeaders.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.pProgressMonitorVerification.ResumeLayout(false);
            this.pProgressMonitorVerification.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tpSimulation.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSeedSimulation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSkipStepsSimulation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStepsSimulation)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.pProgressMonitorSimulation.ResumeLayout(false);
            this.pProgressMonitorSimulation.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem kPLinguaModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newKpLinguaModelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openKpLinguaModelMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kPQueriesFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newKpQueriesFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openKpQueriesFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem experimentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simulationToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem verificationToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpVerification;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel pProgressMonitorVerification;
        private System.Windows.Forms.Label lDetailsProgressMonitorVerification;
        private System.Windows.Forms.Label lMessageProgressMonitorVerification;
        private System.Windows.Forms.ProgressBar pbProgressMonitorVerification;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button bPerformVerification;
        private System.Windows.Forms.TabPage tpSimulation;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button bBrowseOutputSimulation;
        private System.Windows.Forms.TextBox tbModelSimulation;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbOutputSimulation;
        private System.Windows.Forms.Button bBrowseModelSimulation;
        private System.Windows.Forms.ComboBox cbSimulatorSimulation;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbConfigurationsOnly;
        private System.Windows.Forms.CheckBox cbRecordConfigurations;
        private System.Windows.Forms.CheckBox cbRecordInstanceCreation;
        private System.Windows.Forms.CheckBox cbRecordTargetSelection;
        private System.Windows.Forms.CheckBox cbRecordRuleSelection;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudSeedSimulation;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown nudSkipStepsSimulation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nudStepsSimulation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel pProgressMonitorSimulation;
        private System.Windows.Forms.Label lDetailsProgressMonitorSimulation;
        private System.Windows.Forms.Label lMessageProgressMonitorSimulation;
        private System.Windows.Forms.ProgressBar pbProgressMonitorSimulation;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button bPerformSimulation;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tlpPropertyHeaders;
        private System.Windows.Forms.Label lQuery;
        private System.Windows.Forms.Label lType;
        private System.Windows.Forms.Label lOutput;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button bBrowseQueriesVerification;
        private System.Windows.Forms.TextBox tbQueriesVerification;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button bBrowseModelVerification;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbModelVerification;
        private System.Windows.Forms.TableLayoutPanel tlpProperties;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}

