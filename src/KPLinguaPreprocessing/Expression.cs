using KPLinguaPreprocessing.Models;

namespace KPLinguaPreprocessing
{
    internal class Expression : IParsingComponent
    {
        private readonly string _expressionText;
        private readonly KplIteratorBuilder _builder;

        public Expression(string expressionText, KplIteratorBuilder builder)
        {
            _expressionText = expressionText;
            _builder = builder;
        }

        public string Process(string? rule)
        {
            Base expression = _builder.BuildExpression(_expressionText);
            return expression.Evaluate().ToString();
        }
    }
}
