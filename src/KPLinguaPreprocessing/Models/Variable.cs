namespace KPLinguaPreprocessing.Models
{
    public class Variable : Base
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public Variable(string name, double value = 0)
        {
            Name = name;
            Value = value;
        }

        public void SetValue(double value)
        {
            Value = value;
        }

        public override double Evaluate()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"Variable: {Name} = {Value}";
        }
    }
}
