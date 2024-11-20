namespace KPLinguaPreprocessing.Models
{
    public class RelationalExpressionTrue : Base
    {
        public override int Evaluate()
        {
            return 1;
        }

        public override string ToString()
        {
            return "True";
        }
    }
}
