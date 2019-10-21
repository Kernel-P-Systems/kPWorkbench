using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class PItem {
        protected string name;
        protected string description;
        protected int id;
        protected Meta meta;

        public virtual string Name { get { return name; } set { name = value; } }
        public virtual string Description { get { return description; } set { description = value; } }
        public virtual Meta Meta { get { return meta; } protected set { meta = value; } }
        public virtual int Id { get { return id; } set { id = value; } }

        public PItem() : this("") { }

        public PItem(string name) : this(name, "") { }

        public PItem(string name, string description) {
            Name = name;
            Description = description;
        }

        public bool HasName() {
            return !String.IsNullOrEmpty(name);
        }

        public bool HasDescription() {
            return !String.IsNullOrEmpty(description);
        }

        public static void CopyProperties(PItem src, PItem dest) {
            dest.Name = src.Name;
            dest.Description = src.Description;
            dest.Id = src.Id;

            if(src.Meta != null) {
                dest.meta = new Meta();
                foreach (KeyValuePair<string, string> kv in src.meta) {
                    dest.meta[kv.Key] = kv.Value;
                }
            }
        }        
    }

    public class InvalidNameException : Exception {
        public InvalidNameException()
            : base("Cannot assign a null or empty string as 'Name' for this object.") {

        }
    }
}
