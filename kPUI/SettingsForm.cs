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
            tbXparserPath.Text = AppSettings.Instance.FlameXparserPath + (AppSettings.Instance.FlameXparserName ?? "");
            tbLibmboardPath.Text = AppSettings.Instance.FlameLibmboardPath;
        }

        private void bBrowseXparserPath_Click(object sender, EventArgs e)
        {
            var opd = new OpenFileDialog();
            opd.Filter = "Xparser (xparser.exe)|xparser.exe";

            if (opd.ShowDialog() == DialogResult.OK)
            {
                tbXparserPath.Text = opd.FileName;
            }
        }

        private void bBrowseLibmboardPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                tbLibmboardPath.Text = fbd.SelectedPath;
            }
        }

        private void bOK_Click(object sender, EventArgs e)
        {
            bool bFlameXparserPath = tbXparserPath.Text.Length > 0;
            bool bFlameLibmboardPath = tbLibmboardPath.Text.Length > 0;
            if (!bFlameXparserPath || !bFlameLibmboardPath)
            {
                int n = !bFlameXparserPath && !bFlameLibmboardPath ? 2 : 1;
                string s = "";
                if (n == 2)
                    s = "The Xparser and Libmboard paths are not set. ";
                else
                    s = string.Format("The {0} paths path is not set. ", !bFlameXparserPath ? "Xparser" : "Libmboard");
                s += "Do you want to leave the settings anyway?";
                if (MessageBox.Show(this, s, "Flame", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    Close();
                }
            }
            else
            {              
                string stbXparserPath = tbXparserPath.Text;
                string sXparserPath = stbXparserPath.Substring(0, stbXparserPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                string sXparserName = stbXparserPath.Substring(sXparserPath.Length, stbXparserPath.Length - sXparserPath.Length);
                
                AppSettings.Instance.FlameXparserPath = sXparserPath;
                AppSettings.Instance.FlameXparserName = sXparserName;
                AppSettings.Instance.FlameLibmboardPath = tbLibmboardPath.Text;
                Close();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
