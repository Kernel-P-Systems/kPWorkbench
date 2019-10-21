using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSimulation {
    public class KpSimulation {

        public string SimulatorName { get; set; }
        public string SimulatorVersion { get; set; }
        public string Author { get; set; }
        public string SystemInfo { get; private set; }

        public int Seed { get; set; }

        public KpSimulation() {
            
        }
    }

    public class KpSimulationStep {
        public KPsystem KPsystem { get; set;}
        public int Step { get; set; }
    }
}
