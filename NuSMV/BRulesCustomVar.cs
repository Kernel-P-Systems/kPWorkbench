using KpCore;
using NuSMV;

namespace NuSMV
{
    public class BRulesCustomVar
    {
        internal static void addRuleToStatusVariable(KpCore.Rule rule, NuSMV.Module module, int strategyIndex)
        {
            // ((main._status = _willDIVIDE) & (_sync = _BUSY)) : _willDIVIDE; ((main._status = _willDIVIDE) & (_sync =
            // _EXCH)) : _DIVIDED;
            if (module.HasDivisionRule)
            {
                CaseLine willDivideCaseLine = new CaseLine();
                //((main._status = _willDIVIDE) & (_sync = _BUSY)) : _willDIVIDE;
                ICondition willDivide = BRulesStandardVar.getStatusCondition(module, StatusStates.WILLDIVIDE);
                ICondition syncBusy = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.BUSY);
                willDivideCaseLine.Rule.Condition = new CompoundBoolExpression(willDivide, BinaryOperator.AND, syncBusy);
                willDivideCaseLine.Result = new Expression(StatusStates.WILLDIVIDE);
                if (!BRulesComVar.ruleExist(module.Status.Next, willDivideCaseLine))
                    module.Status.Next.addCaseLine(willDivideCaseLine);
                // ((main._status = _willDIVIDE) & (_sync = _EXCH)) : _DIVIDED;
                CaseLine dividedCaseLine = new CaseLine();
                ICondition syncExch = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.EXCHANGE);
                dividedCaseLine.Rule.Condition = new CompoundBoolExpression(willDivide, BinaryOperator.AND, syncExch);
                dividedCaseLine.Result = new Expression(StatusStates.DIVIDED);
                if (!BRulesComVar.ruleExist(module.Status.Next, dividedCaseLine))
                    module.Status.Next.addCaseLine(dividedCaseLine);
            }
            // ((main._status = _willDISSOLVE) & (_sync = _BUSY)) : _willDISSOLVE; ((main._status = _willDISSOLVE) & (_sync =
            // _EXCH)) : _DISSOLVED;
            if (module.HasDissolutionRule)
            {
                CaseLine caseLine2 = new CaseLine();
                //status = willDissolve & (_sync = _BUSY) : willDissolve;
                ICondition willSolve = BRulesStandardVar.getStatusCondition(module, StatusStates.WILLDISSOLVE);
                ICondition syncBusy = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.BUSY);
                caseLine2.Rule.Condition = new CompoundBoolExpression(willSolve, BinaryOperator.AND, syncBusy);
                caseLine2.Result = new Expression(StatusStates.WILLDISSOLVE);
                if (!BRulesComVar.ruleExist(module.Status.Next, caseLine2))
                    module.Status.Next.addCaseLine(caseLine2);
                //((c1._status = _willDISSOLVE) & (_sync = _EXCH)) : _DISSOLVED;
                CaseLine caseLine3 = new CaseLine();
                ICondition syncExch = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.EXCHANGE);
                caseLine3.Rule.Condition = new CompoundBoolExpression(willSolve, BinaryOperator.AND, syncExch);
                caseLine3.Result = new Expression(StatusStates.DISSOLVED);
                if (!BRulesComVar.ruleExist(module.Status.Next, caseLine3))
                    module.Status.Next.addCaseLine(caseLine3);
            }

            CaseLine caseLine = new CaseLine();
            caseLine.Result = getResultOfStatusRuleFromKPRule(module, rule);
            caseLine.Rule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
            caseLine.Rule.ID = rule.Id;
            //if module has division rule then, dissolve process happens inside instances.
            // ICondition statusCondition = BRulesStandardVar.getStatusCondition(module); No need status=Active condition anymore
            ICondition sequence = BRulesStandardVar.getSequenceCondition(module, strategyIndex, rule.Id);
            //CompoundBoolExpression compound = new CompoundBoolExpression(statusCondition, BinaryOperator.AND, sequence);
            caseLine.Rule.AddBoolExpression(sequence, BinaryOperator.AND);
            //if the case line has not added yet
            if (!BRulesComVar.ruleExist(module.Status.Next, caseLine))
                module.Status.Next.addCaseLine(caseLine);
        }

        private static IExp getResultOfStatusRuleFromKPRule(Module module, KpCore.Rule rule)
        {
            Expression result = new Expression();
            if (rule.Type == RuleType.MEMBRANE_DIVISION)
            {
                result.Exp = StatusStates.WILLDIVIDE;
            }
            else if (rule.Type == RuleType.MEMBRANE_DISSOLUTION)
            {
                result.Exp = StatusStates.WILLDISSOLVE;
            }
            return result;
        }
    }
}