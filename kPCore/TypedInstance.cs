using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {

    /// <summary>
    /// This class represents an membrane instance - type tuple;
    /// an instance that is bound to a type which is guaranteed to be non-null. It is not
    /// intended to be used in MTypes since adding and removing instances requires updates on both objects to preserve 
    /// consistency. (i.e. each time the MType property of an instance is changed, the MType object that contains this 
    /// instance should remove it from its registry and viceversa).
    /// </summary>
    public class TypedInstance {
        private MType type;
        private MInstance instance;
        public MType Type {
            get { return type; }
            set {
                if (value == null) {
                    throw new ArgumentNullException();
                }
                type = value;
            }
        }

        public MInstance Instance {
            get { return instance; }
            set { instance = value; }
        }
    
        public TypedInstance(MType mtype, MInstance minstance) {
            Type = mtype;
            Instance = minstance;
        }
    }
}
