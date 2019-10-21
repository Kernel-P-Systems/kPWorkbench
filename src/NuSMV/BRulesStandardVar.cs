using KpCore;
using NuSMV;
using System;
using System.Linq;

namespace NuSMV
{
    internal class BRulesStandardVar
    {

        /// <summary>
        /// Translates KPRule to NUSMV rule, it ignores guards for ARB and MAX strategies, because the guards are
        /// represented by _arb0 and _max0 custom variables.
        /// </summary>
        /// <param name="kpRule"></param>
        /// <returns></returns>
        public static NuSMV.Rule extractStandardRuleFromKPRule(KpCore.Rule kpRule, Module module, int strategyIndex)
        {
            NuSMV.Rule smvRule = new NuSMV.Rule();
            smvRule.Type = SMVRuleType.REWRITING;
            Strategy ex = module.ExecutionStrategies.ElementAt(strategyIndex);
            if (kpRule is ConsumerRule)
            {
                ConsumerRule consumerRule = (ConsumerRule)kpRule;
                // If strategy is ARB or MAX then ignore guards for regular rules
                if (!(ex.Type == StrategyTypes.ARBITRARY || ex.Type == StrategyTypes.MAX))
                {
                    if (kpRule.IsGuarded)
                    {
                        ICondition guardCondition = extractGuardConditionFromKPRule(kpRule);
                        smvRule.Condition = guardCondition;
                    }
                }


                foreach (var multiset in consumerRule.Lhs)
                {
                    BoolExp ruleConditions = new BoolExp();

                    ruleConditions.Left = new Expression(multiset.Key);
                    ruleConditions.RelationalOperator.Operator = NuSMV.RelationalOperator.GEQ;
                    ruleConditions.Right = new Expression(multiset.Value.ToString());
                    if (smvRule.Condition == null)
                    {
                        smvRule.Condition = ruleConditions;
                    }
                    else
                    {
                        smvRule.AppendBoolExpression(ruleConditions, BinaryOperator.AND);
                    }
                }
            }
            return smvRule;
        }

        /// <summary>
        /// Adds guards for the rules of NonDeter and INVAR of ARB and MAX strategies. 
        /// </summary>
        /// <param name="kpRule"></param>
        /// <param name="module"></param>
        /// <param name="strategyIndex"></param>
        /// <returns></returns>
        public static NuSMV.Rule extractStandardRuleWithGuard4ARBandMAX(KpCore.Rule kpRule)
        {
            NuSMV.Rule smvRule = new NuSMV.Rule();
            smvRule.Type = SMVRuleType.REWRITING;
            if (kpRule is ConsumerRule)
            {
                ConsumerRule consumerRule = (ConsumerRule)kpRule;
                // it adds guards to _arb0 and _max0
                if (kpRule.IsGuarded)
                {
                    ICondition guardCondition = extractGuardConditionFromKPRule(kpRule);
                    smvRule.Condition = guardCondition;
                }
                foreach (var multiset in consumerRule.Lhs)
                {
                    BoolExp ruleConditions = new BoolExp();

                    ruleConditions.Left = new Expression(multiset.Key);
                    ruleConditions.RelationalOperator.Operator = NuSMV.RelationalOperator.GEQ;
                    ruleConditions.Right = new Expression(multiset.Value.ToString());
                    if (smvRule.Condition == null)
                    {
                        smvRule.Condition = ruleConditions;
                    }
                    else
                    {
                        smvRule.AppendBoolExpression(ruleConditions, BinaryOperator.AND);
                    }
                }
            }
            return smvRule;
        }

        /// <summary>
        /// Returns turn EQUALS turnState turnState
        /// </summary>
        /// <param name="turnState">turnState</param>
        /// <returns></returns>
        public static ICondition getTurnCondition(string turnState)
        {
            return getTurnCondition(turnState, NuSMV.RelationalOperator.EQUAL);
        }

