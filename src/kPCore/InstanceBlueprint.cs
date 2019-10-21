using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class InstanceBlueprint {
        private MType mtype;
        public MType Type { get { return mtype; } set { mtype = value; } }

        private Multiset multiset;
        public Multiset Multiset { get { return multiset; } private set { multiset = value; } }

        public InstanceBlueprint(MType mt)
            : this(mt, new Multiset()) {
        }

        public InstanceBlueprint(MType mt, Multiset ms) {
            if (mt == null) {
                throw new ArgumentNullException("mt", "MType cannot be null");
            }
            mtype = mt;
            multiset = ms;
        }
    }
}
