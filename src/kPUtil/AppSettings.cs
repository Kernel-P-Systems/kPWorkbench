using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil
{
    public class AppSettings
    {
        private static AppSettings instance;
        private System.Configuration.Configuration config;
        private string spinPath = null;
        private string gccPath = null;
        private string spinOptions = null;
        private string nuSmvPath = null;
        private string flameXparserPath = null;
        private string flameXparserName = null;
        private string flameLibmboardPath = null;

        private AppSettings()
        {
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string configFile = System.IO.Path.Combine(appPath, "App.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
        }

        public static AppSettings Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new AppSettings();
                }
                return instance;
            }
        }

        public bool IsSpinPath
        {
            get
            {
                return !String.IsNullOrEmpty(SpinPath);
            }
        }

        public string SpinPath
        {
            get
            {
                if (spinPath == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["SpinPath"];
                    spinPath = setting?.Value;
                }

                return spinPath;
            }
            set
            {
                spinPath = value;
                if (config.AppSettings.Settings["SpinPath"] == null)
                {
                    config.AppSettings.Settings.Add("SpinPath", spinPath);
                }
                else
                {
                    config.AppSettings.Settings["SpinPath"].Value = spinPath;
                }
                config.Save();
            }
        }

        public string SpinOptions
        {
            get
            {
                if (spinOptions == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["SpinOptions"];
                    spinOptions = setting?.Value;
                }

                return spinOptions;
            }
            set
            {
                spinOptions = value;
                if (config.AppSettings.Settings["SpinOptions"] == null)
                {
                    config.AppSettings.Settings.Add("SpinOptions", spinOptions);
                }
                else
                {
                    config.AppSettings.Settings["SpinOptions"].Value = spinOptions;
                }
                config.Save();
            }
        }

        public bool IsGccPath
        {
            get
            {
                return !String.IsNullOrEmpty(GccPath);
            }
        }

        public string GccPath
        {
            get
            {
                if (gccPath == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["GccPath"];
                    gccPath = setting?.Value;
                }

                return gccPath;
            }
            set
            {
                gccPath = value;
                if (config.AppSettings.Settings["GccPath"] == null)
                {
                    config.AppSettings.Settings.Add("GccPath", gccPath);
                }
                else
                {
                    config.AppSettings.Settings["GccPath"].Value = gccPath;
                }
                config.Save();
            }
        }

        public bool IsNuSmvPath
        {
            get
            {
                return !String.IsNullOrEmpty(NuSmvPath);
            }
        }

        public string NuSmvPath
        {
            get
            {
                if (nuSmvPath == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["NuSmvPath"];
                    nuSmvPath = setting?.Value;
                }

                return nuSmvPath;
            }
            set
            {
                nuSmvPath = value;
                if (config.AppSettings.Settings["NuSmvPath"] == null)
                {
                    config.AppSettings.Settings.Add("NuSmvPath", nuSmvPath);
                }
                else
                {
                    config.AppSettings.Settings["NuSmvPath"].Value = nuSmvPath;
                }
                config.Save();
            }
        }

        public bool IsFlameXparserPath
        {
            get
            {
                return FlameXparserPath != null;
            }
        }

        public string FlameXparserPath
        {
            get
            {
                if (flameXparserPath == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["FlameXparserPath"];
                    flameXparserPath = setting == null ? null : setting.Value;
                }
                return flameXparserPath;
            }
            set
            {
                string s = value;
                if (s != null && s.Length > 0 && s.Substring(s.Length - 1, 1) != Path.DirectorySeparatorChar.ToString())
                {
                    s += Path.DirectorySeparatorChar;
                }
                flameXparserPath = s;
                if (config.AppSettings.Settings["FlameXparserPath"] == null)
                {
                    config.AppSettings.Settings.Add("FlameXparserPath", flameXparserPath);
                }
                else
                {
                    config.AppSettings.Settings["FlameXparserPath"].Value = flameXparserPath;
                }
                config.Save();
            }
        }

        public string FlameXparserName
        {
            get
            {
                if (flameXparserName == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["FlameXparserName"];
                    flameXparserName = setting == null ? null : setting.Value;
                }
                return flameXparserName;
            }
            set
            {
                flameXparserName = value;
                if (config.AppSettings.Settings["FlameXparserName"] == null)
                {
                    config.AppSettings.Settings.Add("FlameXparserName", flameXparserName);
                }
                else
                {
                    config.AppSettings.Settings["FlameXparserName"].Value = flameXparserName;
                }
                config.Save();
            }
        }
        public bool IsFlameLibmboardPath
        {
            get
            {
                return FlameLibmboardPath != null;
            }
        }
        public string FlameLibmboardPath
        {
            get
            {
                if (flameLibmboardPath == null)
                {
                    KeyValueConfigurationElement setting = config.AppSettings.Settings["FlameLibmboardPath"];
                    flameLibmboardPath = setting == null ? null : setting.Value;
                }
                return flameLibmboardPath;
            }
            set
            {
                string s = value;
                while (s != null && s.Length > 0 && s.Substring(s.Length - 1, 1) == @"\")
                {
                    s = s.Substring(0, s.Length - 1);
                }
                flameLibmboardPath = s;
                if(config.AppSettings.Settings["FlameLibmboardPath"] == null)
                {
                    config.AppSettings.Settings.Add("FlameLibmboardPath", flameLibmboardPath);
                }
                else
                {
                    config.AppSettings.Settings["FlameLibmboardPath"].Value = flameLibmboardPath;
                }
                config.Save();
            }
        }
    }
}
