using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public sealed class Multiset {
        private Dictionary<string, int> ms;

        public Multiset() {
            ms = new Dictionary<string, int>();
        }

        public Multiset(string obj, int multiplicity)
            : this() {
            Add(obj, multiplicity);
        }

        public Multiset(params string[] objects)
            : this() {
            if (objects != null) {
                foreach (string obj in objects) {
                    Add(obj, 1);
                }
            }
        }

        public int this[string obj] {
            get {
                if (obj == null) {
                    return 0;
                }
                int x = 0;
                ms.TryGetValue(obj, out x);
                return x;
            }
            set {
                ms[obj] = value;
            }
        }

        public int Count {
            get {
                return ms.Count;
            }
        }

        public IEnumerable<string> Objects {
            get {
                return ms.Keys;
            }
        }

        public IEnumerable<int> ParikhVector {
            get { return ms.Values; }
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator() {
            return ms.GetEnumerator();
        }

        public bool IsEmpty() {
            foreach (int x in ms.Values) {
                if (x > 0) {
                    return false;
                }
            }

            return true;
        }

        public bool Contains(Multiset multiset) {
            foreach (KeyValuePair<string, int> kv in multiset) {
                //assume multisets are normalised
                int v = 0;
                ms.TryGetValue(kv.Key, out v);

                if (v < kv.Value) {
                    return false;
                }
            }
            return true;
        }

        public bool Equals(Multiset multiset) {
            return Contains(multiset) && multiset.Contains(this);
        }

        public Multiset Add(Multiset multiset) {
            foreach (KeyValuePair<string, int> kv in multiset) {
                Add(kv.Key, kv.Value);
            }

            return this;
        }

        public Multiset Add(string obj, int multiplicity) {
            return Add(obj, multiplicity, true);
        }

        public Multiset Add(string obj, int multiplicity, bool ignoreNullValues) {
            if (multiplicity < 0) {
                throw new InvalidMultiplicityException();
            }

            if (multiplicity == 0 && ignoreNullValues) {
                return this;
            }

            int x = 0;
            ms.TryGetValue(obj, out x);
            ms[obj] = x + multiplicity;

            return this;
        }

        public Multiset Subtract(String obj, int multiplicity) {
            if (multiplicity < 0) {
                throw new InvalidMultiplicityException();
            }
            int x = 0;
            ms.TryGetValue(obj, out x);
            if (x == multiplicity) {
                ms.Remove(obj);
            } else {
                ms[obj] = x - multiplicity;
            }

            return this;
        }

        public Multiset Subtract(Multiset multiset) {
            foreach (KeyValuePair<string, int> kv in multiset) {
                Subtract(kv.Key, kv.Value);
            }

            return this;
        }

        public Multiset Clear() {
            ms.Clear();
            return this;
        }

        public static Multiset operator +(Multiset ms1, Multiset ms2) {
            Multiset ms = new Multiset();
            ms.Add(ms1).Add(ms2);
            return ms;
        }

        public static Multiset operator -(Multiset ms1, Multiset ms2) {
            Multiset ms = new Multiset();
            ms.Add(ms1).Subtract(ms2);
            return ms;
        }

        public static bool operator >=(Multiset ms1, Multiset ms2) {
            return ms1.Contains(ms2);
        }

        public static bool operator <(Multiset ms1, Multiset ms2) {
            return !(ms1 >= ms2);
        }

        public static bool operator >(Multiset ms1, Multiset ms2) {
            return !(ms1 <= ms2);
        }

        public static bool operator <=(Multiset ms1, Multiset ms2) {
            return ms2 >= ms1;
        }

        public override string ToString() {

            StringBuilder buf = new StringBuilder();
            buf.Append("ms[");
            foreach (KeyValuePair<String, int> kv in ms) {
                buf.Append("(").Append(kv.Key).Append(", ").Append(kv.Value).Append("), ");
            }
            if (buf.Length > 3) {
                buf.Remove(buf.Length - 2, 2);
            }
            buf.Append("]");

            return buf.ToString();
        }

        public Multiset Clone() {
            Multiset clone = new Multiset();
            foreach (KeyValuePair<string, int> kv in ms) {
                clone.Add(kv.Key, kv.Value);
            }

            return clone;
        }

        public bool IsDisjointFrom(Multiset multiset) {
            foreach (KeyValuePair<string, int> kv in ms) {
                int x = 0;
                if (multiset.ms.TryGetValue(kv.Key, out x)) {
                    if (x > 0) {
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class InvalidMultiplicityException : Exception {

        public InvalidMultiplicityException()
            : base("Multiplicity must be a valid positive integer") {

        }

        public InvalidMultiplicityException(string msg)
            : base(msg) {
        }
    }

    public class MultisetNullException : Exception {
        public MultisetNullException()
            : base("Multiset is null") {
        }

        public MultisetNullException(string msg)
            : base(msg) {
        }
    }
}
