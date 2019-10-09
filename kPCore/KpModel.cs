using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore.Ltl;

namespace KpCore {

    /// <summary>
    /// A KpModel object may include much more than a Kernel P system: it may have certain meta information,
    /// directives and parameters for model simulation and verification and of course, properties to verify.
    /// </summary>
    public class KpModel {

        private KPsystem kp;
        public KPsystem KPsystem { get { return kp; } set { kp = value; } }

        private List<LtlProperty> ltlProperties;
        public List<LtlProperty> LtlProperties { get { return ltlProperties; } protected set { ltlProperties = value; } }

        public KpModel() {
            ltlProperties = new List<LtlProperty>();
        }

        public KpModel(KPsystem kps) : this() {
            kp = kps;
        }

    }
}
