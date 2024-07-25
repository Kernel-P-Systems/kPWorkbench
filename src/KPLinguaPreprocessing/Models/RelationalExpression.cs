using System;
using System.Collections.Generic;

namespace KPLinguaPreprocessing.Models
{
    public class RelationalExpression : Base
    {
        private static readonly Dictionary<string, Func<bool, bool, bool>> Expressions = new Dictionary<string, Func<bool, bool, bool>>
        {
            { RelationalOperators.Or, (v1, v2) => v1 || v2 },
            { RelationalOperators.And, (v1, v2) => v1 && v2 }
        };

        private readonly Base value1;
        private readonly string operation;
        private readonly Base value2;

        public RelationalExpression(Base value1, string operation, Base value2)
        {
            this.value1 = value1;
            this.operation = operation;
            this.value2 = value2;
        }

        public override double Evaluate()
        {
            return Expressions[operation](value1.Evaluate() != 0, value2.Evaluate() != 0) ? 1 : 0;
        }

        public override string ToString()
        {
            return $"{value1} {operation} {value2}";
        }
    }
}
