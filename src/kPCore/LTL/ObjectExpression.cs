using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore.Ltl {
    public abstract class EntityExpression {

    }

    public class EntityCountExpression : EntityExpression {
        public string ObjectName { get; set; }
    }

    public class EntityArithmenticEpxression : EntityExpression {
        public EntityExpression Lhs { get; set; }
        public EntityExpression Rhs { get; set; }
        public ArithmeticOperator Operator { get; set; }

        public EntityArithmenticEpxression(ArithmeticOperator op, EntityExpression lhs, EntityExpression rhs) {
            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
        }
    }
}
