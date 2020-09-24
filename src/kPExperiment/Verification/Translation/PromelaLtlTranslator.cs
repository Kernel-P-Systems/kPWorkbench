using KpExperiment.Model.Verification;
using KpExperiment.Verification.Translation.Base;
using KpUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Verification.Translation
{
    public class PromelaLtlTranslator : ILtlPropertyTranslator
    {
        private KpMetaModel _kpMetaModel;

        public ModelCheckingTarget Target
        {
            get { return ModelCheckingTarget.Promela; }
        }

        public PromelaLtlTranslator(KpMetaModel kpMetaModel)
        {
            _kpMetaModel = kpMetaModel;
        }

        public string Visit(Model.Verification.UnaryProperty expression)
        {
            var operand = expression.Operand.Accept(this);
            var translation = string.Empty;

            switch (expression.Operator)
            {
                case TemporalOperator.Next: translation = string.Format("X (state != step_complete U ({0} && state == step_complete))", operand); break;
                case TemporalOperator.Eventually: translation = string.Format("<> ({0} && state == step_complete)", operand); break;
                case TemporalOperator.Always: translation = string.Format("[] ({0} || state != step_complete)", operand); break;
                case TemporalOperator.Never: translation = string.Format("!(<> ({0} && state == step_complete))", operand); break;
                case TemporalOperator.InfinitelyOften: translation = string.Format("[] (<> ({0} && state == step_complete) || state != step_complete)", operand); break;
                case TemporalOperator.SteadyState: translation = string.Format("<> ([] ({0} || state != step_complete) && state == step_complete)", operand); break;
            }

            return translation;
        }

        public string Visit(Model.Verification.BinaryProperty expression)
        {
            var leftOperand = expression.LeftOperand.Accept(this);
            var rightOperand = expression.RightOperand.Accept(this);
            var translation = string.Empty;

            switch (expression.Operator)
            {
                case TemporalOperator.Until: translation = string.Format("({0} && state != step_complete) U ({1} && state == step_complete)", leftOperand, rightOperand); break;
                case TemporalOperator.FollowedBy: translation = string.Format("[] (({0} -> <> ({1} && state == step_complete)) || state != step_complete)", leftOperand, rightOperand); break;
                case TemporalOperator.PrecededBy: translation = string.Format("!((!({0}) || state != step_complete) U (!({0}) && {1} && state == step_complete))", leftOperand, rightOperand); break;
            }

            return translation;
        }

        public string Visit(Model.Verification.NotProperty expression)
        {
            return string.Format("!({0})", expression.Operand.Accept(this));
        }

        public string Visit(Model.Verification.RelationalExpression expression)
        {
            return string.Format("{0} {1} {2}", expression.LeftOperand.Accept(this), TranslateOperator(expression.Operator), expression.RightOperand.Accept(this));
        }

        public string Visit(ArithmeticExpression expression)
        {
            return string.Format("({0} {1} {2})", expression.LeftOperand.Accept(this), TranslateOperator(expression.Operator), expression.RightOperand.Accept(this));
        }

        public string Visit(Model.Verification.BooleanExpression expression)
        {
            return string.Format("({0} {1} {2})", expression.LeftOperand.Accept(this), TranslateOperator(expression.Operator), expression.RightOperand.Accept(this));
        }

        public string Visit(Model.Verification.NotExpression expression)
        {
            return string.Format("!({0})", expression.Operand.Accept(this));
        }

        public string Visit(Model.Verification.ObjectMultiplicity expression)
        {
            var membraneInstanceMeta = _kpMetaModel.InstanceRegistry.Values.FirstOrDefault(im => im.MInstance.Name == expression.MembraneId);

            if (membraneInstanceMeta != null)
            {
                var objectSymbol = membraneInstanceMeta.MTypeMeta.Symbol(expression.ObjectId);

                if (objectSymbol != null)
                {
                    //old translator version
                    //return string.Format("m{0}[{1}].x[{2}]", membraneInstanceMeta.MTypeMeta.Id, membraneInstanceMeta.Id, objectSymbol.Id);

                    //new translator version
                    return string.Format("c[{0}].x[{1}_]", membraneInstanceMeta.MTypeMeta.Id, objectSymbol.Name);
                }
                else
                {
                    throw new Exception(string.Format("Object '{0}' does not belong to the compartment '{1}'", expression.ObjectId, expression.MembraneId));
                }
            }
            else
            {
                throw new Exception(string.Format("No compartment named '{0}' is defined into the current KPLingua model", expression.MembraneId));
            }

        }

        public string Visit(Model.Verification.NumericLiteral expression)
        {
            return expression.Value.ToString();
        }

        private string TranslateOperator(RelationalOperator op)
        {
            switch (op)
            {
                case RelationalOperator.GT: return ">";
                case RelationalOperator.GE: return ">=";
                case RelationalOperator.LT: return "<";
                case RelationalOperator.LE: return "<=";
                case RelationalOperator.EQ: return "==";
                case RelationalOperator.NE: return "!=";
                default: return string.Empty;
            }
        }

        private string TranslateOperator(BooleanOperator op)
        {
            switch (op)
            {
                case BooleanOperator.And: return "&&";
                case BooleanOperator.Or: return "||";
                case BooleanOperator.Implication: return "->";
                case BooleanOperator.Equivalence: return "<->";
                default: return string.Empty;
            }
        }

        private string TranslateOperator(ArithmeticOperator op)
        {
            switch (op)
            {
                case ArithmeticOperator.ADDITION: return "+";
                case ArithmeticOperator.MULTIPLICATION: return "*";
                default: return string.Empty;
            }
        }
    }
}
