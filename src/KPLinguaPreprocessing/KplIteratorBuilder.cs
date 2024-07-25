using Antlr4.Runtime;
using KPLinguaPreprocessing.Grammar;
using KPLinguaPreprocessing.Models;
using System.Collections.Generic;
using static KPLinguaPreprocessing.Grammar.KplIteratorParser;

namespace KPLinguaPreprocessing
{
    public class KplIteratorBuilder : KplIteratorBaseVisitor<object>
    {
        private Dictionary<string, Variable> variables;

        public KplIteratorBuilder(Dictionary<string, Variable> variables)
        {
            this.variables = variables;
        }

        public (List<Iterator>, Base) VisitIterators(KplIteratorParser.IteratorsContext ctx)
        {
            var iterators = new List<Iterator>();
            foreach (var child in ctx.children)
            {
                var iterator = Visit(child) as Iterator;
                if (iterator != null)
                {
                    iterators.Add(iterator);
                }
            }
            Base restrictions = new RelationalExpressionTrue();
            if (ctx.restrictions != null)
            {
                restrictions = Visit(ctx.restrictions) as Base;
            }
            return (iterators, restrictions);
        }

        public override object VisitIterator(KplIteratorParser.IteratorContext ctx)
        {
            string leftInq = ctx.leftInq.Text == Sign.LessOrEqual ? Sign.LessOrEqual : Sign.Less;
            string rightInq = ctx.rightInq.Text == Sign.LessOrEqual ? Sign.LessOrEqual : Sign.Less;
            var e1 = Visit(ctx.left) as Base;
            var e2 = Visit(ctx.right) as Base;
            var identifier = ctx.Identifier().GetText();
            int increment = 1;
            if (ctx.increment != null)
            {
                increment = int.Parse(ctx.increment.Text);
            }

            if (!variables.TryGetValue(identifier, out var variable))
            {
                variable = new Variable(identifier);
                variables[identifier] = variable;
            }

            return new Iterator(variable, e1, leftInq, e2, rightInq, increment);
        }

        public override object VisitArOpExp(KplIteratorParser.ArOpExpContext ctx)
        {
            if (ctx.op != null)
            {
                string operation = ctx.op.Text;
                var e1 = Visit(ctx.left) as Base;
                var e2 = Visit(ctx.right) as Base;
                return new ArithmeticExpression(e1, operation, e2);
            }
            else if (ctx.Number() != null)
            {
                return new Number(int.Parse(ctx.GetText()));
            }
            else if (ctx.identifierOp != null)
            {
                return Visit(ctx.identifierOp);
            }
            return null;
        }

        public override object VisitLoOpExp(KplIteratorParser.LoOpExpContext ctx)
        {
            if (ctx.op != null)
            {
                var op = ctx.op.Text;
                var e1 = Visit(ctx.left) as Base;
                var e2 = Visit(ctx.right) as Base;
                return new RelationalExpression(e1, op, e2);
            }
            else
            {
                return Visit(ctx.lo);
            }
        }

        public override object VisitLogicalExpression(KplIteratorParser.LogicalExpressionContext ctx)
        {
            var ex1 = Visit(ctx.left) as Base;
            var op = Visit(ctx.lo);
            var ex2 = Visit(ctx.right) as Base;
            return new LogicalExpression(ex1, op.ToString(), ex2);
        }

        public override object VisitLogicalOperatorName(KplIteratorParser.LogicalOperatorNameContext ctx)
        {
            return LogicalOperators.GetOperator(ctx.GetText());
        }

        public override object VisitIdentifier(KplIteratorParser.IdentifierContext ctx)
        {
            var identifier = ctx.Identifier().GetText();
            if (!variables.TryGetValue(identifier, out var variable))
            {
                variable = new Variable(identifier);
                variables[identifier] = variable;
            }
            return variable;
        }

        public override object VisitParameters(KplIteratorParser.ParametersContext ctx)
        {
            var variables = new List<Variable>();
            foreach (var child in ctx.children)
            {
                var variable = Visit(child) as Variable;
                if (variable != null)
                {
                    variables.Add(variable);
                }
            }
            return variables;
        }

        public override object VisitParameter(KplIteratorParser.ParameterContext ctx)
        {
            var identifier = ctx.Identifier().GetText();
            int number = ctx.Number() != null ? int.Parse(ctx.Number().GetText()) : 0;
            return new Variable(identifier, number);
        }

        public List<Variable> BuildParameters(string text)
        {
            var data = new AntlrInputStream(text);
            var lexer = new KplIteratorLexer(data);
            var stream = new CommonTokenStream(lexer);
            var parser = new KplIteratorParser(stream);
            var tree = parser.parameters();
            return VisitParameters(tree) as List<Variable>;
        }

        public Base BuildExpression(string text)
        {
            var data = new AntlrInputStream(text);
            var lexer = new KplIteratorLexer(data);
            var stream = new CommonTokenStream(lexer);
            var parser = new KplIteratorParser(stream);
            var tree = parser.arithmeticExpression() as ArOpExpContext;
            return VisitArOpExp(tree) as Base;
        }

        //public Base BuildLogicalExpression(string text)
        //{
        //    var data = new AntlrInputStream(text);
        //    var lexer = new KplIteratorLexer(data);
        //    var stream = new CommonTokenStream(lexer);
        //    var parser = new KplIteratorParser(stream);
        //    KplIteratorParser.RelationalExpressionContext tree = parser.relationalExpression();
        //    return VisitLoOpExp(tree) as Base;
        //}

        public (List<Iterator>, Base, Dictionary<string, Variable>) BuildIterators(string text)
        {
            var data = new AntlrInputStream(text);
            var lexer = new KplIteratorLexer(data);
            var stream = new CommonTokenStream(lexer);
            var parser = new KplIteratorParser(stream);
            var tree = parser.iterators();
            var (iterators, restrictions) = VisitIterators(tree);
            return (iterators, restrictions, variables);
        }
    }
}