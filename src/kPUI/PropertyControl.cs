using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KpExperiment.Model.Verification;

namespace KpUi
{
    public partial class PropertyControl : UserControl
    {
        public class PathEventArgs : EventArgs
        {
            public string Path { get; set; }
        }
        public event EventHandler<PathEventArgs> Path;
        private void OnPath(PathEventArgs e)
        {
            if(Path != null)
            {
                Path(this, e);
            }
        }
        public string OutPath { get { return tbPath.Text; } set { tbPath.Text = value; } }
        public string Property { get; private set; }
        public PropertyType Type { get; private set; }
        public IProperty ExperimentProperty { get; private set; }
        public string ModelCheckerName { get { return cbType.Text; } }
        public bool IsCheck { get { return cbCheck.Checked; } }
        public PropertyControl()
        {
            InitializeComponent();

        }
        public PropertyControl(int number, PropertyType propertyType, string property, IProperty experimentProperty)
            : this()
        {
            lProperty.Text = property;
            Property = property;
            Type = propertyType;
            ExperimentProperty = experimentProperty;
            cbType.Items.AddRange(ModelChecker.GetModelChecker(Type).ToArray());

            ToolTip toolTip = new ToolTip();
            toolTip.AutoPopDelay = 5000;
            toolTip.InitialDelay = 500;
            toolTip.ReshowDelay = 500;
            toolTip.ShowAlways = true;
            toolTip.SetToolTip(lProperty, Property);
        }

        private void bBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if(fbd.ShowDialog(this) == DialogResult.OK)
            {
                tbPath.Text = fbd.SelectedPath;
                PathEventArgs args = new PathEventArgs();
                args.Path = tbPath.Text;
                OnPath(args);
            }
        }
    }
}
