namespace KPLinguaPreprocessing.Models
{
    public class Iterator : Base
    {
        private readonly Variable variable;
        private readonly Base minValue;
        private readonly string minSign;
        private readonly Base maxValue;
        private readonly string maxSign;
        private readonly int increment;

        public Iterator(Variable variable, Base minValue, string minSign, Base maxValue, string maxSign, int increment = 1)
        {
            this.variable = variable;
            this.minValue = minValue;
            this.minSign = minSign;
            this.maxValue = maxValue;
            this.maxSign = maxSign;
            this.increment = increment;
        }

        public void Init()
        {
            if (minSign == Sign.LessOrEqual)
            {
                variable.SetValue(minValue.Evaluate());
            }
            else
            {
                variable.SetValue(minValue.Evaluate() + 1);
            }
        }

        public bool HasNext()
        {
            if (maxSign == Sign.LessOrEqual)
            {
                return variable.Evaluate() + increment <= maxValue.Evaluate();
            }
            else
            {
                return variable.Evaluate() + increment < maxValue.Evaluate();
            }
        }

        public void Next()
        {
            variable.SetValue(variable.Evaluate() + increment);
        }

        public override int Evaluate()
        {
            return variable.Evaluate();
        }

        public override string ToString()
        {
            return $"{minValue.Evaluate()} {minSign} {variable} {maxSign} {maxValue.Evaluate()}, {increment}";
        }
    }
}
