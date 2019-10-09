using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class MType : PItem {
        private List<MInstance> instances;
        private ExecutionStrategy strategy;

        public ExecutionStrategy ExecutionStrategy {
            get { return strategy; }
            set {
                if (value == null) { throw new ArgumentNullException(); }
                strategy = value;
            }
        }
        public List<MInstance> Instances { get { return instances; } }

        public MType()
            : this("default") {

        }

        public MType(string typeName)
            : this(typeName, "") {

        }

        public MType(string typeName, string description)
            : base(typeName, description) {
            instances = new List<MInstance>();
            strategy = new ExecutionStrategy();
        }

        public MType Append(MType mtype) {
            foreach (MInstance instance in mtype.Instances) {
                instances.Add(instance);
            }
            if (strategy.IsEmpty()) {
                strategy = mtype.ExecutionStrategy;
            } else {
                strategy.Next = mtype.ExecutionStrategy;
            }

            return this;
        }

        /// <summary>
        /// Deep copy
        /// </summary>
        /// <returns></returns>
        public MType Clone() {
            MType clone = new MType(this.Name);
            PItem.CopyProperties(this, clone);

            foreach (MInstance instance in instances) {
                clone.Instances.Add(instance.Clone());
            }

            return clone;
        }
    }
}
