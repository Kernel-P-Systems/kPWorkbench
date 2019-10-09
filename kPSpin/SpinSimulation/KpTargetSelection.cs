using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin.Simulation {
    public class KpTargetSelection : KpSimulationProduct {
        public Rule Rule { get; set; }
        public MType MType { get; set; }
        public MInstance Instance { get; set; }

        private Dictionary<MInstance, MType> targets;
        public Dictionary<MInstance, MType> Target { get { return targets; } protected set { targets = value; } }

        public KpTargetSelection() {
            targets = new Dictionary<MInstance,MType>();
        }

        public KpTargetSelection(Rule r, MType mt, MInstance instance) : this() {
            Rule = r;
            MType = mt;
            Instance = instance;
        }
    }
}
