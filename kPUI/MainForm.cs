using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using KpUi.Editor;
using KpExperiment.Model;
using kpw;
using KpExperiment.Model.Verification;
using KpUi.Simulation.Runtime;
using KpUi.Runtime;
using System.Reflection;
using ModelChecking.Runtime;
using KpExperiment.Verification.Translation.Base;

namespace KpUi
{
    public partial class MainForm : Form
    {
        private Point _imageLocation = new Point(15, 5);
        private Point _imgHitArea = new Point(13, 2);

        private PropertyList properties;

        public MainForm()
        {
            InitializeComponent();

            properties = new PropertyList(tlpPropertyHeaders, tlpProperties);

            cbSimulatorSimulation.Items.Add(GetTypeDescription(SimulatorTarget.Kpw));
            cbSimulatorSimulation.Items.Add(GetTypeDescription(SimulatorTarget.Flame));
            cbSimulatorSimulation.SelectedIndex = 0;

            tcMain.TabPages.Remove(tpSimulation);
            tcMain.TabPages.Remove(tpVerification);
            newKpLinguaModelMenuItem_Click(null, new EventArgs());

            lMessageProgressMonitorVerification.Visible = false;
            pbProgressMonitorVerification.Visible = false;
            lDetailsProgressMonitorVerification.Visible = false;

            lMessageProgressMonitorSimulation.Visible = false;
            pbProgressMonitorSimulation.Visible = false;
            lDetailsProgressMonitorSimulation.Visible = false;
        }

        private string GetTypeDescription(Enum type)
        {
            FieldInfo fi = type.GetType().GetField(type.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return type.ToString();
            }
        }

        private void tcMain_ControlAdded(object sender, ControlEventArgs e)
        {
            AddSpaces(e.Control);
        }

        private void AddSpaces(Control control)
        {
            Graphics g = control.CreateGraphics();
            SizeF w = g.MeasureString(" ", this.Font);
            int n = (int)(8f / w.Width);
            string s = control.Text;
            s = s.TrimEnd() + new string(' ', n + 3);
            control.Text = s;
        }

        private void tcMain_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            try
            {
                //Close Image to draw
                Image img = kPUI.Properties.Resources.xImage;

                Rectangle r = e.Bounds;
                r = this.tcMain.GetTabRect(e.Index);
                r.Offset(2, 2);

                Brush TitleBrush = new SolidBrush(Color.Black);
                Font f = this.Font;

                string title = this.tcMain.TabPages[e.Index].Text;

                e.Graphics.DrawString(title, f, TitleBrush, new PointF(r.X, r.Y));
                e.Graphics.DrawImage(img, new Point(r.X + (this.tcMain.GetTabRect(e.Index).Width - _imageLocation.X), _imageLocation.Y + 4));
            }
            catch (Exception) { }
        }

        private void tcMain_MouseClick(object sender, MouseEventArgs e)
        {
            TabControl tc = (TabControl)sender;
            Point p = e.Location;
            int _tabWidth = 0;
            _tabWidth = this.tcMain.GetTabRect(tc.SelectedIndex).Width - (_imgHitArea.X);
            Rectangle r = this.tcMain.GetTabRect(tc.SelectedIndex);
            r.Offset(_tabWidth, _imgHitArea.Y);
            r.Width = 16;
            r.Height = 16;
            if (r.Contains(p))
            {
                TabPage TabP = (TabPage)tc.TabPages[tc.SelectedIndex];
                tc.TabPages.Remove(TabP);
            }
        }

        private void HandleSimulationClick(object sender, EventArgs e)
        {
            if (!tcMain.TabPages.Contains(tpSimulation))
            {
                tcMain.TabPages.Add(tpSimulation);
                tcMain.SelectedTab = tpSimulation;
            }
        }

        private void HandleVerificationClick(object sender, EventArgs e)
        {
            if (!tcMain.TabPages.Contains(tpVerification))
            {
                tcMain.TabPages.Add(tpVerification);
                tcMain.SelectedTab = tpVerification;
            }
        }

