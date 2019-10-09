using KpCore;
using NuSMV;
using System;

namespace NuSMV
{
    /// <summary>
    /// Build Rules For communication variables.
    /// </summary>
    public class BRulesComVar
    {
        public static bool ruleExist(Next next, CaseLine caseLine)
        {
            bool exist = false;
            string caseline1 = caseLine.ToString();
            foreach (var line in next.CaseStatement.CaseLines)
            {
                string caseline2 = line.ToString();
                if (caseline1 == caseline2)
                {
                    exist = true;
                    break;
                }
            }
            return exist;
        }

        /// <summary> Returns condition for lower and upper bound of a variable, to be added in rules. e.g., ((step +
        /// 1)>=0)&((step + 1)<=9) </summary> <param name="sequence">variable</param> <param name="result">result of a rule
        /// e.g.(step + 1)</param> <returns>((step + 1)>=0)&((step + 1)<=9)</returns>
        internal static ICondition getBoundCondition(Variable variable, IExp result)
        {
            //ex. ((step + 1)>=0)
            BoolExp lowerCondition = new BoolExp();
            lowerCondition.Left = result;
            lowerCondition.RelationalOperator.Operator = NuSMV.RelationalOperator.GEQ;
            lowerCondition.Right = new Expression(((variable.Type) as BoundInt).LowerBound.ToString());
            //ex. ((step + 1)<=9)
            BoolExp upperCondition = new BoolExp();
            upperCondition.Left = result;
            upperCondition.RelationalOperator.Operator = NuSMV.RelationalOperator.LEQ;
            upperCondition.Right = new Expression(((variable.Type) as BoundInt).UpperBound.ToString());

            CompoundBoolExpression boundCondition = new CompoundBoolExpression();
            boundCondition.LeftCondition = lowerCondition;
            boundCondition.BinaryOperator.Operator = BinaryOperator.AND;
            boundCondition.RightCondition = upperCondition;

            return boundCondition;
        }

        private static ICondition extractConditionFromKpGuard(Module sourceModule, IGuard guard)
        {
            ICondition condition = null;
            if (guard is BasicGuard)
            {
                BasicGuard basicGuard = (BasicGuard)guard;
                condition = getBoolExpression(sourceModule, basicGuard);
            }
            else if (guard is NegatedGuard)
            {
                NegatedGuard negatedGuard = (NegatedGuard)guard;
                Console.Error.WriteLine("NegatedGuard is not applicable.");
                // ICondition negatedBooleanExpression = getNegatedGuard(negatedGuard);
            }
            else if (guard is CompoundGuard)
            {
                CompoundGuard compoundGuard = (CompoundGuard)guard;
                CompoundBoolExpression compoundBoolExpr = null;
                compoundBoolExpr.LeftCondition = extractConditionFromKpGuard(sourceModule, compoundGuard.Lhs);
                compoundBoolExpr.BinaryOperator = SMVUtil.getBinaryOperator(compoundGuard.Operator);
                compoundBoolExpr.RightCondition = extractConditionFromKpGuard(sourceModule, compoundGuard.Rhs);
                condition = compoundBoolExpr;
            }
            return condition;
        }

        /// <summary>
        /// Extracts boolean expression from a KP guard. In addition it adds module name as an identifier.
        /// </summary>
        /// <param name="sourceModule">module of guard</param>
        /// <param name="basicGuard"></param>
        /// <returns></returns>
        private static ICondition getBoolExpression(Module sourceModule, BasicGuard basicGuard)
        {
            Multiset ms = basicGuard.Multiset;
            NuSMV.RelationalOperator oper = SMVUtil.getRelationalOperator(basicGuard.Operator);
            BoolExp booleanExpression = new BoolExp();
            foreach (var obj in ms.Objects)
            {
                if (sourceModule != null)
                    booleanExpression.Left = new InstancedExp(sourceModule.Type, obj);
                else
                    booleanExpression.Left = new InstancedExp(obj);
                booleanExpression.RelationalOperator = oper;
                booleanExpression.Right = new Expression(ms[obj].ToString());
            }
            return booleanExpression;
        }

        internal static void addCaseLineToCurrentCopyCommVar(Variable orjVariable, Variable copyCommVar, KpCore.Rule rule, Module module, Module targetModule, int strategyIndex)
        {
            //for each variable generate a case line
            CaseLine caseLine = new CaseLine();
            OperExp result = new OperExp();
            int resultValue = 0;
            RewriteCommunicationRule rcr = (RewriteCommunicationRule)rule;
            foreach (var target in rcr.TargetRhs.Values)
            {
                TargetedMultiset targetMultiSet = (TargetedMultiset)target;
                InstanceIdentifier targetType = (InstanceIdentifier)targetMultiSet.Target;
                if (targetModule.Type == targetType.Value)
                {
                    Multiset ms = targetMultiSet.Multiset;
                    foreach (var obj in ms.Objects)
                    {
                        if (obj.Equals(orjVariable.Name))
                        {
                            resultValue += ms[obj];
                        }
                    }
                }
            }
            result.Exp = copyCommVar.Name;
            if (resultValue != 0)
            {
                result.Oper.Value = MathOper.ADD;
                result.Result = new Expression(resultValue.ToString());
            }

            caseLine.Result = result;
            caseLine.Rule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);

            ICondition sequenceCondition = BRulesStandardVar.getSequenceCondition(module, strategyIndex, rule.Id);
            ICondition targetBounds = getBoundCondition(copyCommVar, caseLine.Result);
            CompoundBoolExpression sequenceAndBound = new CompoundBoolExpression(sequenceCondition, BinaryOperator.AND, targetBounds);

            ICondition statusCondition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            CompoundBoolExpression statusAndSequence = new CompoundBoolExpression(statusCondition, BinaryOperator.AND, sequenceAndBound);

            // _conn = to_c2
            if (module.connectionToModuleExist(targetModule))
            {
                BoolExp connEqInstance = new BoolExp(module.getConnectionToModule(targetModule).Name, NuSMV.RelationalOperator.EQUAL, SMVPreFix.getConnectedTo(targetModule.Instance));
                caseLine.Rule.AddBoolExpression(connEqInstance, BinaryOperator.AND);
            }
            caseLine.Rule.AddBoolExpression(statusAndSequence, BinaryOperator.AND);
            caseLine.Rule.ID = rule.Id;

            if (copyCommVar.Next != null)
            {
                //if the case line has not added yet
                if (!ruleExist(copyCommVar.Next, caseLine))
                {
                    copyCommVar.Next.addCaseLine(caseLine);
                }
            }
        }

        /// <summary>
        /// this is instanced status e.g. c1.STATUS =ACTIVE
        /// </summary>
        /// <param name="module"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static BoolExp getInstancedStatus(Module module, string statusStates)
        {
            BoolExp statusCondition = null;
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                statusCondition = new BoolExp();
                statusCondition.Left = new InstancedExp(module.Instance.Name, module.Status.Name);
                statusCondition.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                statusCondition.Right = new Expression(statusStates);
            }
            return statusCondition;
        }
    }
}