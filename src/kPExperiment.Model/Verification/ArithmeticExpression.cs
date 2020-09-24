using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public class ArithmeticExpression : IArithmeticOperand
    {
        public IArithmeticOperand LeftOperand { get; set; }

        public IArithmeticOperand RightOperand { get; set; }

        public ArithmeticOperator Operator { get; set; }

        public TResult Accept<TResult>(IVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