        private void bBrowseModelVerification_Click(object sender, EventArgs e)
        {
            OpenFileDialog opd = new OpenFileDialog();
            opd.Filter = "Kernel P System files (*.kpl)|*.kpl";
            if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbModelVerification.Text = opd.FileName;
            }
        }

        private void bBrowseQueriesVerification_Click(object sender, EventArgs e)
        {
            OpenFileDialog opd = new OpenFileDialog();
            opd.Filter = "Query files (*.kpq)|*.kpq";
            if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string kpxFileName = opd.FileName;
                tbQueriesVerification.Text = opd.FileName;
                string[] lines = File.ReadAllLines(opd.FileName);
                Experiment kpExperiment = null;
                if (!string.IsNullOrEmpty(kpxFileName))
                {
                    if (new FileInfo(kpxFileName).Exists)
                    {
                        try
                        {
                            kpExperiment = KP.FromKpx(kpxFileName);
                        }
                        catch (Exception exception)
                        {
                            throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kpxFileName, exception.Message));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("File '{0}' does not exist. Please specify a valid experiment file.", kpxFileName));
                    }
                }
                properties.Load(new List<string>(lines), kpExperiment);
            }
        }

        private void bBrowseModelSimulation_Click(object sender, EventArgs e)
        {
            OpenFileDialog opd = new OpenFileDialog();
            opd.Filter = "Kernel P System files (*.kpl)|*.kpl";
            if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbModelSimulation.Text = opd.FileName;
            }
        }

        private void bBrowseOutputSimulation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                tbOutputSimulation.Text = fbd.SelectedPath;
            }
        }

        private void newKpLinguaModelMenuItem_Click(object sender, EventArgs e)
        {
            var kplEditor = new KpLinguaEditor();
            var newTabPage = new TabPage("New kP-Lingua Model");
            newTabPage.Controls.Add(kplEditor);

            tcMain.TabPages.Add(newTabPage);
            tcMain.SelectedTab = newTabPage;
        }

        private void newKpQueriesFileMenuItem_Click(object sender, EventArgs e)
        {
            var kpqEditor = new KpQueriesEditor();
            var newTabPage = new TabPage("New kP-Queries File");
            newTabPage.Controls.Add(kpqEditor);

            tcMain.TabPages.Add(newTabPage);
            tcMain.SelectedTab = newTabPage;
        }

        private void openKpLinguaModelMenuItem_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            opd.Filter = "kP Lingua files (*.kpl)|*.kpl";

            if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileName = opd.FileName;

                var kplEditor = new KpLinguaEditor();
                kplEditor.Editor.Load(fileName);
                kplEditor.FileName = fileName;

                var newTabPage = new TabPage(Path.GetFileName(fileName));
                newTabPage.ToolTipText = fileName;
                newTabPage.Controls.Add(kplEditor);

                tcMain.TabPages.Add(newTabPage);
                tcMain.SelectedTab = newTabPage;
            }
        }

        private void openKpQueriesFileMenuItem_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            opd.Filter = "kP Queries file (*.kpq)|*.kpq";

            if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileName = opd.FileName;

                var kplEditor = new KpQueriesEditor();
                kplEditor.Editor.Load(fileName);
                kplEditor.FileName = fileName;

                var newTabPage = new TabPage(Path.GetFileName(fileName));
                newTabPage.ToolTipText = fileName;
                newTabPage.Controls.Add(kplEditor);

                tcMain.TabPages.Add(newTabPage);
                tcMain.SelectedTab = newTabPage;
            }
        }

        private async void bPerformVerification_Click(object sender, EventArgs e)
        {
            FileInfo kplModelFile = new FileInfo(tbModelVerification.Text);

            /*
            FileInfo verificationModelFile = null;
            List<IProperty> nuSMVProperties = properties.NuSMVProperties;
            List<IProperty> spinProperties = properties.SpinProperties;
            if (nuSMVProperties.Count > 0)
            {
                verificationModelFile = new FileInfo(properties.NuSMVPath);
                await ModelCheckingManager.Instance.Verify(kplModelFile, nuSMVProperties, verificationModelFile, ModelCheckingTarget.NuSmv, new UiProgressMonitor(lMessageProgressMonitorVerification, lDetailsProgressMonitorVerification, pbProgressMonitorVerification));
            }
            if (spinProperties.Count > 0)
            {
                verificationModelFile = new FileInfo(properties.SpinPath);
                await ModelCheckingManager.Instance.Verify(kplModelFile, spinProperties, verificationModelFile, ModelCheckingTarget.Promela, new UiProgressMonitor(lMessageProgressMonitorVerification, lDetailsProgressMonitorVerification, pbProgressMonitorVerification));
            }
             */

            if (properties.PropertyControls != null)
            {
                foreach (var propertyControl in properties.PropertyControls)
                {
                    if (propertyControl.IsCheck)
                    {
                        if (propertyControl.ModelCheckerName == ModelChecker.NuSMV)
                        {
                            var verificationModelFile = new FileInfo(propertyControl.OutPath);
                            await ModelCheckingManager.Instance.Verify(kplModelFile, new[] { propertyControl.ExperimentProperty }, verificationModelFile, ModelCheckingTarget.NuSmv, new UiProgressMonitor(lMessageProgressMonitorVerification, lDetailsProgressMonitorVerification, pbProgressMonitorVerification));
                        }
                        else if (propertyControl.ModelCheckerName == ModelChecker.Spin)
                        {
                            var verificationModelFile = new FileInfo(propertyControl.OutPath);
                            await ModelCheckingManager.Instance.Verify(kplModelFile, new[] { propertyControl.ExperimentProperty }, verificationModelFile, ModelCheckingTarget.Promela, new UiProgressMonitor(lMessageProgressMonitorVerification, lDetailsProgressMonitorVerification, pbProgressMonitorVerification));
                        }
                    }
                }
            }
        }

        private async void bPerformSimulation_Click(object sender, EventArgs e)
        {
            FileInfo kplModelFile = new FileInfo(tbModelSimulation.Text);
            string output = tbOutputSimulation.Text;
            if (output.Substring(output.Length - 1, 1) != @"\")
                output = output + @"\";
            FileInfo outputFile = new FileInfo(output);

            SimulatorSettings settings = new SimulatorSettings();
            settings.Steps = (int)nudStepsSimulation.Value;
            settings.SkipSteps = (int)nudSkipStepsSimulation.Value;
            settings.Seed = (int)nudSeedSimulation.Value;
            settings.RecordRuleSelection = cbRecordRuleSelection.Checked;
            settings.RecordTargetSelection = cbRecordTargetSelection.Checked;
            settings.RecordInstanceCreation = cbRecordInstanceCreation.Checked;
            settings.RecordConfigurations = cbRecordConfigurations.Checked;
            settings.ConfigurationsOnly = cbConfigurationsOnly.Checked;

            if (TestPaths() == false)
                return;

            if (cbSimulatorSimulation.SelectedItem.ToString() == GetTypeDescription(SimulatorTarget.Kpw))
            {
                await SimulatorManager.Instance.Verify(kplModelFile, outputFile, settings, SimulatorTarget.Kpw, new UiProgressMonitor(lMessageProgressMonitorSimulation, lDetailsProgressMonitorSimulation, pbProgressMonitorSimulation));
            }
            else if (cbSimulatorSimulation.SelectedItem.ToString() == GetTypeDescription(SimulatorTarget.Flame))
            {
                await SimulatorManager.Instance.Verify(kplModelFile, outputFile, settings, SimulatorTarget.Flame, new UiProgressMonitor(lMessageProgressMonitorSimulation, lDetailsProgressMonitorSimulation, pbProgressMonitorSimulation));
            }
        }

        private bool TestPaths()
        {
            bool b = true;
            bool bFlameXparserPath = AppSettings.Instance.IsFlameXparserPath;
            bool bFlameLibmboardPath = AppSettings.Instance.IsFlameLibmboardPath;
            if (!bFlameXparserPath || !bFlameLibmboardPath)
            {
                int n = !bFlameXparserPath && !bFlameLibmboardPath ? 2 : 1;
                string s = "";
                if (n == 2)
                    s = "The Xparser and Libmboard paths are not set. To continue paths must be set. ";
                else
                    s = string.Format("The {0} paths path is not set. To continue the path must be set. ", !bFlameXparserPath ? "Xparser" : "Libmboard");
                s += "Do you want to make the settings now?";
                if (MessageBox.Show(this, s, "Flame", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    SettingsForm sf = new SettingsForm();
                    sf.ShowDialog(this);
                    bFlameXparserPath = AppSettings.Instance.IsFlameXparserPath;
                    bFlameLibmboardPath = AppSettings.Instance.IsFlameLibmboardPath;
                    b = bFlameXparserPath && bFlameLibmboardPath;
                }
                else
                    b = false;
            }
            return b;
        }

        private void HandleSaveClick(object sender, EventArgs e)
        {
            SaveDocument();
        }

        private void SaveDocument()
        {
            if (tcMain.SelectedTab != null)
            {
                var editor = tcMain.SelectedTab.Controls.Cast<Control>().FirstOrDefault() as BaseKpEditor;
                if (editor != null)
                {
                    editor.Save();
                    tcMain.SelectedTab.Text = Path.GetFileName(editor.FileName);
                    tcMain.SelectedTab.ToolTipText = editor.FileName;
                    AddSpaces(tcMain.SelectedTab);
                }
            }
        }

        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm();
            sf.ShowDialog(this);
        }

        private void HandleExitClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
