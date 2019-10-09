using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public class RelationalExpression : IPredicate
    {
        public IAtomicOperand LeftOperand { get; set; }

        public IAtomicOperand RightOperand { get; set; }

        public RelationalOperator Operator { get; set; }

        public TResult Accept<TResult>(IVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
