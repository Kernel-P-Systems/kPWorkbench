using Antlr4.Runtime.Tree;
using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KpLingua.Input
{
    public class KpExperimentModelBuilder : IKpExperimentVisitor<object>
    {
        public Experiment GetExperiment(KpExperimentParser.KPExprimentContext context)
        {
            return VisitKPExpriment(context) as Experiment;
        }

        public object VisitKPExpriment(KpExperimentParser.KPExprimentContext context)
        {
            var ltlPropertiesContext = context.ltlProperty();
            var ctlPropertiesContext = context.ctlProperty();

            var ltlProperties = Enumerable.Empty<ILtlProperty>();
            var ctlProperties = Enumerable.Empty<ICtlProperty>();

            if (ltlPropertiesContext.Length > 0)
            {
                ltlProperties = ltlPropertiesContext.Select(p => VisitLtlProperty(p) as ILtlProperty).ToList();
            }

            if (ctlPropertiesContext.Length > 0)
            {
                ctlProperties = ctlPropertiesContext.Select(p => VisitCtlProperty(p) as ICtlProperty).ToList();
            }

            return new Experiment
            {
                LtlProperties = ltlProperties,
                CtlProperties = ctlProperties,
            };
        }

        public object VisitLtlProperty(KpExperimentParser.LtlPropertyContext context)
        {
            return VisitLtlExpression(context.ltlExpression()) as ILtlProperty;
        }

        public object VisitCtlProperty(KpExperimentParser.CtlPropertyContext context)
        {
            return VisitCtlExpression(context.ctlExpression()) as ICtlProperty;
        }

        public object VisitLtlExpression(KpExperimentParser.LtlExpressionContext context)
        {
            var unaryExpressionContext = context.unaryExpression();
            var binaryExpressionContext = context.binaryExpression();
            var ltlExpression = default(ILtlProperty);

            if (unaryExpressionContext != null)
            {
                ltlExpression = VisitUnaryExpression(unaryExpressionContext) as ILtlProperty;
            }
            else if (binaryExpressionContext != null)
            {
                ltlExpression = VisitBinaryExpression(binaryExpressionContext) as ILtlProperty;
            }

            return ltlExpression;
        }

        public object VisitCtlExpression(KpExperimentParser.CtlExpressionContext context)
        {
            var unaryExpressionContext = context.unaryExpression();
            var binaryExpressionContext = context.binaryExpression();
            var ctlExpression = default(ICtlProperty);

            if (unaryExpressionContext != null)
            {
                ctlExpression = VisitUnaryExpression(unaryExpressionContext) as ICtlProperty;
            }
            else if (binaryExpressionContext != null)
            {
                ctlExpression = VisitBinaryExpression(binaryExpressionContext) as ICtlProperty;
            }

            return ctlExpression;
        }

        public object VisitUnaryExpression(KpExperimentParser.UnaryExpressionContext context)
        {
            var notExpressionContext = context.notLtlExpression();
            var eventuallyExpressionContext = context.eventuallyExpression();
            var alwaysExpressionContext = context.alwaysExpression();
            var nextExpressionContext = context.nextExpression();
            var neverExpressionContext = context.neverExpression();
            var infinitelyOftenExpressionContext = context.infinitelyOftenExpression();
            var steadyStateExpressionContext = context.steadyStateExpression();

            var ltlExpression = default(UnaryProperty);

            if (notExpressionContext != null)
            {
                ltlExpression = VisitNotLtlExpression(notExpressionContext) as UnaryProperty;
            }
            else if (eventuallyExpressionContext != null)
            {
                ltlExpression = VisitEventuallyExpression(eventuallyExpressionContext) as UnaryProperty;
            }
            else if (alwaysExpressionContext != null)
            {
                ltlExpression = VisitAlwaysExpression(alwaysExpressionContext) as UnaryProperty;
            }
            else if (nextExpressionContext != null)
            {
                ltlExpression = VisitNextExpression(nextExpressionContext) as UnaryProperty;
            }
            else if (neverExpressionContext != null)
            {
                ltlExpression = VisitNeverExpression(neverExpressionContext) as UnaryProperty;
            }
            else if (infinitelyOftenExpressionContext != null)
            {
                ltlExpression = VisitInfinitelyOftenExpression(infinitelyOftenExpressionContext) as UnaryProperty;
            }
            else if (steadyStateExpressionContext != null)
            {
                ltlExpression = VisitSteadyStateExpression(steadyStateExpressionContext) as UnaryProperty;
            }

            return ltlExpression;
        }

        public object VisitBinaryExpression(KpExperimentParser.BinaryExpressionContext context)
        {
            var untilExpressionContext = context.untilExpression();
            var weakUntilExpressionContext = context.weakUntilExpression();
            var followedByExpressionContext = context.followedByExpression();
            var precededByExpressionContext = context.precededByExpression();
            var ltlExpression = default(BinaryProperty);

            if (untilExpressionContext != null)
            {
                ltlExpression = VisitUntilExpression(untilExpressionContext) as BinaryProperty;
            }
            else if (weakUntilExpressionContext != null)
            {
                ltlExpression = VisitWeakUntilExpression(weakUntilExpressionContext) as BinaryProperty;
            }
            else if (followedByExpressionContext != null)
            {
                ltlExpression = VisitFollowedByExpression(followedByExpressionContext) as BinaryProperty;
            }
            else if (precededByExpressionContext != null)
            {
                ltlExpression = VisitPrecededByExpression(precededByExpressionContext) as BinaryProperty;
            }

            return ltlExpression;
        }

        public object VisitAlwaysExpression(KpExperimentParser.AlwaysExpressionContext context)
        {
            var equivalenceExpressionContext = context.equivalenceExpression();
            var equivalenceExpression = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;

            return new UnaryProperty
            {
                Operand = equivalenceExpression,
                Operator = TemporalOperator.Always,
            };
        }

        public object VisitEventuallyExpression(KpExperimentParser.EventuallyExpressionContext context)
        {
            var equivalenceExpressionContext = context.equivalenceExpression();
            var equivalenceExpression = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;

            return new UnaryProperty
            {
                Operand = equivalenceExpression,
                Operator = TemporalOperator.Eventually,
            };
        }

        public object VisitNextExpression(KpExperimentParser.NextExpressionContext context)
        {
            var equivalenceExpressionContext = context.equivalenceExpression();
            var equivalenceExpression = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;

            return new UnaryProperty
            {
                Operand = equivalenceExpression,
                Operator = TemporalOperator.Next,
            };
        }

        public object VisitNeverExpression(KpExperimentParser.NeverExpressionContext context)
        {
            var equivalenceExpressionContext = context.equivalenceExpression();
            var equivalenceExpression = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;

            return new UnaryProperty
            {
                Operand = equivalenceExpression,
                Operator = TemporalOperator.Never,
            };
        }

        public object VisitInfinitelyOftenExpression(KpExperimentParser.InfinitelyOftenExpressionContext context)
        {
            var equivalenceExpressionContext = context.equivalenceExpression();
            var equivalenceExpression = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;

            return new UnaryProperty
            {
                Operand = equivalenceExpression,
                Operator = TemporalOperator.InfinitelyOften,
            };
        }

        public object VisitSteadyStateExpression(KpExperimentParser.SteadyStateExpressionContext context)
        {
            var equivalenceExpressionContext = context.equivalenceExpression();
            var equivalenceExpression = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;

            return new UnaryProperty
            {
                Operand = equivalenceExpression,
                Operator = TemporalOperator.SteadyState,
            };
        }

        public object VisitNotLtlExpression(KpExperimentParser.NotLtlExpressionContext context)
        {
            var ltlExpressionContext = context.ltlExpression();
            var ltlExpression = VisitLtlExpression(ltlExpressionContext) as ILtlProperty;

            return new NotProperty
            {
                Operand = ltlExpression,
            };
        }

        public object VisitUntilExpression(KpExperimentParser.UntilExpressionContext context)
        {
            var leftPredicateContext = context.equivalenceExpression().FirstOrDefault();
            var rightPredicateCotext = context.equivalenceExpression().Skip(1).FirstOrDefault();

            return new BinaryProperty
            {
                LeftOperand = VisitEquivalenceExpression(leftPredicateContext) as IPredicate,
                RightOperand = VisitEquivalenceExpression(rightPredicateCotext) as IPredicate,
                Operator = TemporalOperator.Until,
            };
        }

        public object VisitWeakUntilExpression(KpExperimentParser.WeakUntilExpressionContext context)
        {
            var leftPredicateContext = context.equivalenceExpression().FirstOrDefault();
            var rightPredicateCotext = context.equivalenceExpression().Skip(1).FirstOrDefault();

            return new BinaryProperty
            {
                LeftOperand = VisitEquivalenceExpression(leftPredicateContext) as IPredicate,
                RightOperand = VisitEquivalenceExpression(rightPredicateCotext) as IPredicate,
                Operator = TemporalOperator.WeakUntil,
            };
        }

        public object VisitFollowedByExpression(KpExperimentParser.FollowedByExpressionContext context)
        {
            var leftPredicateContext = context.equivalenceExpression().FirstOrDefault();
            var rightPredicateCotext = context.equivalenceExpression().Skip(1).FirstOrDefault();

            return new BinaryProperty
            {
                LeftOperand = VisitEquivalenceExpression(leftPredicateContext) as IPredicate,
                RightOperand = VisitEquivalenceExpression(rightPredicateCotext) as IPredicate,
                Operator = TemporalOperator.FollowedBy,
            };
        }

        public object VisitPrecededByExpression(KpExperimentParser.PrecededByExpressionContext context)
        {
            var leftPredicateContext = context.equivalenceExpression().FirstOrDefault();
            var rightPredicateCotext = context.equivalenceExpression().Skip(1).FirstOrDefault();

            return new BinaryProperty
            {
                LeftOperand = VisitEquivalenceExpression(leftPredicateContext) as IPredicate,
                RightOperand = VisitEquivalenceExpression(rightPredicateCotext) as IPredicate,
                Operator = TemporalOperator.PrecededBy,
            };
        }

        public object VisitEquivalenceExpression(KpExperimentParser.EquivalenceExpressionContext context)
        {
            var leftPredicateContext = context.implicationExpression().FirstOrDefault();
            var rightPredicateCotext = context.implicationExpression().Skip(1).FirstOrDefault();
            var result = default(IPredicate);

            if (rightPredicateCotext != null)
            {
                result = new BooleanExpression
                {
                    LeftOperand = VisitImplicationExpression(leftPredicateContext) as IPredicate,
                    RightOperand = VisitImplicationExpression(rightPredicateCotext) as IPredicate,
                    Operator = BooleanOperator.Equivalence,
                };
            }
            else
            {
                result = VisitImplicationExpression(leftPredicateContext) as IPredicate;
            }

            return result;
        }

        public object VisitImplicationExpression(KpExperimentParser.ImplicationExpressionContext context)
        {
            var leftPredicateContext = context.orExpression().FirstOrDefault();
            var rightPredicateCotext = context.orExpression().Skip(1).FirstOrDefault();
            var result = default(IPredicate);

            if (rightPredicateCotext != null)
            {
                result = new BooleanExpression
                {
                    LeftOperand = VisitOrExpression(leftPredicateContext) as IPredicate,
                    RightOperand = VisitOrExpression(rightPredicateCotext) as IPredicate,
                    Operator = BooleanOperator.Implication,
                };
            }
            else
            {
                result = VisitOrExpression(leftPredicateContext) as IPredicate;
            }

            return result;
        }

        public object VisitOrExpression(KpExperimentParser.OrExpressionContext context)
        {
            var leftPredicateContext = context.andExpression().FirstOrDefault();
            var rightPredicateCotext = context.andExpression().Skip(1).FirstOrDefault();
            var result = default(IPredicate);

            if (rightPredicateCotext != null)
            {
                result = new BooleanExpression
                {
                    LeftOperand = VisitAndExpression(leftPredicateContext) as IPredicate,
                    RightOperand = VisitAndExpression(rightPredicateCotext) as IPredicate,
                    Operator = BooleanOperator.Or,
                };
            }
            else
            {
                result = VisitAndExpression(leftPredicateContext) as IPredicate;
            }

            return result;
        }

        public object VisitAndExpression(KpExperimentParser.AndExpressionContext context)
        {
            var leftPredicateContext = context.ltlExpressionOperand().FirstOrDefault();
            var rightPredicateCotext = context.ltlExpressionOperand().Skip(1).FirstOrDefault();
            var result = default(IPredicate);

            if (rightPredicateCotext != null)
            {
                result = new BooleanExpression
                {
                    LeftOperand = VisitLtlExpressionOperand(leftPredicateContext) as IPredicate,
                    RightOperand = VisitLtlExpressionOperand(rightPredicateCotext) as IPredicate,
                    Operator = BooleanOperator.And,
                };
            }
            else
            {
                result = VisitLtlExpressionOperand(leftPredicateContext) as IPredicate;
            }

            return result;
        }

        public object VisitLtlExpressionOperand(KpExperimentParser.LtlExpressionOperandContext context)
        {
            var ltlExpressionCotext = context.ltlExpression();
            var equivalenceExpressionContext = context.equivalenceExpression();
            var atomicPredicateContext = context.atomicExpression();
            var notExpressionContext = context.notExpression();

            var result = default(IPredicate);

            if (ltlExpressionCotext != null)
            {
                result = VisitLtlExpression(ltlExpressionCotext) as IPredicate;
            }
            else if (equivalenceExpressionContext != null)
            {
                result = VisitEquivalenceExpression(equivalenceExpressionContext) as IPredicate;
            }
            else if (atomicPredicateContext != null)
            {
                result = VisitAtomicExpression(atomicPredicateContext) as IPredicate;
            }
            else
            {
                result = VisitNotExpression(notExpressionContext) as IPredicate;
            }

            return result;
        }

        public object VisitNotExpression(KpExperimentParser.NotExpressionContext context)
        {
            var equivalenceExpressionCotext = context.equivalenceExpression();

            return new NotExpression
            {
                Operand = VisitEquivalenceExpression(equivalenceExpressionCotext) as IPredicate,
            };
        }

        public object VisitAtomicExpression(KpExperimentParser.AtomicExpressionContext context)
        {
            var leftObjectMultiplicityContext = context.objectMultiplicity().FirstOrDefault();
            var rightObjectMultiplicityContext = context.objectMultiplicity().Skip(1).FirstOrDefault();
            var rightNumericLiteralTerminal = context.NumericLiteral();
            var relationalOperatorTerminal = context.RelationalOperator();

            var result = new RelationalExpression
            {
                LeftOperand = VisitObjectMultiplicity(leftObjectMultiplicityContext) as IAtomicOperand,
                RightOperand = (rightObjectMultiplicityContext != null ? VisitObjectMultiplicity(rightObjectMultiplicityContext) : GetNumericLiteral(rightNumericLiteralTerminal)) as IAtomicOperand,
                Operator = GetRelationalOperator(relationalOperatorTerminal),
            };

            return result;
        }

        public object VisitObjectMultiplicity(KpExperimentParser.ObjectMultiplicityContext context)
        {
            var membraneIdTerminal = context.Identifier().FirstOrDefault();
            var objectIdTerminal = context.Identifier().Skip(1).FirstOrDefault();

            return new ObjectMultiplicity
            {
                MembraneId = GetIdentifier(membraneIdTerminal),
                ObjectId = GetIdentifier(objectIdTerminal),
            };
        }

        #region Not Implemented Methods

        public object Visit(Antlr4.Runtime.Tree.IParseTree tree)
        {
            throw new System.NotImplementedException();
        }

        public object VisitChildren(Antlr4.Runtime.Tree.IRuleNode node)
        {
            throw new System.NotImplementedException();
        }

        public object VisitErrorNode(Antlr4.Runtime.Tree.IErrorNode node)
        {
            throw new Exception(node.ToString());
        }

        public object VisitTerminal(Antlr4.Runtime.Tree.ITerminalNode node)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Helper Methods

        private string GetIdentifier(ITerminalNode node)
        {
            return node.Symbol.Text;
        }

        private NumericLiteral GetNumericLiteral(ITerminalNode node)
        {
            return new NumericLiteral
            {
                Value = int.Parse(node.Symbol.Text),
            };
        }

        private KpExperiment.Model.Verification.RelationalOperator GetRelationalOperator(ITerminalNode node)
        {
            switch (node.Symbol.Text)
            {
                case ">=": return KpExperiment.Model.Verification.RelationalOperator.GE;
                case "<=": return KpExperiment.Model.Verification.RelationalOperator.LE;
                case ">": return KpExperiment.Model.Verification.RelationalOperator.GT;
                case "<": return KpExperiment.Model.Verification.RelationalOperator.LT;
                case "=": return KpExperiment.Model.Verification.RelationalOperator.EQ;
                case "!=": return KpExperiment.Model.Verification.RelationalOperator.NE;
                default: return default(KpExperiment.Model.Verification.RelationalOperator);
            }
        }

        #endregion
    }
}
