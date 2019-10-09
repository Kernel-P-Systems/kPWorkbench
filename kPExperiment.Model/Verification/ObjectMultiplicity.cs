using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public class ObjectMultiplicity : IAtomicOperand
    {
        public string MembraneId { get; set; }

        public string ObjectId { get; set; }

        public TResult Accept<TResult>(IVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
