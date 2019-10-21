using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore.Ltl {

    public abstract class LtlExpression {
    }

    public class BoolValueExpression : LtlExpression {
        public bool Value { get; set; }
        public BoolValueExpression(bool val) {
            Value = val;
        }
    }

    public class UnaryLtlExpression : LtlExpression {
        private UnaryLtlOperator op;
        public UnaryLtlOperator Operator {
            get { return op; }
            set {
                op = value;
            }
        }

        public LtlExpression expr;
        public LtlExpression Operand { get { return expr; } set { expr = value; } }

        public UnaryLtlExpression()
            : this(UnaryLtlOperator.ALWAYS, null) {

        }

        public UnaryLtlExpression(UnaryLtlOperator op, LtlExpression operand) {
            Operator = op;
            Operand = operand;
        }
    }

    public class BinaryLtlExpression : LtlExpression {
        private BinaryLtlOperator op;
        public BinaryLtlOperator Operator { get { return op; } set { op = value; } }

        public LtlExpression lhs;
        public LtlExpression LhsOperand { get { return lhs; } set { lhs = value; } }

        public LtlExpression rhs;
        public LtlExpression RhsOperand { get { return rhs; } set { rhs = value; } }

        public BinaryLtlExpression(BinaryLtlOperator op, LtlExpression lhs, LtlExpression rhs) {
            Operator = op;
            LhsOperand = lhs;
            RhsOperand = rhs;
        }
    }
}
