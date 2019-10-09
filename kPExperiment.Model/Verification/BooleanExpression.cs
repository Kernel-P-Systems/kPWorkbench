using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public class BooleanExpression : IPredicate
    {
        public IPredicate LeftOperand { get; set; }

        public IPredicate RightOperand { get; set; }

        public BooleanOperator Operator { get; set; }

        public TResult Accept<TResult>(IVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
