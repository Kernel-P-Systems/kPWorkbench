namespace KPLinguaPreprocessing.Models
{
    public class RelationalExpressionTrue : Base
    {
        public override double Evaluate()
        {
            return 1;
        }

        public override string ToString()
        {
            return "True";
        }
    }
}
