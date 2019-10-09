using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore.Ltl {
    public class Numeric {

    }

    public class IntLiteral : Numeric {
        private int val;
        public int Value { get { return val; } set { val = value; } }
    }

    public class ObjectMultiplicity : Numeric {
        private string objectName;
    }

    public class ArithmeticExpression : Numeric {
        public Numeric Lhs { get; set; }
        public Numeric Rhs { get; set; }
        public ArithmeticOperator Operator { get; set; }

        public ArithmeticExpression(ArithmeticOperator op, Numeric lhs, Numeric rhs) {
            Lhs = lhs;
            Rhs = rhs;
            Operator = op;
        }
    }

    public class NumericSequence : IEnumerable<Numeric> {
        private List<Numeric> seq;


        public IEnumerator<Numeric> GetEnumerator() {
            return seq.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return seq.GetEnumerator();
        }

        public Numeric this[int index] {
            get {
                return seq[index];
            }
            set {
                seq[index] = value;
            }
        }
    }
}
