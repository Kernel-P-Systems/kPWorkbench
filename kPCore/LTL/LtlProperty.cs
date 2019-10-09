using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore.Ltl {


    public abstract class LtlProperty : PItem {

        private LtlExpression ltlExpression;
        public LtlExpression LtlExpression { get { return ltlExpression; } set { ltlExpression = value; } }

        public LtlProperty(string name) {
            if (String.IsNullOrEmpty(name)) {
                throw new InvalidNameException();
            }
            Name = name;
        }
    }
}
