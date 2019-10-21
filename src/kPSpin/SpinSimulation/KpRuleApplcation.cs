using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin.Simulation {
    public class KpRuleApplcation : KpSimulationProduct {
        public Rule Rule { get; set; }
        public MType MType { get; set; }
        public MInstance Instance { get; set; }

        public KpRuleApplcation() {

        }

        public KpRuleApplcation(Rule r, MType mt, MInstance instance) {
            Rule = r;
            MType = mt;
            Instance = instance;
        }
    }
}
