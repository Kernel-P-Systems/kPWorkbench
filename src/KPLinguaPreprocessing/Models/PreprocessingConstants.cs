using System;

namespace KPLinguaPreprocessing.Models
{
    internal class ArithmeticOperations
    {
        public const string Addition = "+";
        public const string Subtraction = "-";
        public const string Multiplication = "*";

        private readonly string value;

        private ArithmeticOperations(string value)
        {
            this.value = value;
        }
    }

    public class LogicalOperators
    {
        public const string Greater = ">";
        public const string GreaterOrEqual = ">=";
        public const string Less = "<";
        public const string LessOrEqual = "<=";
        public const string Equality = "==";
        public const string Inequality = "!=";

        private readonly string value;

        public LogicalOperators(string value)
        {
            this.value = value;
        }

        public static LogicalOperators GetOperator(string value)
        {
            switch (value)
            {
                case Greater:
                    return new LogicalOperators(Greater);
                case GreaterOrEqual:
                    return new LogicalOperators(GreaterOrEqual);
                case Less:
                    return new LogicalOperators(Less);
                case LessOrEqual:
                    return new LogicalOperators(LessOrEqual);
                case Equality:
                    return new LogicalOperators(Equality);
                case Inequality:
                    return new LogicalOperators(Inequality);
            }

            return null;
        }

        public override string ToString()
        {
            return value;
        }
    }

    public static class RelationalOperators
    {
        public const string Or = "|";
        public const string And = "&";
    }

    public class Sign
    {
        private const string LessOrEqualValue = "<=";
        private const string LessValue = "<";

        public static Sign LessOrEqual = new Sign(LessOrEqualValue);
        public static Sign Less = new Sign(LessValue);


        private readonly string value;

        private Sign(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }

        public override bool Equals(object obj)
        {
            if (obj is Sign mode)
            {
                return value == mode.value;
            }
            if (obj is string str)
            {
                return value == str;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static implicit operator string(Sign mode) => mode.value;
        public static implicit operator Sign(string value)
        {
            return value switch
            {
                LessOrEqualValue => LessOrEqual,
                LessValue => Less,
                _ => throw new ArgumentException($"Invalid Sign '{value}'"),
            };
        }

    }

    public static class Token
    {
        public const string OpenVariable = "$";
        public const string CloseVariable = "$";
    }
}
