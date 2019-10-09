using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class MTuple {

        private string obj;
        private int multiplicity;

        public string Obj { get { return obj; } set { obj = value; } }
        public int Multiplicity { get { return multiplicity; } set { multiplicity = value; } }

        public MTuple(string obj, int multiplicity) {
            this.obj = obj;
            this.multiplicity = multiplicity;
        }
    }
}
