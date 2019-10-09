using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public class BinaryProperty : ILtlProperty, ICtlProperty
    {
        public IPredicate LeftOperand { get; set; }

        public IPredicate RightOperand { get; set; }

        public TemporalOperator Operator { get; set; }

        public TResult Accept<TResult>(IVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
