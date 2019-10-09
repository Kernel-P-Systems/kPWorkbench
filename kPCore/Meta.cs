using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class Meta : IEnumerable<KeyValuePair<string, string>> {
        private Dictionary<string, string> properties;

        public Meta() {

        }

        public virtual string this[string propertyName] {
            get {
                if (properties == null) {
                    return null;
                } else {
                    string val = null;
                    properties.TryGetValue(propertyName, out val);
                    return val;
                }
            }
            set {
                if (properties == null) {
                    properties = new Dictionary<string, string>();
                }
                properties[propertyName] = value;
            }
        }

        public bool UserDefinedPropertiesEmpty() {
            return properties == null || properties.Count == 0;
        }

        public IEnumerator GetEnumerator() {
            if (properties == null) {
                properties = new Dictionary<string, string>();
            }
            return properties.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator() {
            if (properties == null) {
                properties = new Dictionary<string, string>();
            }
            return properties.GetEnumerator();
        }
    }
}
