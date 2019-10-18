using KpUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KpUi
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Shown(object sender, EventArgs e)
        {
            tbSpinPath.Text = AppSettings.Instance.SpinPath;
            tbGccPath.Text = AppSettings.Instance.GccPath;
            tbNusmvPath.Text = AppSettings.Instance.NuSmvPath;
            tbXparserPath.Text = AppSettings.Instance.FlameXparserPath + (AppSettings.Instance.FlameXparserName ?? "");
            tbLibmboardPath.Text = AppSettings.Instance.FlameLibmboardPath;
        }

        private void bBrowseXparserPath_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            if (opd.ShowDialog() == DialogResult.OK)
            {
                tbXparserPath.Text = opd.FileName;
            }
        }

        private void bBrowseLibmboardPath_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                tbLibmboardPath.Text = fbd.SelectedPath;
            }
        }

        private void bBrowseNusmvPath_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            if (opd.ShowDialog() == DialogResult.OK)
            {
                tbNusmvPath.Text = opd.FileName;
            }
        }

        private void bBrowseSpinPath_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            if (opd.ShowDialog() == DialogResult.OK)
            {
                tbSpinPath.Text = opd.FileName;
            }
        }

        private void bBrowseGccPath_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            if (opd.ShowDialog() == DialogResult.OK)
            {
                tbGccPath.Text = opd.FileName;
            }
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            var missingSettings = new List<String>();

            if (String.IsNullOrEmpty(tbSpinPath.Text))
            {
                missingSettings.Add("Spin");
            }

            if (String.IsNullOrEmpty(tbGccPath.Text))
            {
                missingSettings.Add("GCC");
            }

            if (String.IsNullOrEmpty(tbNusmvPath.Text))
            {
                missingSettings.Add("NuSMV");
            }

            if (String.IsNullOrEmpty(tbXparserPath.Text))
            {
                missingSettings.Add("XParser");
            }

            if (String.IsNullOrEmpty(tbLibmboardPath.Text))
            {
                missingSettings.Add("Libmboard");
            }


            if (missingSettings.Count > 0)
            {
                string message = String.Format("The paths for the following external tools have not been configured: {0}. \nDo you want to leave the Settings anyway?", String.Join(", ", missingSettings));


                if (MessageBox.Show(this, message, "Paths not set", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    Close();
                }
            }

                string xParserPath = tbXparserPath.Text.Substring(0, tbXparserPath.Text.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                string xParserName = tbXparserPath.Text.Substring(xParserPath.Length, tbXparserPath.Text.Length - xParserPath.Length);

                AppSettings.Instance.SpinPath = tbSpinPath.Text;
                AppSettings.Instance.GccPath = tbGccPath.Text;
                AppSettings.Instance.NuSmvPath = tbNusmvPath.Text;
                AppSettings.Instance.FlameLibmboardPath = tbLibmboardPath.Text;
                AppSettings.Instance.FlameXparserPath = xParserPath;
                AppSettings.Instance.FlameXparserName = xParserName;
                Close();
        }
    }
}
