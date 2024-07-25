using System.Globalization;

namespace KPLinguaPreprocessing.Models
{
    public class Number : Base
    {
        private readonly double value;

        public Number(double value)
        {
            this.value = value;
        }

        public override double Evaluate()
        {
            return value;
        }

        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