        /// <summary>
        /// Return turn RELATIONALOP state, eg. turn !=Ready
        /// </summary>
        /// <param name="turnState"></param>
        /// <param name="relationalOperator">eg. NuSMV.RelationalOperator.NOT_EQUAL</param>
        /// <returns></returns>
        public static ICondition getTurnCondition(string turnState, String relationalOperator)
        {
            //turn = sequence1
            BoolExp turnExp = new BoolExp();
            turnExp.Left = new Expression(CustomVariables.TURN);
            turnExp.RelationalOperator.Operator = relationalOperator;
            turnExp.Right = new Expression(turnState);
            return turnExp;
        }

        public static ICondition getTurnCondition(Module module, int strategyIndex)
        {
            Strategy ex = module.ExecutionStrategies.ElementAt(strategyIndex);
            string turnState = ex.Name;
            //turn = sequence1
            BoolExp turnEQState = new BoolExp();
            turnEQState.Left = new Expression(CustomVariables.TURN);
            turnEQState.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            turnEQState.Right = new Expression(turnState + strategyIndex);
            return turnEQState;
        }

        /// <summary>
        /// NEXT(_turn)=turnState;
        /// </summary>
        /// <param name="turnState"></param>
        /// <returns></returns>
        public static BoolExp getNextTurnCondition(string turnState)
        {
            BoolExp turnIsSequence = new BoolExp();
            string nextTurn = SMVKeys.NEXT + "(" + CustomVariables.TURN + ")";
            turnIsSequence.Left = new Expression(nextTurn);
            turnIsSequence.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            turnIsSequence.Right = new Expression(turnState);
            return turnIsSequence;
        }

        /// <summary>
        /// if module has either dissolution or division rule, then (_status = _ACTIVE)
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static ICondition getStatusCondition(Module module)
        {
            BoolExp status = null;

            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                status = new BoolExp();
                status.Left = new Expression(module.Status.Name);
                status.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                status.Right = new Expression(StatusStates.ACTIVE);
            }
            return status;
        }

