using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSimulation {
    public class KpSimulationParams {

        public int SkipSteps { get; set; }
        public int Steps { get; set; }
        public bool RecordRuleSelection { get; set; }
        public bool RecordTargetSelection { get; set; }
        public bool RecordInstanceCreation { get; set; }
        public bool RecordConfigurations { get; set; }

        public bool ConfigurationsOnly {
            get {
                return RecordConfigurations &&
                    !(RecordRuleSelection || RecordTargetSelection || RecordInstanceCreation);
            }
            set {
                RecordConfigurations = true;
                RecordRuleSelection = false;
                RecordTargetSelection = false;
                RecordInstanceCreation = false;
            }
        }

        public bool PrintDissolvedCompartments { get; set; }

        public int Seed { get; set; }

        public static KpSimulationParams Default() {
            return new KpSimulationParams() {
                SkipSteps = 0,
                Steps = 10,
                RecordConfigurations = true,
                RecordRuleSelection = true,
                RecordTargetSelection = true,
                RecordInstanceCreation = true,
                PrintDissolvedCompartments = true
            };
        }
    }
}
