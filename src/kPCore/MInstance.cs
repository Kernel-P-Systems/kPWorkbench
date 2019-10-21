using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class MInstance : LabelledPItem {

        //one shouldn't be able to assign a multiset to an instance directly
        //if there are exceptional cases, then a subclass will have access to this member
        protected Multiset multiset;
        public Multiset Multiset { get { return multiset; } protected set { multiset = value; } }

        protected List<MInstance> connections;
        public List<MInstance> Connections { get { return connections; } protected set { connections = value; } }

        private bool isDissolved;
        private bool isDivided;
        private bool isCreated;
        public bool IsDissolved { get { return isDissolved; } set { isDissolved = value; } }
        public bool IsDivided { get { return isDivided; } set { isDivided = value; } }
        public bool IsCreated { get { return isCreated; } set { isCreated = value; } }

        public MInstance()
            : this(new Multiset()) {
        }

        public MInstance(Multiset ms) : base() {
            multiset = ms;
            connections = new List<MInstance>();
        }

        public bool ContainsLabels(params string[] labels) {

            if (!HasLabels()) {
                return false;
            }

            foreach (string l in labels) {
                if (!Labels.Contains(l)) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Performes a reference change to a new non-null multiset of objects; aka, shallow copy
        /// </summary>
        /// <param name="newMultiset"></param>
        public void ReplaceMultiset(Multiset newMultiset) {
            if (newMultiset == null) {
                throw new ArgumentNullException("newMultiset");
            }
            Multiset = newMultiset;
        }

        public void ConnectTo(MInstance instance) {
            if (instance != null) {
                Connections.Add(instance);
            }
        }

        public void ConnectBidirectionalTo(MInstance instance) {
            if (instance != null) {
                Connections.Add(instance);
                instance.Connections.Add(this);
            }
        }

        public void DisconnectBidirectionalFrom(MInstance instance) {
            if (instance != null) {
                Connections.Remove(instance);
                instance.Connections.Remove(this);
            }
        }

        /// <summary>
        /// Deep Copy of an membrane instance. VERY IMPORTANT!!! This will not coppy the connections to other instances, nor clone
        /// those instances. This must be done at a higher level where access to all instances of P system is available.
        /// </summary>
        /// <returns></returns>
        public MInstance Clone() {
            MInstance clone = new MInstance(multiset.Clone());
            PItem.CopyProperties(this, clone);
            LabelledPItem.CopyLabels(this, clone);
            
            return clone;
        }
    }
}
