using System;
using System.Collections.Generic;

namespace KPLinguaPreprocessing.Models
{
    public class LogicalExpression : Base
    {
        private static readonly Dictionary<string, Func<double, double, bool>> Expressions = new Dictionary<string, Func<double, double, bool>>
        {
            { LogicalOperators.Greater, (v1, v2) => v1 > v2 },
            { LogicalOperators.GreaterOrEqual, (v1, v2) => v1 >= v2 },
            { LogicalOperators.Less, (v1, v2) => v1 < v2 },
            { LogicalOperators.LessOrEqual, (v1, v2) => v1 <= v2 },
            { LogicalOperators.Equality, (v1, v2) => v1 == v2 },
            { LogicalOperators.Inequality, (v1, v2) => v1 != v2 }
        };

        private readonly Base value1;
        private readonly string operation;
        private readonly Base value2;

        public LogicalExpression(Base value1, string operation, Base value2)
        {
            this.value1 = value1;
            this.operation = operation;
            this.value2 = value2;
        }

        public override double Evaluate()
        {
            return Expressions[operation](value1.Evaluate(), value2.Evaluate()) ? 1 : 0;
        }

        public override string ToString()
        {
            return $"{value1} {operation} {value2}";
        }
    }
}
