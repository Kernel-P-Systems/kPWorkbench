using System.Globalization;

namespace KPLinguaPreprocessing.Models
{
    public class Number : Base
    {
        private readonly int value;

        public Number(int value)
        {
            this.value = value;
        }

        public override int Evaluate()
        {
            return value;
        }

        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
