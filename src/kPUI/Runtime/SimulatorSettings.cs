using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUi.Simulation.Runtime
{
    public class SimulatorSettings
    {
        public int Steps { get; set; }
        public int Seed { get; set; }
        public int SkipSteps { get; set; }
        public bool RecordRuleSelection { get; set; }
        public bool RecordTargetSelection { get; set; }
        public bool RecordInstanceCreation { get; set; }
        public bool RecordConfigurations { get; set; }
        public bool ConfigurationsOnly { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Steps != 10)
                sb.AppendFormat("Steps={0}", Steps);
            if (Seed != 0)
                sb.AppendFormat("{0}Seed={1}", sb.Length > 0?" ":"", Seed);
            if (SkipSteps > 0)
                sb.AppendFormat("{0}SkipSteps={1}", sb.Length > 0 ? " " : "", SkipSteps);
            if (RecordRuleSelection == false)
                sb.AppendFormat("{0}RecordRuleSelection={1}", sb.Length > 0 ? " " : "", RecordRuleSelection);
            if (RecordTargetSelection == false)
                sb.AppendFormat("{0}RecordTargetSelection={1}", sb.Length > 0 ? " " : "", RecordTargetSelection);
            if (RecordInstanceCreation == false)
                sb.AppendFormat("{0}RecordInstanceCreation={1}", sb.Length > 0 ? " " : "", RecordInstanceCreation);
            if (RecordConfigurations == false)
                sb.AppendFormat("{0}RecordConfigurations={1}", sb.Length > 0 ? " " : "", RecordConfigurations);
            if (ConfigurationsOnly == true)
                sb.AppendFormat("{0}ConfigurationsOnly={1}", sb.Length > 0 ? " " : "", ConfigurationsOnly);
            return sb.ToString();
        }
    }
}