        /// <summary>
        /// Status = StatusState
        /// </summary>
        /// <param name="module"></param>
        /// <param name="statusState"></param>
        /// <returns></returns>
        public static ICondition getStatusCondition(Module module, string statusState)
        {
            BoolExp statusCondition = null;
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                statusCondition = new BoolExp();
                statusCondition = new BoolExp(module.Status.Name, NuSMV.RelationalOperator.EQUAL, statusState);
            }
            return statusCondition;
        }

        /// <summary> Combines status and turn conditions </summary> <param name="module">NuSMV module</param> <param
        /// name="turnState">TurnStates.SEQUENCE + strategyIndex</param> <returns>(_status = _ACTIVE) & (_turn =
        /// _SEQUENCE1)</returns>
        public static CompoundBoolExpression getStatusAndTurnCondition(Module module, string turnState)
        {
            ICondition statusCondition = getStatusCondition(module, StatusStates.ACTIVE);
            //turn = sequence1
            ICondition turnIsSequence = getTurnCondition(turnState);
            CompoundBoolExpression statusAndTurn = new CompoundBoolExpression(statusCondition, BinaryOperator.AND, turnIsSequence);
            return statusAndTurn;
        }

        public static ICondition getStatusAndTurnCondition(Module module, int strategyIndex)
        {
            Strategy ex = module.ExecutionStrategies.ElementAt(strategyIndex);
            string turnState = ex.Name;
            ICondition statusCondition = getStatusCondition(module);
            //turn = sequence1
            BoolExp turnEQState = new BoolExp();
            turnEQState.Left = new Expression(CustomVariables.TURN);
            turnEQState.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            turnEQState.Right = new Expression(turnState + strategyIndex);
            CompoundBoolExpression statusAndTurn = new CompoundBoolExpression(statusCondition, BinaryOperator.AND, turnEQState);
            return statusAndTurn;
        }

        /// <summary> Combines status and turn conditions </summary> <param name="module">NuSMV module</param> <param
        /// name="turnState">TurnStates.SEQUENCE + strategyIndex</param> <returns>((_status = _ACTIVE) | (_status =
        /// _willDISSOLVE))) & (next(_turn) = _SEQUENCE1)</returns>
        public static ICondition getStatusAndNextTurnCondition(Module module, string turnState)
        {
            ICondition result = null;
            //next(turn) = sequence1
            result = getNextTurnCondition(turnState);
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                ICondition statusActive = getStatusCondition(module);
                ICondition statusWillDissolveOrWillDivide = getStatusWillDissolveOrWillDivide(module);
                statusActive = new CompoundBoolExpression(statusActive, BinaryOperator.OR, statusWillDissolveOrWillDivide);
                result = new CompoundBoolExpression(statusActive, BinaryOperator.AND, result);
            }

            return result;
        }

        /// <summary>
        /// ((_status = _willDISSOLVE | (_status = _willDIVIDE))
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public static ICondition getStatusWillDissolveOrWillDivide(Module module)
        {
            ICondition statusWillDissolveOrWillDivide = null;
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                ICondition statusWillDissolve = null;
                ICondition statusWillDivide = null;
                if (module.HasDissolutionRule)
                {
                    statusWillDissolve = getStatusCondition(module, StatusStates.WILLDISSOLVE);
                }
                if (module.HasDivisionRule)
                {
                    statusWillDivide = getStatusCondition(module, StatusStates.WILLDIVIDE);
                }
                statusWillDissolveOrWillDivide = new CompoundBoolExpression(statusWillDissolve, BinaryOperator.OR, statusWillDivide);
            }
            return statusWillDissolveOrWillDivide;
        }

        /// <summary> (((_status = _ACTIVE) | ((_status = _willDISSOLVE) | (_status = _willDIVIDE))) & (_sync = _EXCH)) : 0;
        /// </summary> <param name="module">NuSMV module</param> <param name="synchState">synchState = _EXCH</param>
        /// <returns>((_status = _ACTIVE | status = willDissolve) & (_synch = _EXCH))</returns>
        public static CompoundBoolExpression getStatusAndSynchCondition(Module module, string synchState)
        {
            ICondition status = null;
            if (module.HasDissolutionRule | module.HasDivisionRule)
            {
                status = getStatusCondition(module);
            }
            BoolExp synchIsExch = new BoolExp();
            synchIsExch.Left = new Expression(CustomVariables.SYNCH);
            synchIsExch.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            synchIsExch.Right = new Expression(synchState);
            CompoundBoolExpression statusAndTurn = new CompoundBoolExpression(status, BinaryOperator.AND, synchIsExch);
            return statusAndTurn;
        }

        /// <summary>
        /// Extract condition from KP Rule, add condition to the next of variable. Rule behaviour are writing rules.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="kpRule"></param>
        /// <param name="module"></param>
        /// <param name="strategyIndex"></param>
        internal static void addCaseLineToStandardVariable(Variable variable, KpCore.Rule kpRule, Module module, int strategyIndex)
        {
            //for each variable generate a case line
            CaseLine caseLine = new CaseLine();
            caseLine.Result = getResultOfRuleFromKPRule(module, kpRule, variable);
            caseLine.Rule = extractStandardRuleFromKPRule(kpRule, module, strategyIndex);
            caseLine.Rule.ID = kpRule.Id;
            //if result is division or dissolution it will be null
            if (caseLine.Result != null)
            {
                caseLine.Rule.AddBoolExpression(getBoundCondition(variable, caseLine.Result), BinaryOperator.AND);
            }
            else
            {
                throw new Exception("BRulesStandarVar, line 176");
                // caseLine.Result = new Expression(variable.Name);
            }
            ICondition statusCondition = getTurnCondition(module, strategyIndex);
            ICondition sequenceCondition = getSequenceCondition(module, strategyIndex, kpRule.Id);
            CompoundBoolExpression statusAndTurn = new CompoundBoolExpression(statusCondition, BinaryOperator.AND, sequenceCondition);
            caseLine.Rule.AddBoolExpression(statusAndTurn, BinaryOperator.AND);

            if (variable.Next != null)
            {
                if (!BRulesComVar.ruleExist(variable.Next, caseLine))
                    variable.Next.addCaseLine(caseLine);
            }
        }

        /// <summary> Returns condition for lower and upper bound of a variable, to be added in rules. eg. ((step +
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

        /// <summary>
        /// return the last caseline which is TRUE : result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static CaseLine trueCaseLine(string result)
        {
            CaseLine caseLine = new CaseLine();
            NuSMV.Rule rule = new NuSMV.Rule();
            rule.Condition = new TruthValue(Truth.TRUE);
            Expression exp = new Expression(result);
            caseLine.Rule = rule;
            caseLine.Result = exp;
            return caseLine;
        }

        /*******************************************************************************************/

        #region extract guard condition from KP rule for standard variables

        public static ICondition extractGuardConditionFromKPRule(KpCore.Rule rule)
        {
            IGuard guard = rule.Guard;
            ICondition condition = extractGuardConditionFromKpGuard(guard);
            return condition;
        }

        private static ICondition extractGuardConditionFromKpGuard(IGuard guard)
        {
            ICondition condition = null;
            if (guard is BasicGuard)
            {
                BasicGuard basicGuard = (BasicGuard)guard;
                condition = getBoolExpression(basicGuard);
            }
            else if (guard is NegatedGuard)
            {
                NegatedGuard negatedGuard = (NegatedGuard)guard;
                Console.Error.WriteLine("KP NegatedGuard to SMV translation has not been implemented yet!");
            }
            else if (guard is CompoundGuard)
            {
                CompoundGuard compoundGuard = (CompoundGuard)guard;
                CompoundBoolExpression compoundBoolExpr = new CompoundBoolExpression();
                compoundBoolExpr.LeftCondition = extractGuardConditionFromKpGuard(compoundGuard.Lhs);
                compoundBoolExpr.BinaryOperator = SMVUtil.getBinaryOperator(compoundGuard.Operator);
                compoundBoolExpr.RightCondition = extractGuardConditionFromKpGuard(compoundGuard.Rhs);
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
        public static ICondition getBoolExpression(BasicGuard basicGuard)
        {
            Multiset ms = basicGuard.Multiset;
            NuSMV.RelationalOperator oper = SMVUtil.getRelationalOperator(basicGuard.Operator);
            BoolExp booleanExpression = new BoolExp();
            foreach (var obj in ms.Objects)
            {
                booleanExpression.Left = new Expression(obj);
                booleanExpression.RelationalOperator = oper;
                booleanExpression.Right = new Expression(ms[obj].ToString());
            }
            return booleanExpression;
        }

        //TODO: Important
        //private static ICondition getNegatedGuard(NegatedGuard negatedGuard)
        //{
        //   //Implement it.
        //}

        #endregion extract guard condition from KP rule for standard variables

        /********************************************************************************/

        #region extract result of a condition from KP Rule

        internal static IExp getResultOfRuleFromKPRule(Module module, KpCore.Rule kpRule, Variable variable)
        {
            IExp result = null;
            if (kpRule.Type == RuleType.MULTISET_REWRITING || kpRule.Type == RuleType.REWRITE_COMMUNICATION)
            {
                result = resultOfReWritingRule(kpRule, variable);
            }
            else if (kpRule.Type == RuleType.MEMBRANE_DIVISION || kpRule.Type == RuleType.MEMBRANE_DISSOLUTION)
            {
                result = resultOfConsumerRule(kpRule, variable);
            }
            return result;
        }

        private static IExp resultOfConsumerRule(KpCore.Rule kpRule, Variable variable)
        {
            OperExp result = new OperExp();
            int value = 0;//value of the variable
            ConsumerRule cr = null;
            if (kpRule.Type == RuleType.MEMBRANE_DISSOLUTION)
            {
                cr = (DissolutionRule)kpRule;
            }
            else if (kpRule.Type == RuleType.MEMBRANE_DIVISION)
            {
                cr = (DivisionRule)kpRule;
            }

            result.Oper.Value = MathOper.SUB;
            foreach (var leftHRule in cr.Lhs)
            {
                if (leftHRule.Key.Equals(variable.Name))
                {
                    value += leftHRule.Value;
                }
            }
            result.Exp = variable.Name;
            result.Result = new Expression(value.ToString());

            return result;
        }

        /// <summary>
        /// Adds sequence of execution strategy, such as, choice
        /// </summary>
        /// <param name="module"></param>
        /// <param name="strategyIndex"></param>
        /// <returns></returns>
        internal static ICondition getSequenceCondition(Module module, int strategyIndex, int ruleID)
        {
            BoolExp boolExpression = new BoolExp();

            Strategy ex = module.ExecutionStrategies.ElementAt(strategyIndex);
            if (ex.Type == StrategyTypes.CHOICE)
            {
                IVar sequence = ex.CustomVars.Find(variable => variable.Name == (CustomVariables.CHOICE + strategyIndex));
                boolExpression.Left = new Expression(sequence.Name);
                boolExpression.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                boolExpression.Right = new Expression(ruleID.ToString());
            }
            else if (ex.Type == StrategyTypes.SEQUENCE)
            {
                IVar sequence = ex.CustomVars.Find(variable => variable.Name == (CustomVariables.SEQUENCE + strategyIndex));
                boolExpression.Left = new Expression(sequence.Name);
                boolExpression.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                boolExpression.Right = new Expression(ruleID.ToString());
            }
            else if (ex.Type == StrategyTypes.ARBITRARY)
            {
                IVar sequence = ex.CustomVars.Find(variable => variable.Name == (CustomVariables.ARBITRARY + strategyIndex));
                boolExpression.Left = new Expression(sequence.Name);
                boolExpression.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                boolExpression.Right = new Expression(ruleID.ToString());
            }
            else if (ex.Type == StrategyTypes.MAX)
            {
                IVar sequence = ex.CustomVars.Find(variable => variable.Name == (CustomVariables.MAX + strategyIndex));
                boolExpression.Left = new Expression(sequence.Name);
                boolExpression.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                boolExpression.Right = new Expression(ruleID.ToString());
            }
            return boolExpression;
        }

        private static IExp resultOfReWritingRule(KpCore.Rule kpRule, Variable variable)
        {
            OperExp result = new OperExp();
            int value = 0;//value of the variable
            RewritingRule rwr = null;
            if (kpRule.Type == RuleType.MULTISET_REWRITING)
            {
                rwr = (RewritingRule)kpRule;
            }
            else if (kpRule.Type == RuleType.REWRITE_COMMUNICATION)
            {
                rwr = (RewriteCommunicationRule)kpRule;
            }
            string orijinalVarName = variable.Name.Replace(SMVPreFix.COPY, "");
            if (variable.Origin == VariableOrigin.Original || variable.Origin == VariableOrigin.OriginalCommVar)
            {
                result.Oper.Value = MathOper.SUB;
                foreach (var leftHRule in rwr.Lhs)
                {
                    if (leftHRule.Key.Equals(orijinalVarName))
                    {
                        value += leftHRule.Value;
                    }
                }
            }
            else if (variable.Origin == VariableOrigin.Copy || variable.Origin == VariableOrigin.CopyOfCommVar)
            {
                result.Oper.Value = MathOper.ADD;
                foreach (var rigthHRule in rwr.Rhs)
                {
                    if (rigthHRule.Key.Equals(orijinalVarName))
                    {
                        value += rigthHRule.Value;
                    }
                }
            }
            result.Exp = variable.Name;
            result.Result = new Expression(value.ToString());

            return result;
        }

        #endregion extract result of a condition from KP Rule

        /*******************************************************************************************/

        #region assignLastCasesToNext

        /// <summary> Assign, ((_status = ACTIVE) & (_turn = _system) & (((a + a_cp) >= 0) & ((a + a_cp) = 5))) : a + a_cp;
        /// (_status = ACTIVE) & (_turn = _system)) : 0; TRUE : a_cp; </summary> <param name="module"></param>
        internal static void assignLastCasesToNext(Module module)
        {
            assignStatusAndSynchToVariables(module);
            //if dissolved, then variable is zero
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                assignDissolvesAndDivides(module);
            }
            assingTRUECaseToStandardVariables(module);
        }

        /// <summary> ((((_status = _ACTIVE) | (_status = _willDISSOLVE)) & (_sync = _EXCH)) & ((a + a_cp >= 0) & (a + a_cp
        /// <= 3))) : a + a_cp; </summary> <param name="module"></param>
        private static void assignStatusAndSynchToVariables(Module module)
        {
            foreach (var variable in module.Variables)
            {
                assignStatusAndSyncToAVariable(module, variable);
            }
        }

        private static void assignStatusAndSyncToAVariable(Module module, IVar variable)
        {
            if (variable is Variable)
            {
                CaseLine caseLine = new CaseLine();
                Variable var = (Variable)variable;
                String exchangeState = SynchStates.EXCHANGE;
                //For Copy variables inside module
                if (var.Origin == VariableOrigin.Copy || var.Origin == VariableOrigin.CopyOfCommVar)
                {
                    Expression result = new Expression("0");
                    caseLine.Rule.Condition = getStatusAndSynchCondition(module, exchangeState);
                    caseLine.Result = result;
                    var.Next.CaseStatement.CaseLines.Add(caseLine);
                }
                //For Original variables inside module
                else if (var.Origin == VariableOrigin.Original && var.Behaviour != VariableBehaviour.COMMUNICATION)
                {
                    //if it has copy var, then add  var + var_cp
                    if (module.isVariableExist(SMVPreFix.getCopyName(var)))
                    {
                        OperExp result = new OperExp();
                        result.Exp = variable.Name;
                        result.Oper.Value = MathOper.ADD;
                        //if variable is a division variable, then it has go to main part, like comm vars, so it will be instanced.
                        if (var.Behaviour == VariableBehaviour.DIVISION)
                        {
                            result.Result = new InstancedExp(variable.Name + SMVPreFix.COPY);
                        }
                        else
                        {
                            result.Result = new Expression(variable.Name + SMVPreFix.COPY);
                        }
                        ICondition boundCondition = getBoundCondition(var, result);
                        caseLine.Rule.Condition = new CompoundBoolExpression(getStatusAndSynchCondition(module, exchangeState), BinaryOperator.AND, boundCondition);
                        caseLine.Result = result;
                        var.Next.CaseStatement.CaseLines.Add(caseLine);
                    }
                }
                //for communication variables, goes into main module.
                else if (var.Origin == VariableOrigin.OriginalCommVar && var.Behaviour == VariableBehaviour.COMMUNICATION)
                {
                    CaseLine commCaseLine = assignStatusAndSyncToACommVariable(module, var);
                    var.Next.CaseStatement.CaseLines.Add(commCaseLine);
                }
            }
        }

        private static CaseLine assignStatusAndSyncToACommVariable(Module module, Variable variable)
        {
            CaseLine commCaseLine = new CaseLine();
            OperExp result = new OperExp();
            result.Exp = new InstancedExp(module.Instance.Name, variable.Name).ToString();
            result.Oper.Value = MathOper.ADD;
            //if it has copy var, then add  var + var_cp
            string res = "";
            if (module.isVariableExist(SMVPreFix.getCopyName(variable)))
            {
                res += new InstancedExp(module.Instance.Name, SMVPreFix.getCopyName(variable));
            }
            foreach (var connectedInstance in module.Instance.ConnectedTo)
            {
                if (connectedInstance.Module.isVariableExist(SMVPreFix.getConnectedCopyCommVarName(variable, module)))
                {
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        res += " + ";
                    }

                    res += new InstancedExp(connectedInstance.Name, SMVPreFix.getConnectedCopyCommVarName(variable, module));
                }
            }
            result.Result = new Expression(res);

            //(c1.status = _ACTIVE)
            CompoundBoolExpression statusAndSync = getInstancedStatusAndSyncCondition(module);

            ICondition boundCondition = getBoundCondition(variable, result);

            ICondition compound = new CompoundBoolExpression(statusAndSync, BinaryOperator.AND, boundCondition);

            CommunicationRule rule = new CommunicationRule();
            rule.Condition = compound;
            //commCaseLine.Rule.Condition = compound;
            commCaseLine.Rule = rule;
            commCaseLine.Result = result;
            return commCaseLine;
        }

        /// <summary>
        /// if dissolved, then variable is zero status = dissolved : 0;
        /// </summary>
        /// <param name="module"></param>
        private static void assignDissolvesAndDivides(Module module)
        {
            //add standard variables
            foreach (var iVar in module.Variables)
            {
                if (iVar is Variable)
                {
                    Variable variable = (Variable)iVar;
                    Expression result = new Expression("0");

                    CaseLine caseLine1 = new CaseLine();
                    //(((_status = _willDISSOLVE) | (_status = _willDIVIDE)) & (_sync = _EXCH)) : 0;
                    caseLine1.Rule.Condition = getWillDissolveOrWillDivideAndSync(module);
                    caseLine1.Result = result;
                    variable.Next.CaseStatement.CaseLines.Add(caseLine1);

                    CaseLine caseLine2 = new CaseLine();
                    //((_status = _DISSOLVED) | (_status = _DIVIDED)) : 0;
                    caseLine2.Rule.Condition = getDissolvedOrDivided(module);
                    caseLine2.Result = result;
                    variable.Next.CaseStatement.CaseLines.Add(caseLine2);
                }
            }
        }

        /// <summary> (((_status = _willDISSOLVE) | (_status = _willDIVIDE)) & (_sync = _EXCH)) </summary> <param
        /// name="module"></param> <returns></returns>
        private static ICondition getWillDissolveOrWillDivideAndSync(Module module)
        {
            ICondition statusWillDissolveOrWillDivide = getStatusWillDissolveOrWillDivide(module);
            BoolExp synchIsExch = new BoolExp();
            synchIsExch.Left = new Expression(CustomVariables.SYNCH);
            synchIsExch.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            synchIsExch.Right = new Expression(SynchStates.EXCHANGE);
            CompoundBoolExpression willDisDivAndSync = new CompoundBoolExpression(statusWillDissolveOrWillDivide, BinaryOperator.AND, synchIsExch);
            return willDisDivAndSync;
        }

        /// <summary>
        /// ((_status = _DISSOLVED) | (_status = _DIVIDED))
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private static ICondition getDissolvedOrDivided(Module module)
        {
            CaseLine caseLine = new CaseLine();
            ICondition dissolved = null;
            ICondition divided = null;

            ICondition dissolvedOrDivided = null;
            if (module.HasDissolutionRule)
            {
                dissolved = getStatusCondition(module, StatusStates.DISSOLVED);
            }
            if (module.HasDivisionRule)
            {
                divided = getStatusCondition(module, StatusStates.DIVIDED);
            }
            dissolvedOrDivided = new CompoundBoolExpression(dissolved, BinaryOperator.OR, divided);
            return dissolvedOrDivided;
        }

        private static CompoundBoolExpression getInstancedStatusAndSyncCondition(Module module)
        {
            ICondition status = null;
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                status = BRulesComVar.getInstancedStatus(module, StatusStates.ACTIVE);
            }
            // this is non-instanced _sync = _EXCH
            BoolExp synchIsExch = new BoolExp();
            synchIsExch.Left = new Expression(CustomVariables.SYNCH);
            synchIsExch.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            synchIsExch.Right = new Expression(SynchStates.EXCHANGE);

            //(_sync = _EXCH)
            CompoundBoolExpression statusAndSync = new CompoundBoolExpression(status, BinaryOperator.AND, synchIsExch);
            return statusAndSync;
        }

        /// <summary> ((_status = _NONEXIST) & (next(_status) = _ACTIVE))) </summary> <param name="module"></param>
        /// <returns></returns>
        internal static CompoundBoolExpression getStatusNonExistNEXTActive(Module module)
        {
            CompoundBoolExpression statusNonExistNEXTActive = null;
            if (module.HasDivisionRule)
            {
                ICondition statusNONEXIST = BRulesStandardVar.getStatusCondition(module, StatusStates.NONEXIST);

                //next(child.status)=ACTIVE
                Expression left = new Expression();
                left.Exp = SMVKeys.NEXT + "(" + module.Status.Name + ")";
                Expression right = new Expression(StatusStates.ACTIVE);
                BoolExp nextStatusACTIVE = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);
                statusNonExistNEXTActive = new CompoundBoolExpression(statusNONEXIST, NuSMV.BinaryOperator.AND, nextStatusACTIVE);
            }
            return statusNonExistNEXTActive;
        }

        private static void assingTRUECaseToStandardVariables(Module module)
        {
            //add status variable
            if (module.Status != null)
            {
                Variable status = module.Status;
                status.Next.CaseStatement.CaseLines.Add(trueCaseLine(status));
            }
            //add standard variables
            foreach (var variable in module.Variables)
            {
                if (variable is Variable)
                {
                    Variable newVar = (Variable)variable;
                    CaseLine caseLine = trueCaseLine(variable);
                    newVar.Next.CaseStatement.CaseLines.Add(caseLine);
                }
            }
        }

        private static CaseLine trueCaseLine(IVar variable)
        {
            CaseLine caseLine = new CaseLine();
            NuSMV.Rule rule = new NuSMV.Rule();
            rule.Condition = new TruthValue(Truth.TRUE);
            IExp exp = null;
            if (variable.Behaviour == VariableBehaviour.COMMUNICATION || variable.Behaviour == VariableBehaviour.DIVISION)
            {
                exp = new InstancedExp(variable.Name);
            }
            else
            {
                exp = new Expression(variable.Name);
            }
            caseLine.Rule = rule;
            caseLine.Result = exp;
            return caseLine;
        }

        internal static void assignLastCasesToDivisionVars(Module module)
        {
            if (module.HasDivisionRule)
            {
                if (module.Instance is ChildInstance)
                {
                    ChildInstance childInstance = (ChildInstance)module.Instance;
                    // childInstance.DivisionStatus.Next.CaseStatement.CaseLines.Add(trueCaseLine(childInstance.DivisionStatus));
                    foreach (var variable in module.Variables)
                    {
                        if (variable.Behaviour == VariableBehaviour.DIVISION)
                        {
                            //Copy all rules of division variable which are added after division value calculation in BDivision name space.
                            Variable divisionVar = (Variable)childInstance.DivisionVariables.First(item => item.Name.Equals(variable.Name));
                            if (variable is Variable)
                            {
                                foreach (var caseLine in (variable as Variable).Next.CaseStatement.CaseLines)
                                {
                                    if (!BRulesComVar.ruleExist(divisionVar.Next, caseLine as CaseLine))
                                    {
                                        divisionVar.Next.addCaseLine(caseLine);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion assignLastCasesToNext
    }
}