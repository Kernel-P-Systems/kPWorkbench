using KpCore;
using KpUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin.Simulation {
    public class KpSystemConfiguration {

        private int step;
        public int Step { get { return step; } set { step = value; } }

        private KPsystem kp;
        public KPsystem KPsystem { get { return kp; } set { kp = value; } }

        public KpSystemConfiguration() {
            step = 0;
        }
    }
}
