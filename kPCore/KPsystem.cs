using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class KPsystem : PItem {

        private Dictionary<string, MType> types;
        public Dictionary<string, MType>.ValueCollection Types { get { return types.Values; } }

        public KPsystem()
            : base() {
            types = new Dictionary<string, MType>();
        }

        public IEnumerator<KeyValuePair<string, MType>> GetEnumerator() {
            return types.GetEnumerator();
        }

        public MType this[string typeName] {
            get {
                return EnsureType(typeName);
            }
            set {
                types[typeName] = value;
            }
        }

        public MType EnsureType(string typeName) {
            MType t = null;
            types.TryGetValue(typeName, out t);
            if (t == null) {
                t = new MType(typeName);
                types.Add(typeName, t);
            }
            return t;
        }

        public void AddType(MType t) {
            types.Add(t.Name, t);
        }

        public void RemoveType(MType t) {
            types.Remove(t.Name);
        }

        public void RemoveType(string typeName) {
            types.Remove(typeName);
        }

        public void Clear() {
            types.Clear();
        }

        public MType TryGetType(string typeName) {
            MType x = null;
            types.TryGetValue(typeName, out x);

            return x;
        }

        /// <summary>
        /// Deep copy
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public KPsystem Clone() {
            KPsystem clone = new KPsystem();
            PItem.CopyProperties(this, clone);

            Dictionary<MInstance, MInstance> instanceCloneRegistry = new Dictionary<MInstance, MInstance>();
            foreach (KeyValuePair<string, MType> kv in types) {
                MType mtype = kv.Value;
                MType typeClone = new MType(mtype.Name);
                PItem.CopyProperties(mtype, typeClone);
                foreach (MInstance instance in mtype.Instances) {
                    MInstance instanceClone = instance.Clone();
                    instanceCloneRegistry.Add(instance, instanceClone);
                    typeClone.Instances.Add(instanceClone);
                }

                clone.AddType(typeClone);
            }

            foreach (KeyValuePair<MInstance, MInstance> kv in instanceCloneRegistry) {
                MInstance tar = kv.Value;
                foreach (MInstance connection in kv.Key.Connections) {
                    tar.Connections.Add(instanceCloneRegistry[connection]);
                }
            }

            return clone;
        }

        public string ToString(bool generateIds = false) {
            string nl = System.Environment.NewLine;
            if (generateIds) {
                new KpIdMap(this);
            }
            StringBuilder buf = new StringBuilder();
            buf.AppendFormat("kP system #{0}", Id);
            if (HasName()) {
                buf.AppendFormat(". {0}", Name);
            }
            buf.AppendLine();
            if (HasDescription()) {
                buf.AppendFormat("* {0} *", Description);
                buf.AppendLine(nl);
            }
            foreach (MType mt in Types) {
                buf.AppendLine().AppendFormat("Type {0}", mt.Name);
                buf.AppendLine();
                foreach (MInstance instance in mt.Instances) {
                    buf.AppendFormat("- Instance #{0}{1}: {{", instance.Id,
                        (instance.HasName() ? "<" + instance.Name + ">" : ""));
                    foreach (KeyValuePair<string, int> kv in instance.Multiset) {
                        buf.AppendFormat("{0}{1} ", (kv.Value == 1 ? "" : kv.Value.ToString()), kv.Key);
                    }
                    buf.Replace(" ", "", buf.Length - 1, 1).AppendLine("}");
                    if (instance.Connections.Count > 0) {
                        buf.Append(" - Connections: ");
                        foreach (MInstance connection in instance.Connections) {
                            buf.AppendFormat("#{0}{1}, ", connection.Id,
                                (connection.HasName() ? "<" + connection.Name + ">" : ""));
                        }
                        buf.Replace(", ", "", buf.Length - 2, 2).AppendLine();
                    }
                }
                ExecutionStrategy ex = mt.ExecutionStrategy;
                while (ex != null) {
                    //print Rules
                    ex = ex.Next;
                }
            }


            return buf.ToString();
        }
    }

    public class KPsystemNullException : Exception {

        public KPsystemNullException()
            : base("K P system model was not specified. Please provide a non-null model.") {
        }
    }
}
