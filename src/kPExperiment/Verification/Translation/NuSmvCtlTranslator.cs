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
    public class NuSmvCtlTranslator : ICtlPropertyTranslator
    {
        private KpMetaModel _kpMetaModel;

        public ModelCheckingTarget Target
        {
            get { return ModelCheckingTarget.Promela; }
        }

        public NuSmvCtlTranslator(KpMetaModel kpMetaModel)
        {
            _kpMetaModel = kpMetaModel;
        }

        public string Visit(Model.Verification.UnaryProperty expression)
        {
            var operand = expression.Operand.Accept(this);
            var translation = string.Empty;

            switch (expression.Operator)
            {
                case TemporalOperator.Next: translation = string.Format("EX {0}", operand); break;
                case TemporalOperator.Eventually: translation = string.Format("EF {0}", operand); break;
                case TemporalOperator.Always: translation = string.Format("AG {0}", operand); break;
                case TemporalOperator.Never: translation = string.Format("!(EF {0})", operand); break;
                case TemporalOperator.InfinitelyOften: translation = string.Format("AG (EF {0})", operand); break;
                case TemporalOperator.SteadyState: translation = string.Format("AF (AG {0})", operand); break;
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
                case TemporalOperator.Until: translation = string.Format("A [{0} U {1}]", leftOperand, rightOperand); break;
                case TemporalOperator.FollowedBy: translation = string.Format("AG ({0} -> EF {1})", leftOperand, rightOperand); break;
                // Mehmet has commented the following line, it has changed to the following line which I believe the correct form of preceded-by
                // you can revert it if you disagree
                //case TemporalOperator.PrecededBy: translation = string.Format("!(E [!({0}) U (!({0}) & {1})])", leftOperand, rightOperand); break;
                case TemporalOperator.PrecededBy: translation = string.Format("!(E [!({1}) U (!({1}) & {0})])", leftOperand, rightOperand); break;
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
                    return string.Format("{0}.{1}", membraneInstanceMeta.MInstance.Name, objectSymbol.Name);
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
                case RelationalOperator.EQ: return "=";
                case RelationalOperator.NE: return "!=";
                default: return string.Empty;
            }
        }

        private string TranslateOperator(BooleanOperator op)
        {
            switch (op)
            {
                case BooleanOperator.And: return "&";
                case BooleanOperator.Or: return "|";
                case BooleanOperator.Implication: return "->";
                case BooleanOperator.Equivalence: return "<->";
                default: return string.Empty;
            }
        }
    }
}
