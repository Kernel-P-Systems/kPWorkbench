using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class TargetedMultiset {

        protected IInstanceIdentifier target;
        public IInstanceIdentifier Target { get { return target; } set { target = value; } }

        protected Multiset multiset;
        public Multiset Multiset { get { return multiset; } set { multiset = value; } }

        public TargetedMultiset(IInstanceIdentifier identifier) {
            multiset = new Multiset();
            target = identifier;
        }

        public TargetedMultiset(InstanceIdentifier identifier, Multiset multiset) {
            Target = identifier;
            Multiset = multiset;
        }

        public bool IsEmpty() {
            return multiset.IsEmpty();
        }
    }

    public class TypeTargetedMultiset : TargetedMultiset {

        private MType mt;
        public MType MType { get { return mt; } set { mt = value; } }

        public TypeTargetedMultiset(MType mtype)
            : this(mtype, new Multiset()) {

        }

        public TypeTargetedMultiset(MType mtype, Multiset multiset) :
            base(new InstanceIdentifier(InstanceIndicator.TYPE, mtype.Name), multiset) {
            MType = mtype;
        }
    }
}
