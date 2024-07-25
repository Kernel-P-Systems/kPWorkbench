using System;
using System.Collections.Generic;

namespace KPLinguaPreprocessing.Models
{
    public class ArithmeticExpression : Base
    {
        private static readonly Dictionary<string, Func<double, double, double>> Operations = new Dictionary<string, Func<double, double, double>>
        {
            { ArithmeticOperations.Addition, (v1, v2) => v1 + v2 },
            { ArithmeticOperations.Subtraction, (v1, v2) => v1 - v2 },
            { ArithmeticOperations.Multiplication, (v1, v2) => v1 * v2 }
        };

        private readonly Base value1;
        private readonly string operation;
        private readonly Base value2;

        public ArithmeticExpression(Base value1, string operation, Base value2)
        {
            this.value1 = value1;
            this.operation = operation;
            this.value2 = value2;
        }

        public override double Evaluate()
        {
            return Operations[operation](value1.Evaluate(), value2.Evaluate());
        }

        public override string ToString()
        {
            return $"{value1} {operation} {value2}";
        }
    }
}
