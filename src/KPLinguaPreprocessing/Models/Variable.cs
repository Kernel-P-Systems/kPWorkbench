namespace KPLinguaPreprocessing.Models
{
    public class Variable : Base
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public Variable(string name, int value = 0)
        {
            Name = name;
            Value = value;
        }

        public void SetValue(int value)
        {
            Value = value;
        }

        public override int Evaluate()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"Variable: {Name} = {Value}";
        }
    }
}
