using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using KpExperiment.Model;
using KpExperiment.Model.Verification;

namespace KpUi
{
    public class PropertyList
    {
        private List<PropertyControl> properties;
        private TableLayoutPanel tlpPropertyHeaders;
        private TableLayoutPanel tlpProperties;
        private bool isSetPath;

        public List<IProperty> SpinProperties
        {
            get
            {
                List<IProperty> experimentProperties = new List<IProperty>();
                foreach (PropertyControl property in properties)
                {
                    if (property.IsCheck && property.ModelCheckerName == ModelChecker.Spin)
                        experimentProperties.Add(property.ExperimentProperty);
                }
                return experimentProperties;
            }
        }

        public string SpinPath
        {
            get
            {
                string path = "";
                foreach (PropertyControl property in properties)
                {
                    if (property.IsCheck && property.ModelCheckerName == ModelChecker.Spin && string.IsNullOrEmpty(path))
                        path = property.OutPath;
                }
                if (string.IsNullOrEmpty(path))
                {
                    if (path.Substring(path.Length - 1, 1) != @"\")
                        path = path + @"\";
                    path = path + "spin";
                }
                return path;
            }
        }

        public List<IProperty> NuSMVProperties
        {
            get
            {
                List<IProperty> experimentProperties = new List<IProperty>();
                foreach (PropertyControl property in properties)
                {
                    if (property.IsCheck && property.ModelCheckerName == ModelChecker.NuSMV)
                        experimentProperties.Add(property.ExperimentProperty);
                }
                return experimentProperties;
            }
        }

        public string NuSMVPath
        {
            get
            {
                string path = "";

                foreach (PropertyControl property in properties)
                {
                    if (property.IsCheck && property.ModelCheckerName == ModelChecker.NuSMV && string.IsNullOrEmpty(path))
                        path = property.OutPath;
                }

                if (string.IsNullOrEmpty(path))
                {
                    if (path.Substring(path.Length - 1, 1) != @"\")
                        path = path + @"\";
                    path = path + "nuSMV";
                }

                return path;
            }
        }

        public List<PropertyControl> PropertyControls
        {
            get { return properties; }
        }

        public PropertyList(TableLayoutPanel tlpPropertyHeaders, TableLayoutPanel tlpProperties)
        {
            this.tlpPropertyHeaders = tlpPropertyHeaders;
            this.tlpProperties = tlpProperties;
            tlpPropertyHeaders.Visible = false;
            isSetPath = true;
            properties = new List<PropertyControl>();
        }

        public void Load(List<string> lines, Experiment kpExperiment)
        {
            Regex regex = new Regex(@"^\s*([a-zA-Z]+)\s*:\s*(.+)(\s*;).*$");
            int number = 0;
            tlpProperties.Controls.Clear();
            tlpProperties.RowStyles.Clear();
            tlpProperties.RowCount = 0;
            properties.Clear();
            List<ILtlProperty> LtlProperties = new List<ILtlProperty>(kpExperiment.LtlProperties);
            List<ICtlProperty> CtlProperties = new List<ICtlProperty>(kpExperiment.CtlProperties);
            foreach (string s in lines)
            {
                Match match = regex.Match(s);
                if (match.Success)
                {
                    PropertyType type = match.Groups[1].Value.ToUpper() == PropertyType.LTL.ToString() ? PropertyType.LTL : PropertyType.CTL;
                    IProperty experimentProperty = null;
                    if (type == PropertyType.LTL)
                    {
                        experimentProperty = LtlProperties[0];
                        LtlProperties.RemoveAt(0);
                    }
                    else
                        if (type == PropertyType.CTL)
                        {
                            experimentProperty = CtlProperties[0];
                            CtlProperties.RemoveAt(0);
                        }
                    Add(++number, type, match.Groups[2].Value, experimentProperty);
                }
            }
            tlpPropertyHeaders.Visible = properties.Count > 0;
        }

        private void onPath(object o, PropertyControl.PathEventArgs e)
        {
            if (isSetPath)
            {
                isSetPath = false;
                foreach (PropertyControl property in properties)
                {
                    if (property != o)
                    {
                        property.OutPath = e.Path;
                    }
                }
            }
        }

        private void Add(int number, PropertyType propertyType, string property, IProperty experimentProperty)
        {
            PropertyControl propertyControl = new PropertyControl(number, propertyType, property, experimentProperty);
            propertyControl.Path += onPath;
            propertyControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            propertyControl.Location = new System.Drawing.Point(3, 3);
            propertyControl.Size = new System.Drawing.Size(700, 37);
            propertyControl.TabIndex = 0;

            properties.Add(propertyControl);

            tlpProperties.Controls.Add(propertyControl, -1, tlpProperties.RowCount);
            tlpProperties.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        }
    }
}
