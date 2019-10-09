using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin
{
    public class PromelaTranslationConfig
    {
        public int MaxInstances { get; set; }
        public int MaxDivisions { get; set; }
        public int MaxLinks { get; set; }
        public int MaxSteps { get; set; }

        public bool LinksEnabled { get; set; }
        public bool DivisionEnabled { get; set; }
        public bool DissolutionEnabled { get; set; }

        public bool PrintRuleExecution { get; set; }
        public bool PrintConfiguration { get; set; }
        public bool PrintTargetSelection { get; set; }
        public bool PrintLinks { get; set; }

        public static PromelaTranslationConfig Default()
        {
            return new PromelaTranslationConfig()
            {
                MaxSteps = 20,
                MaxDivisions = 20,
                MaxInstances = 20,
                MaxLinks = 100,
                LinksEnabled = true,
                DivisionEnabled = true,
                DissolutionEnabled = true,
                PrintRuleExecution = true,
                PrintConfiguration = true,
                PrintTargetSelection = true,
                PrintLinks = true
            };
        }
    }

    public class PromelaTranslationParams : PromelaTranslationConfig, IEnumerable<KeyValuePair<string, string>>
    {
        public bool LogEnabled
        {
            get
            {
                return PrintConfiguration || (PrintLinks && LinksEnabled);
            }
        }

        public const int UNLIMITED = -1;

        private Dictionary<string, string> userParams;

        private PromelaTranslationParams()
        {
            userParams = new Dictionary<string, string>();
        }

        public static PromelaTranslationParams Default()
        {
            return new PromelaTranslationParams()
            {
                MaxSteps = 20,
                MaxDivisions = 20,
                MaxInstances = 20,
                MaxLinks = 100,
                LinksEnabled = true,
                DivisionEnabled = true,
                DissolutionEnabled = true,
                PrintRuleExecution = true,
                PrintConfiguration = true,
                PrintTargetSelection = true,
                PrintLinks = true
            };
        }

        public string this[string key]
        {
            get
            {
                return userParams[key];
            }
            set
            {
                userParams[key] = value;
            }
        }

        public int Count { get { return userParams.Count; } }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return userParams.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return userParams.GetEnumerator();
        }
    }
}
