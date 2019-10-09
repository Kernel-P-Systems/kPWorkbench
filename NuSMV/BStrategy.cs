using KpCore;
using NuSMV;
using System.Collections.Generic;
using System.Linq;

namespace NuSMV
{
    internal class BStrategy
    {
        public static void buildExecutionStrategies(Module module, MType kpType)
        {
            ExecutionStrategy kpES = kpType.ExecutionStrategy;
            int strategyIndex = 0;
            while (kpES != null)
            {
                if (kpES.Operator == StrategyOperator.CHOICE)
                {
                    BStrategy.buildChoiceStrategy(module, kpES, strategyIndex);
                }
                else if (kpES.Operator == StrategyOperator.ARBITRARY)
                {
                    BStrategy.buildArbitraryStrategy(module, kpES, strategyIndex);
                }
                else if (kpES.Operator == StrategyOperator.SEQUENCE)
                {
                    BStrategy.buildSequenceStrategy(module, kpES, strategyIndex);
                }
                else if (kpES.Operator == StrategyOperator.MAX)
                {
                    BStrategy.buildMaxStrategy(module, kpES, strategyIndex);
                }
                kpES = kpES.Next;
                strategyIndex++;
            }
            //Add caselines to turn variable
            addCaseLines2Turn(module, kpType);
            //(_status = _ACTIVE) & ((next (_turn) = _ARB1) | (next (_turn) = _SEQ2)) : 1;
            if (module.HasArbitraryStrategy || module.HasSequenceStrategy || module.HasMaxStrategy)
                addLastCaseLine2Count(module, kpType.ExecutionStrategy);
        }

        public static void buildArbitraryStrategy(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            ArbitraryStrategy arbStrategy = new ArbitraryStrategy();
            arbStrategy.TotalRule = kpES.Rules.Count;

            module.ExecutionStrategies.Add(arbStrategy);

            buildARBCustomVariables(module, kpES, strategyIndex, arbStrategy);

            //_count : 0 .. 10;
            Variable count = module.Count;
            //update upper bound if rule number is greater then default value.
            (count.Type as BoundInt).UpperBound = kpES.Rules.Count;
            count.Next.addCaseLine(addARBCaseLine2Count(module, kpES, strategyIndex));

            //add arbitrary condition to turn variable
            addARB2Turn(module, kpES, strategyIndex);
        }

        public static void buildChoiceStrategy(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            ChoiceStrategy choiceStrategy = new ChoiceStrategy();
            // firstly, generate choice variable itself
            string choiceVarName = CustomVariables.CHOICE + strategyIndex;
            NonDeterVar choice = new NonDeterVar(choiceVarName);
            choice.Behaviour = VariableBehaviour.CUSTOM;
            choice.Type = new NuSMV.BoundInt(0, kpES.Rules.Count);
            //add to ex strategy list
            module.ExecutionStrategies.Add(choiceStrategy);

            choice.INVARCase = getChoiceINVAR(module, choiceVarName, kpES, strategyIndex);
            choice.NonDetCase = getChoiceNonDetCase(module, choiceVarName, kpES, strategyIndex);
            choiceStrategy.CustomVars.Add(choice);
            choiceStrategy.TotalRule = kpES.Rules.Count;

            // add choice strategy in turn
            addChoice2Turn(module, kpES, strategyIndex);
        }

        public static void buildMaxStrategy(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            MAXStrategy maxStrategy = new MAXStrategy();
            maxStrategy.TotalRule = kpES.Rules.Count;

            string maxVarName = CustomVariables.MAX + strategyIndex;
            NonDeterVar max = new NonDeterVar(maxVarName);
            max.Behaviour = VariableBehaviour.CUSTOM;
            max.Type = new NuSMV.BoundInt(0, kpES.Rules.Count);

            module.ExecutionStrategies.Add(maxStrategy);

            max.INVARCase = getMAXINVAR(module, maxVarName, kpES, strategyIndex);
            max.NonDetCase = getMAXNonDetCase(module, kpES, strategyIndex);
            maxStrategy.CustomVars.Add(max);

            //add guards for each rule of arbitrary.
            buildMAXGuardVariables(module, kpES, strategyIndex, maxStrategy.CustomVars);

            //_count : 0 .. 10;
            Variable count = module.Count;
            //update upper bound if rule number is greater then default value.
            (count.Type as BoundInt).UpperBound = kpES.Rules.Count;
            count.Next.addCaseLine(addMAXCaseLine2Count(module, kpES, strategyIndex));

            addMAX2Turn(module, kpES, strategyIndex);
        }

        public static void buildSequenceStrategy(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            SequenceStrategy sequenceStrategy = new SequenceStrategy();

            string sequenceVarName = CustomVariables.SEQUENCE + strategyIndex;
            NonDeterVar sequence = new NonDeterVar(sequenceVarName);
            sequence.Behaviour = VariableBehaviour.CUSTOM;
            sequence.Type = new NuSMV.BoundInt(0, kpES.Rules.Count);

            module.ExecutionStrategies.Add(sequenceStrategy);

            sequence.NonDetCase = getSEQCase(module, kpES, strategyIndex, sequence);

            sequenceStrategy.CustomVars.Add(sequence);
            sequenceStrategy.TotalRule = kpES.Rules.Count;

            //_count : 0 .. 10;
            Variable count = module.Count;
            //update upper bound if rule number is greater then default value.
            (count.Type as BoundInt).UpperBound = kpES.Rules.Count;
            count.Next.addCaseLine(addSEQCaseLine2Count(module, kpES, strategyIndex));

            addSEQ2Turn(module, kpES, strategyIndex, sequence);
        }

        public static Case getChoiceINVAR(Module module, string choiceVarName, ExecutionStrategy kpES, int strategyIndex)
        {
            Case smvCase = new Case();
            CaseLine caseLine = new CaseLine();
            NuSMV.Rule smvRule = null;
            foreach (var rule in kpES.Rules)
            {
                NuSMV.Rule tempRule = null;
                tempRule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
                if (tempRule != null)
                {
                    if (smvRule == null)
                    {
                        smvRule = tempRule;
                    }
                    else
                        smvRule.AppendBoolExpression(tempRule.Condition, BinaryOperator.OR);
                }
            }
            if (smvRule != null)
            {
                smvRule.AddBoolExpression(BRulesStandardVar.getTurnCondition(module, strategyIndex), BinaryOperator.AND);
                caseLine.Rule = smvRule;
                caseLine.Result = new Expression(choiceVarName + " != 0");
            }
            smvCase.CaseLines.Add(caseLine);
            smvCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(choiceVarName + " = 0"));
            return smvCase;
        }

        public static Case getChoiceNonDetCase(Module module, string choiceVarName, ExecutionStrategy kpES, int strategyIndex)
        {
            Case newCase = new Case();
            CaseLine caseLine = new CaseLine();
            //((_status = _ACTIVE) & (_turn = _CHO0)) : case
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            int count = 1;
            UnionCases unionCases = new UnionCases();
            foreach (var rule in kpES.Rules)
            {
                Case unionCase = new Case();
                CaseLine unionLine = new CaseLine();
                unionLine.Rule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
                unionLine.Result = new Expression(count.ToString());
                unionCase.CaseLines.Add(unionLine);
                unionCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
                unionCases.addCase(unionCase);
                count++;
            }
            caseLine.Result = new Expression(TUtil.tabInnerCase(unionCases));
            newCase.CaseLines.Add(caseLine);

            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));

            return newCase;
        }

        private static void addARB2Turn(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            string turnState = TurnStates.ARBITRARY + strategyIndex;
            string arbVarName = CustomVariables.ARBITRARY + strategyIndex;
            //((_turn = _ARB2)) : case
            CaseLine caseLine = new CaseLine();
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(turnState);

            Case innerCase = new Case();

            //(_count < _rand) & _arb1 != 0 : _ARB1;
            CaseLine innerCaseLine1 = new CaseLine();
            BoolExp countLTRand =
                new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.LT, CustomVariables.RAND);
            BoolExp arbNEQZero =
                new BoolExp(arbVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            innerCaseLine1.Rule.Condition = new CompoundBoolExpression(countLTRand, BinaryOperator.AND, arbNEQZero);
            innerCaseLine1.Result = new Expression(turnState);
            innerCase.CaseLines.Add(innerCaseLine1);
            //add TRUE caseline
            string nextExecution = "";
            if (kpES.Next == null)
            {
                nextExecution = TurnStates.READY;
            }
            else
                nextExecution = TurnStates.getState(kpES.Next.Operator) + (strategyIndex + 1);//next strategy, eg. sequence2
            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(nextExecution));
            //add condition and inner case to turn's case
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase));
            module.Turn.Next.addCaseLine(caseLine);
        }

        private static CaseLine addARBCaseLine2Count(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            string turnState = TurnStates.ARBITRARY + strategyIndex;
            string arbVarName = CustomVariables.ARBITRARY + strategyIndex;
            CaseLine caseLine1 = new CaseLine();
            //(_status = _ACTIVE) & (_turn = _ARB1) & (_count < _rand) : case
            caseLine1.Rule.Condition = BRulesStandardVar.getTurnCondition(turnState);
            BoolExp countLTRand = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.LT, CustomVariables.RAND);
            caseLine1.Rule.AppendBoolExpression(countLTRand, BinaryOperator.AND);

            Case innerCase = new Case();
            //_arb1 != 0 : _count + 1;
            CaseLine inLine = new CaseLine();
            BoolExp arbNEQZero = new BoolExp(arbVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            inLine.Rule.Condition = arbNEQZero;
            inLine.Result = new Expression(CustomVariables.COUNT + " + 1");
            innerCase.CaseLines.Add(inLine);
            //TRUE : 1; --if next strategy is arb or seq type then it should start by 1, 0 otherwise
            string trueResult = "0";
            if (kpES.Next != null)
            {
                if (TurnStates.getState(kpES.Next.Operator) == TurnStates.ARBITRARY || TurnStates.getState(kpES.Next.Operator) == TurnStates.SEQUENCE)
                    trueResult = "1";
            }
            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(trueResult));
            caseLine1.Result = new Expression(TUtil.tabInnerCase(innerCase));

            return caseLine1;
        }

        private static void addChoice2Turn(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            CaseLine caseLine = new CaseLine();
            string turnState = TurnStates.CHOICE + strategyIndex;

            //if turn is choice, then next is the next execution strategy.
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(turnState);
            //if it is last execution, then next is the system
            string nextExecution = "";
            if (kpES.Next == null)
            {
                nextExecution = TurnStates.READY;
            }
            else
                nextExecution = TurnStates.getState(kpES.Next.Operator) + (strategyIndex + 1);//next strategy, eg. sequence2

            Expression result = new Expression(nextExecution);
            caseLine.Result = result;
            //add case to system.
            module.Turn.Next.addCaseLine(caseLine);
        }

        /// <summary> 
        /// if next strategy is either arbitrary or sequence then start counter from 1 (_status = _ACTIVE) & ((next
        /// (_turn) = _ARB1) | (next (_turn) = _SEQ2)) : 1; TRUE : 0; 
        /// </summary> 
        /// <param name="module"></param> 
        /// <param name="kpES"></param>
        private static void addLastCaseLine2Count(Module module, ExecutionStrategy kpES)
        {
            int strategyIndex = 0;
            CaseLine caseLine = new CaseLine();
            NuSMV.Rule rule = null;
            while (kpES != null)
            {
                //((next(_turn) = _SEQ0) : 1;
                ICondition nextTurn = null;
                string nextTurnStr = SMVKeys.NEXT + "(" + CustomVariables.TURN + ")";
                if (kpES.Operator == StrategyOperator.ARBITRARY)
                {
                    nextTurn = new BoolExp(nextTurnStr, NuSMV.RelationalOperator.EQUAL, TurnStates.ARBITRARY + strategyIndex);
                }
                else if (kpES.Operator == StrategyOperator.SEQUENCE)
                {
                    nextTurn = new BoolExp(nextTurnStr, NuSMV.RelationalOperator.EQUAL, TurnStates.SEQUENCE + strategyIndex);
                }
                if (nextTurn != null)
                {
                    if (rule == null)//if at least one arbt or seq exists then activate this case line
                        rule = new NuSMV.Rule();
                    rule.AppendBoolExpression(nextTurn, BinaryOperator.OR);
                }
                kpES = kpES.Next;
                strategyIndex++;
            }
            if (rule != null)
            {
                caseLine.Rule = rule;
                caseLine.Result = new Expression("1");
                module.Count.Next.addCaseLine(caseLine);
            }
            //regardless of previous calculation, always add TRUE : 0; to at the very end.
            module.Count.Next.addCaseLine(BRulesStandardVar.trueCaseLine("0"));
        }

        private static void addMAX2Turn(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            string turnState = TurnStates.MAX + strategyIndex;
            string maxVarName = CustomVariables.MAX + strategyIndex;
            //(_status = _ACTIVE) & (_turn = _MAX3) : case
            CaseLine caseLine = new CaseLine();
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(turnState);

            Case innerCase = new Case();
            //_max3 != 0 : _MAX3;
            CaseLine innerLine = new CaseLine();
            BoolExp maxNEQZero =
                new BoolExp(maxVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            innerLine.Rule.Condition = maxNEQZero;
            innerLine.Result = new Expression(turnState);
            innerCase.CaseLines.Add(innerLine);
            //add TRUE caseline
            string nextExecution = "";
            if (kpES.Next == null)
            {
                nextExecution = TurnStates.READY;
            }
            else
                nextExecution = TurnStates.getState(kpES.Next.Operator) + (strategyIndex + 1);//next strategy, eg. sequence2
            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(nextExecution));
            //add condition and inner case to turn's case
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase));
            module.Turn.Next.addCaseLine(caseLine);
        }

        private static ICaseLine addMAXCaseLine2Count(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            string maxVarName = CustomVariables.MAX + strategyIndex;
            CaseLine caseLine1 = new CaseLine();
            //(_status = _ACTIVE) & (_turn = _MAX3) : case
            caseLine1.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            Case innerCase = new Case();
            //_max3 != 0 : 1;
            CaseLine inLine = new CaseLine();
            BoolExp arbNEQZero = new BoolExp(maxVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            inLine.Rule.Condition = arbNEQZero;
            inLine.Result = new Expression("1");
            innerCase.CaseLines.Add(inLine);
            //TRUE : 0; --if next strategy is arb or seq type then it should start by 1, 0 otherwise
            string trueResult = "0";
            if (kpES.Next != null)
            {
                if (TurnStates.getState(kpES.Next.Operator) == TurnStates.ARBITRARY || TurnStates.getState(kpES.Next.Operator) == TurnStates.SEQUENCE)
                    trueResult = "1";
            }
            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(trueResult));
            caseLine1.Result = new Expression(TUtil.tabInnerCase(innerCase));

            return caseLine1;
        }

        /// <summary>
        /// Add custom caselines to turn variable. In each execution strategy some caselines already has been added to turn,
        /// here we will add the cases which should be at very beginning or at the end
        /// </summary>
        /// <param name="module"></param>
        /// <param name="kpType"></param>
        private static void addCaseLines2Turn(Module module, MType kpType)
        {
            ExecutionStrategy kpES = kpType.ExecutionStrategy;
            //Add first cases of turn variable
            //When division rule exists, and when they are activated, they should immediately start execution from first strategy.
            //This case should be very first one.
            int insertIndex = 0;
            if (module.HasDivisionRule)
            {
                //((_status = _NONEXIST) & (next(_status) = _ACTIVE)):_CHO0;
                addExecuteChildWhenNextStatusActive2Turn(module, kpES, insertIndex);
                insertIndex++;
            }
            //Then add the active status to turn variable
            //(_status = _ACTIVE) & (_turn = _READY)) -> (_sync = _BUSY) : _READY; (_sync = _EXCH) : _CHO0; TRUE : _turn;
            addActiveStatusCaseLine2Turn(module, kpES, insertIndex);

            //Add last cases of TURN variable
            //(((_status = _NONEXIST) | (_status = _DIVIDED)) | (_status = _DISSOLVED)) : _READY;
            if (module.HasDissolutionRule || module.HasDivisionRule)
            {
                addInActiveStatus2Turn(module, kpES);
            }
            //always add TRUE caseline at the very end of the Turn variable.
            module.Turn.Next.addCaseLine(BRulesStandardVar.trueCaseLine(CustomVariables.TURN));
        }

        /// <summary> 
        /// When division rule exists, and when they are activated, they should immediately start execution from first strategy.
        /// ((_status = _NONEXIST) & (next(_status) = _ACTIVE)):_CHO0 (First execution strategy);
        /// </summary> 
        /// <param name="module"></param>
        /// <param name="kpES">first executionStrategy</param>
        private static void addExecuteChildWhenNextStatusActive2Turn(Module module, ExecutionStrategy kpES, int insertIndex)
        {
            CaseLine caseLine = new CaseLine();

            CompoundBoolExpression compound = BRulesStandardVar.getStatusNonExistNEXTActive(module);
            caseLine.Rule.Condition = compound;

            Expression result = new Expression(TurnStates.getState(kpES.Operator) + 0);

            caseLine.Result = result;
            module.Turn.Next.CaseStatement.CaseLines.Insert(insertIndex, caseLine);
        }

        /// <summary> 
        /// If module Active ((_status = _ACTIVE) & (_turn = _READY)) : case (_sync = _BUSY) : _READY; --stay in
        /// READY state until all the other modules to be ready as well (_sync = _EXCH) : _CHO0; --start over, namely go to
        /// first strategy TRUE : _turn; esac; 
        /// </summary> 
        /// <param name="module"></param>
        /// <param name="kpES">The first execution strategy</param>
        private static void addActiveStatusCaseLine2Turn(Module module, ExecutionStrategy kpES, int insertIndex)
        {
            CaseLine caseLine = new CaseLine();
            caseLine.Rule.Condition = BRulesStandardVar.getStatusAndTurnCondition(module, TurnStates.READY);

            Case innerCase = new Case();
            //sync = busy : _READY;
            CaseLine inLine1 = new CaseLine();
            inLine1.Rule.Condition = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.BUSY);
            inLine1.Result = new Expression(TurnStates.READY);
            innerCase.CaseLines.Add(inLine1);
            //sync = exch : _CHO0;
            CaseLine inLine2 = new CaseLine();
            inLine2.Rule.Condition = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.EXCHANGE);
            inLine2.Result = new Expression(TurnStates.getState(kpES.Operator) + 0);
            innerCase.CaseLines.Add(inLine2);
            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(CustomVariables.TURN));

            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase));

            module.Turn.Next.CaseStatement.CaseLines.Insert(insertIndex, caseLine);
        }

        /// <summary>
        /// Inactive status are, NONEXIST, divided or dissolved. Remained status are not active. If status is inactive, then
        /// turn should stay in READY state. //((_status = _NONEXIST) | (_status = _DIVIDED)) |(_status = _DISSOLVED) :_READY;
        /// </summary>
        /// <param name="module"></param>
        /// <param name="kpES"></param>
        private static void addInActiveStatus2Turn(Module module, ExecutionStrategy kpES)
        {
            CaseLine caseLine = new CaseLine();
            if (module.HasDivisionRule)
            {
                ICondition nonExist = BRulesStandardVar.getStatusCondition(module, StatusStates.NONEXIST);
                ICondition divided = BRulesStandardVar.getStatusCondition(module, StatusStates.DIVIDED);
                caseLine.Rule.Condition = new CompoundBoolExpression(nonExist, BinaryOperator.OR, divided);
            }
            if (module.HasDissolutionRule)
            {
                caseLine.Rule.AppendBoolExpression(BRulesStandardVar.getStatusCondition(module, StatusStates.DISSOLVED), BinaryOperator.OR);
            }
            caseLine.Result = new Expression(TurnStates.READY);
            module.Turn.Next.addCaseLine(caseLine);
        }

        /// <summary> 
        /// ((_status = _ACTIVE) & (_turn = _SEQ2)) : case (_seq0 = x) : _READY; -- x is the dissolution rule id, it must be the first caseline.
        /// _seq2 = 2 : _MAX3; -- Completed all rules( 2 is the max num of rules) continue to the NEXT EXEC STRA 
        /// _seq2 != 0 : _SEQ2; -- Continue next rule in this strategy
        /// _seq2 = 0 : _READY; --It cannot execute current rule, so skip remained strategies and move to system position.
        /// --	_seq2!=0 & _seq2<=2 : _SEQ2;
        /// --((_seq2 = 1) & (signal >= 1)) : _SEQ2;
        /// --((_seq2 = 2) & ((a >= 1) & (signal >= 1))) : _SYS; TRUE : _READY; esac; 
        /// </summary> 
        /// <param name="module"></param>
        /// <param name="kpES"></param>
        /// <param name="strategyIndex"></param>
        /// <param name="sequence"></param>
        private static void addSEQ2Turn(Module module, ExecutionStrategy kpES, int strategyIndex, IVar sequence)
        {
            CaseLine caseLine = new CaseLine();

            string turnState = TurnStates.SEQUENCE + strategyIndex;
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(turnState);

            //string seqVarName = CustomVariables.SEQUENCE + strategyIndex;
            string nextExecution = "";
            Case innerCase = new Case();

            //_seq2 != 0 : _SEQ2; -- Continue next rule
            CaseLine caseLine1 = new CaseLine();
            caseLine1.Rule.Condition = new BoolExp(sequence.Name, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            caseLine1.Result = new Expression(turnState);
            innerCase.CaseLines.Add(caseLine1);

            if (kpES.Next == null)
            {
                nextExecution = TurnStates.READY;
            }
            else
                nextExecution = TurnStates.getState(kpES.Next.Operator) + (strategyIndex + 1);//next strategy

            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(nextExecution));
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase));

            module.Turn.Next.addCaseLine(caseLine);
        }

        private static int getDissolutionRuleID(ExecutionStrategy kpES)
        {
            int ruleID = 0;
            foreach (var rule in kpES.Rules)
            {
                if (rule.Type == RuleType.MEMBRANE_DISSOLUTION)
                {
                    ruleID = rule.Id;
                    break;
                }
            }
            return ruleID;
        }

        /// <summary>
        /// (_cho1 = 1) | (_cho1 = 2) | (_cho1 = 3))))
        /// </summary>
        /// <param name="kpES"></param>
        /// <returns></returns>

        private static List<int> getDivisionRuleIDs(ExecutionStrategy kpES)
        {
            List<int> ruleIDs = new List<int>();
            foreach (var rule in kpES.Rules)
            {
                if (rule.Type == RuleType.MEMBRANE_DIVISION)
                {
                    ruleIDs.Add(rule.Id);
                }
            }
            return ruleIDs;
        }

        private static ICaseLine addSEQCaseLine2Count(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            string turnState = TurnStates.SEQUENCE + strategyIndex;
            string seqVarName = CustomVariables.SEQUENCE + strategyIndex;
            CaseLine caseLine = new CaseLine();
            //(_turn = _SEQ2) & (_count < max#OfRule) : case
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(turnState);
            BoolExp countLTMAXRule = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.LT, kpES.Rules.Count.ToString());
            caseLine.Rule.AppendBoolExpression(countLTMAXRule, BinaryOperator.AND);

            Case innerCase = new Case();

            //_seq2 != 0 : _count + 1;
            CaseLine inLine2 = new CaseLine();
            BoolExp seqNEQZero = new BoolExp(seqVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            inLine2.Rule.Condition = seqNEQZero;
            inLine2.Result = new Expression(CustomVariables.COUNT + " + 1");
            innerCase.CaseLines.Add(inLine2);
            //TRUE : 0; if seq halts turn goes to ready, so next turn will be zero in any way.
            string trueResult = "0";

            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(trueResult));
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase));

            return caseLine;
        }

        /// <summary>
        /// (_seq0 = 2) : _READY;
        /// </summary>
        /// <param name="kpES"></param>
        /// <param name="seqVarName">result</param>
        /// <returns></returns>
        private static CaseLine getDissolutionCondition(ExecutionStrategy kpES, string seqVarName, string result)
        {
            CaseLine inLine1 = null;
            int ruleID = getDissolutionRuleID(kpES);
            //if dissolution rule inside current execution strategy
            if (ruleID != 0)
            {
                inLine1 = new CaseLine();
                inLine1.Rule.Condition = new BoolExp(seqVarName, NuSMV.RelationalOperator.EQUAL, ruleID.ToString());
                inLine1.Result = new Expression(result);
            }

            return inLine1;
        }

        /// <summary>
        /// Identify division rule IDs convert them to case line
        /// </summary>
        /// <param name="kpES"></param>
        /// <param name="seqVarName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static CaseLine getDivisionCondition(ExecutionStrategy kpES, string seqVarName, string result)
        {
            CaseLine inLine1 = null;
            List<int> ruleIDs = getDivisionRuleIDs(kpES);
            //if dissolution rule inside current execution strategy
            if (ruleIDs.Count > 0)
            {
                inLine1 = new CaseLine();
                NuSMV.Rule ruleCon = new NuSMV.Rule();
                CompoundBoolExpression compExpr = new CompoundBoolExpression();
                //is last element
                foreach (var ruleID in ruleIDs)
                {
                    //(_cho1 = 1) || (_cho1 = 2))
                    ruleCon.AppendBoolExpression(new BoolExp(seqVarName, NuSMV.RelationalOperator.EQUAL, ruleID.ToString()), BinaryOperator.OR);
                }
                inLine1.Rule.Condition = ruleCon.Condition;
                inLine1.Result = new Expression(result);
            }
            return inLine1;
        }

        private static void buildARBCustomVariables(Module module, ExecutionStrategy kpES, int strategyIndex, ArbitraryStrategy arbStrategy)
        {
            List<IVar> customVar = new List<IVar>();
            arbStrategy.CustomVars = customVar;

            string arbVarName = CustomVariables.ARBITRARY + strategyIndex;
            NonDeterVar arb = new NonDeterVar(arbVarName);
            arb.Behaviour = VariableBehaviour.CUSTOM;
            arb.Type = new NuSMV.BoundInt(0, kpES.Rules.Count);
            arb.INVARCase = getARBINVAR(module, arbVarName, kpES, strategyIndex);
            arb.NonDetCase = getARBNonDetCase(module, kpES, strategyIndex);
            customVar.Add(arb);

            bool randVarExist = false;
            //If there are previous arbt strategies, we will use their rand variable
            ArbitraryStrategy firstArbitraryExecutionStrategy = null;
            //One rand variable will be enough if there are more than one arb strategy, so if rand variable has not created then create one.
            foreach (Strategy strategy in module.ExecutionStrategies)
            {
                //if there is another arbitrary strategy, then rand is already there.
                if (strategy is ArbitraryStrategy)
                {
                    //other than current strategy
                    if (strategy != arbStrategy)
                    {
                        randVarExist = true;
                        firstArbitraryExecutionStrategy = strategy as ArbitraryStrategy;
                        break;
                    }
                }
            }
            Variable rand = null;
            //if rand does not exist, generate new one
            if (!randVarExist)
            {
                //_randNum : 0 .. 10;
                rand = new Variable(CustomVariables.RAND);
                rand.Behaviour = VariableBehaviour.CUSTOM;
                rand.Type = new NuSMV.BoundInt(0, BoundedNumbers.MAXRANDOM);

                //TRUE: 0; add as last item, the remained cases will be added at follows
                rand.Next.addCaseLine(BRulesStandardVar.trueCaseLine("0"));

                customVar.Add(rand);
            }
            else//bring existing one.
                rand = (Variable)firstArbitraryExecutionStrategy.CustomVars.First(item => item.Name.Equals(CustomVariables.RAND));

            //Add remained caselines e.g., next(_turn) = _ARB3)) & (_rand = 0)) : 1 .. 3;
            addCaseLines2Rand(module, kpES, strategyIndex);
            //rand.Next = getRandNext(module, kpES, strategyIndex, customVar);

            //add guards for each rule of arbitrary.
            buildARBGuardVariables(module, kpES, strategyIndex, customVar);
        }

        private static Next buildARBGuardNext(Module module, KpCore.Rule rule, Variable guard, int strategyIndex)
        {
            Next next = new Next();
            Case newCase = new Case();
            CaseLine caseLine1 = new CaseLine();
            string arbVarName = CustomVariables.ARBITRARY + strategyIndex;
            //(((_turn = _ARB1) & (_arb1 != 0)) & (_count = 1) & g1 = 1) : TRUE;
            caseLine1.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            BoolExp arbNEQZero =
               new BoolExp(arbVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            BoolExp countEQOne = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "1");
            caseLine1.Rule.AppendBoolExpression(new CompoundBoolExpression(arbNEQZero, BinaryOperator.AND, countEQOne), BinaryOperator.AND);

            if (rule.IsGuarded)
            {
                ICondition guardCondition = BRulesStandardVar.extractGuardConditionFromKPRule(rule);
                caseLine1.Rule.AppendBoolExpression(guardCondition, BinaryOperator.AND);
            }
            caseLine1.Result = new Expression(Truth.TRUE);
            newCase.CaseLines.Add(caseLine1);

            //((_status = _ACTIVE) & (next (_turn) = _ARB1) & (_arb1 != 0)) : _guard3;
            CaseLine caseLine2 = new CaseLine();
            caseLine2.Rule.Condition = BRulesStandardVar.getStatusAndNextTurnCondition(module, TurnStates.ARBITRARY + strategyIndex);
            caseLine2.Rule.AppendBoolExpression(arbNEQZero, BinaryOperator.AND);
            Expression result = new Expression(guard.Name);
            caseLine2.Result = result;
            newCase.CaseLines.Add(caseLine2);
            //TRUE : FALSE;
            CaseLine caseLine3 = BRulesStandardVar.trueCaseLine(Truth.FALSE);
            newCase.CaseLines.Add(caseLine3);

            next.CaseStatement = newCase;
            return next;
        }

        private static void buildARBGuardVariables(Module module, ExecutionStrategy kpES, int strategyIndex, List<IVar> customVar)
        {
            foreach (var rule in kpES.Rules)
            {
                //g1 : boolean;
                Variable guard = new Variable(SMVPreFix.getGuardName(module, strategyIndex, rule));
                guard.Behaviour = VariableBehaviour.CUSTOM;
                guard.Type = new SBool();
                guard.Init = Truth.FALSE;
                customVar.Add(guard);
                guard.Next = buildARBGuardNext(module, rule, guard, strategyIndex);
            }
        }

        private static Next buildMAXGuardNext(Module module, KpCore.Rule rule, Variable guard, int strategyIndex)
        {
            Next next = new Next();
            Case newCase = new Case();
            //(((_turn = _MAX3) & (_max3 != 0)) & (_count = 0) & signal >= 1) : TRUE;
            CaseLine caseLine1 = new CaseLine();
            string maxVarName = CustomVariables.MAX + strategyIndex;

            caseLine1.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            BoolExp maxNEQZero =
               new BoolExp(maxVarName, NuSMV.RelationalOperator.NOT_EQUAL, "0");
            BoolExp countEQZero = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "0");
            caseLine1.Rule.AppendBoolExpression(new CompoundBoolExpression(maxNEQZero, BinaryOperator.AND, countEQZero), BinaryOperator.AND);

            if (rule.IsGuarded)
            {
                ICondition guardCondition = BRulesStandardVar.extractGuardConditionFromKPRule(rule);
                caseLine1.Rule.AppendBoolExpression(guardCondition, BinaryOperator.AND);
            }
            caseLine1.Result = new Expression(Truth.TRUE);
            newCase.CaseLines.Add(caseLine1);

            //((_status = _ACTIVE) & (next (_turn) = _MAX3) & (_max3 != 0)) : _guard7;
            CaseLine caseLine2 = new CaseLine();
            caseLine2.Rule.Condition = BRulesStandardVar.getStatusAndNextTurnCondition(module, TurnStates.MAX + strategyIndex);
            caseLine2.Rule.AppendBoolExpression(maxNEQZero, BinaryOperator.AND);
            Expression result = new Expression(guard.Name);
            caseLine2.Result = result;
            newCase.CaseLines.Add(caseLine2);
            //TRUE : FALSE;
            CaseLine caseLine3 = BRulesStandardVar.trueCaseLine(Truth.FALSE);
            newCase.CaseLines.Add(caseLine3);

            next.CaseStatement = newCase;
            return next;
        }

        /// <summary>
        /// ((_status = _ACTIVE) & (_turn = _READY)) : case
        /// sync = busy : _READY;
        /// sync = exch : _CHO0;
        /// TRUE : _turn;
        /// esac;
        /// </summary>
        /// <param name="module"></param>
        /// <param name="kpES"></param>
        private static void buildMAXGuardVariables(Module module, ExecutionStrategy kpES, int strategyIndex, List<IVar> customVar)
        {
            foreach (var rule in kpES.Rules)
            {
                //g1 : boolean;
                Variable guard = new Variable(SMVPreFix.getGuardName(module, strategyIndex, rule));
                guard.Behaviour = VariableBehaviour.CUSTOM;
                guard.Type = new SBool();
                guard.Init = Truth.FALSE;
                guard.Next = buildMAXGuardNext(module, rule, guard, strategyIndex);
                customVar.Add(guard);
            }
        }

        private static Case getARBINVAR(Module module, string arbVarName, ExecutionStrategy kpES, int strategyIndex)
        {
            Case newCase = new Case();
            CaseLine caseLine = new CaseLine();
            //((_status = _ACTIVE) & _turn = _ARB1) : case
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            // _count = 1 : case --(in first arb step, check with all LHS)
            Case innerCase1 = new Case();
            CaseLine innerLine1 = new CaseLine();//first line in inner1

            innerLine1.Rule.Condition = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "1");
            //(g1 = 1 & a >= 1) | (b >= 1) : _arb1 != 0; --with LHS
            Case innerCase2 = new Case();// innercase of innerCase1
            CaseLine innerLine1_in2 = new CaseLine();//second case first line
            NuSMV.Rule countEQZeroRules = null;
            foreach (var rule in kpES.Rules)
            {
                NuSMV.Rule tempRule = null;
                tempRule = BRulesStandardVar.extractStandardRuleWithGuard4ARBandMAX(rule);
                if (tempRule != null)
                {
                    if (countEQZeroRules == null)
                    {
                        countEQZeroRules = tempRule;
                    }
                    else
                        countEQZeroRules.AppendBoolExpression(tempRule.Condition, BinaryOperator.OR);
                }
            }
            innerLine1_in2.Rule = countEQZeroRules;
            innerLine1_in2.Result = new Expression(arbVarName + " != 0");
            innerCase2.CaseLines.Add(innerLine1_in2);
            // TRUE: _arb1 = 0;
            innerCase2.CaseLines.Add(BRulesStandardVar.trueCaseLine(arbVarName + " = 0"));
            innerLine1.Result = new Expression(TUtil.tabInnerCase(innerCase2));
            innerCase1.CaseLines.Add(innerLine1);

            //(_count <= _rand) : case
            CaseLine innerLine2 = new CaseLine();//first case second line
            innerLine2.Rule.Condition = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.LEQ, CustomVariables.RAND);
            Case innerCase3 = new Case();
            CaseLine innerLine1_in3 = new CaseLine();//third case, first line
            NuSMV.Rule countLEQRandRules = null;
            foreach (var rule in kpES.Rules)
            {
                NuSMV.Rule tempRule = null;
                tempRule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
                tempRule.AddBoolExpression(new BoolVariable(SMVPreFix.getGuardName(module, strategyIndex, rule)), BinaryOperator.AND);
                if (tempRule != null)
                {
                    if (countLEQRandRules == null)
                    {
                        countLEQRandRules = tempRule;
                    }
                    else
                        countLEQRandRules.AppendBoolExpression(tempRule.Condition, BinaryOperator.OR);
                }
            }
            innerLine1_in3.Rule = countLEQRandRules;
            innerLine1_in3.Result = new Expression(arbVarName + " != 0");
            innerCase3.CaseLines.Add(innerLine1_in3);
            // TRUE: _arb1 = 0;
            innerCase3.CaseLines.Add(BRulesStandardVar.trueCaseLine(arbVarName + " = 0"));
            innerLine2.Result = new Expression(TUtil.tabInnerCase(innerCase3));
            innerCase1.CaseLines.Add(innerLine2);

            // TRUE: _arb1 = 0;
            innerCase1.CaseLines.Add(BRulesStandardVar.trueCaseLine(arbVarName + " = 0"));

            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase1));
            newCase.CaseLines.Add(caseLine);
            // TRUE: _arb1 = 0;
            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(arbVarName + " = 0"));

            return newCase;
        }

        private static Case getARBNonDetCase(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            Case newCase = new Case();
            string arbStrategy = TurnStates.ARBITRARY + strategyIndex;
            CaseLine caseLine = new CaseLine();
            //((_status = _ACTIVE) & _turn = _ARB1) : case
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(arbStrategy);
            Case innerCase1 = new Case();
            // _count = 1 : case
            CaseLine inLine1 = new CaseLine();
            BoolExp countEQ1 = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "1");
            inLine1.Rule.Condition = countEQ1;
            UnionCases unionCases1 = new UnionCases();
            int count = 1;
            foreach (var rule in kpES.Rules)
            {
                Case unionCase = new Case();
                CaseLine unionLine1 = new CaseLine();
                unionLine1.Rule = BRulesStandardVar.extractStandardRuleWithGuard4ARBandMAX(rule);
                unionLine1.Result = new Expression(count.ToString());
                unionCase.CaseLines.Add(unionLine1);
                unionCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
                unionCases1.addCase(unionCase);
                count++;
            }
            inLine1.Result = new Expression(TUtil.tabInnerCase(unionCases1));
            innerCase1.CaseLines.Add(inLine1);
            // (_count <= _rand) : case
            CaseLine inLine2 = new CaseLine();
            BoolExp countLEQRand = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.LEQ, CustomVariables.RAND);
            inLine2.Rule.Condition = countLEQRand;
            UnionCases unionCases2 = new UnionCases();
            count = 1;
            foreach (var rule in kpES.Rules)
            {
                Case unionCase = new Case();
                CaseLine unionLine1 = new CaseLine();
                unionLine1.Rule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
                unionLine1.Rule.AddBoolExpression(new BoolVariable(SMVPreFix.getGuardName(module, strategyIndex, rule)), BinaryOperator.AND);
                unionLine1.Result = new Expression(count.ToString());
                unionCase.CaseLines.Add(unionLine1);
                unionCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
                unionCases2.addCase(unionCase);
                count++;
            }
            inLine2.Result = new Expression(TUtil.tabInnerCase(unionCases2));
            innerCase1.CaseLines.Add(inLine2);
            // TRUE: 0;
            innerCase1.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase1));

            newCase.CaseLines.Add(caseLine);
            // TRUE: 0;
            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
            return newCase;
        }

        private static Case getMAXINVAR(Module module, string maxVarName, ExecutionStrategy kpES, int strategyIndex)
        {
            Case newCase = new Case();
            CaseLine caseLine = new CaseLine();
            //((_status = _ACTIVE) & _turn = _MAX3) : case
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);

            // _count = 0 : case --(in first arb step, check with all LHS)
            Case innerCase1 = new Case();
            CaseLine innerLine1 = new CaseLine();//first line in inner1

            innerLine1.Rule.Condition = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "0");
            //(signal = 1 & c >= 1) | (c >= 1) : _max3 != 0; --with LHS
            Case innerCase2 = new Case();// innercase of innerCase1
            CaseLine innerLine1_in2 = new CaseLine();//second case first line
            NuSMV.Rule countEQZero = null;
            foreach (var rule in kpES.Rules)
            {
                NuSMV.Rule tempRule = null;
                tempRule = BRulesStandardVar.extractStandardRuleWithGuard4ARBandMAX(rule);
                if (tempRule != null)
                {
                    if (countEQZero == null)
                    {
                        countEQZero = tempRule;
                    }
                    else
                        countEQZero.AppendBoolExpression(tempRule.Condition, BinaryOperator.OR);
                }
            }
            innerLine1_in2.Rule = countEQZero;
            innerLine1_in2.Result = new Expression(maxVarName + " != 0");
            innerCase2.CaseLines.Add(innerLine1_in2);
            // TRUE: _arb1 = 0;
            innerCase2.CaseLines.Add(BRulesStandardVar.trueCaseLine(maxVarName + " = 0"));
            innerLine1.Result = new Expression(TUtil.tabInnerCase(innerCase2));
            innerCase1.CaseLines.Add(innerLine1);

            //(_count = 1) : case
            CaseLine innerLine2 = new CaseLine();//first case second line
            innerLine2.Rule.Condition = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "1");
            Case innerCase3 = new Case();
            CaseLine innerLine1_in3 = new CaseLine();//third case, first line
            NuSMV.Rule countEQOne = null;
            foreach (var rule in kpES.Rules)
            {
                NuSMV.Rule tempRule = null;
                tempRule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);

                tempRule.AddBoolExpression(new BoolVariable(SMVPreFix.getGuardName(module, strategyIndex, rule)), BinaryOperator.AND);
                if (tempRule != null)
                {
                    if (countEQOne == null)
                    {
                        countEQOne = tempRule;
                    }
                    else
                        countEQOne.AppendBoolExpression(tempRule.Condition, BinaryOperator.OR);
                }
            }
            innerLine1_in3.Rule = countEQOne;
            innerLine1_in3.Result = new Expression(maxVarName + " != 0");
            innerCase3.CaseLines.Add(innerLine1_in3);
            // TRUE: _arb1 = 0;
            innerCase3.CaseLines.Add(BRulesStandardVar.trueCaseLine(maxVarName + " = 0"));
            innerLine2.Result = new Expression(TUtil.tabInnerCase(innerCase3));
            innerCase1.CaseLines.Add(innerLine2);

            // TRUE: _arb1 = 0;
            innerCase1.CaseLines.Add(BRulesStandardVar.trueCaseLine(maxVarName + " = 0"));

            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase1));
            newCase.CaseLines.Add(caseLine);
            // TRUE: _arb1 = 0;
            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(maxVarName + " = 0"));

            return newCase;
        }

        private static Case getMAXNonDetCase(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            Case newCase = new Case();

            CaseLine caseLine = new CaseLine();
            //((_status = _ACTIVE) & _turn = _MAX3) : case
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(module, strategyIndex);
            Case innerCase1 = new Case();
            // _count = 0 : case
            CaseLine inLine1 = new CaseLine();
            BoolExp countEQZero = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "0");
            inLine1.Rule.Condition = countEQZero;
            UnionCases unionCases1 = new UnionCases();
            int count = 1;
            foreach (var rule in kpES.Rules)
            {
                Case unionCase = new Case();
                CaseLine unionLine1 = new CaseLine();
                unionLine1.Rule = BRulesStandardVar.extractStandardRuleWithGuard4ARBandMAX(rule);
                unionLine1.Result = new Expression(count.ToString());
                unionCase.CaseLines.Add(unionLine1);
                unionCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
                unionCases1.addCase(unionCase);
                count++;
            }
            inLine1.Result = new Expression(TUtil.tabInnerCase(unionCases1));
            innerCase1.CaseLines.Add(inLine1);
            //(_count = 1) : case
            CaseLine inLine2 = new CaseLine();
            BoolExp countEQOne = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, "1");
            inLine2.Rule.Condition = countEQOne;
            UnionCases unionCases2 = new UnionCases();
            count = 1;
            foreach (var rule in kpES.Rules)
            {
                Case unionCase = new Case();
                CaseLine unionLine1 = new CaseLine();
                unionLine1.Rule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
                unionLine1.Rule.AddBoolExpression(new BoolVariable(SMVPreFix.getGuardName(module, strategyIndex, rule)), BinaryOperator.AND);
                unionLine1.Result = new Expression(count.ToString());
                unionCase.CaseLines.Add(unionLine1);
                unionCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
                unionCases2.addCase(unionCase);
                count++;
            }
            inLine2.Result = new Expression(TUtil.tabInnerCase(unionCases2));
            innerCase1.CaseLines.Add(inLine2);
            // TRUE: 0;
            innerCase1.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase1));
            newCase.CaseLines.Add(caseLine);
            // TRUE: 0;
            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
            return newCase;
        }

        /// <summary> 
        /// For each arbitrary execution strategy, it adds the following lines to the next of rand variable
        /// (next(_turn) = _ARB3)) & (_rand = 0)) : 1 .. 3;
        /// (next(_turn) = _ARB3)) & (_arb3 != 0)) : _rand;
        /// </summary>
        /// <param name="module"></param>
        /// <param name="kpES"></param>
        /// <param name="strategyIndex"></param>
        /// <param name="customVar"></param>
        private static void addCaseLines2Rand(Module module, ExecutionStrategy kpES, int strategyIndex)
        {
            ArbitraryStrategy firstArbitraryStrategy = null;
            foreach (Strategy strategy in module.ExecutionStrategies)
            {
                if (strategy is ArbitraryStrategy)
                {
                    firstArbitraryStrategy = strategy as ArbitraryStrategy;
                    break;
                }
            }

            Variable randVariable = (Variable)firstArbitraryStrategy.CustomVars.First(item => item.Name.Equals(CustomVariables.RAND));
            if (randVariable != null)
            {
                string arbState = TurnStates.ARBITRARY + strategyIndex;
                CaseLine caseLine1 = new CaseLine();

                //((_status = _ACTIVE))
                ICondition statusActive = null;
                if (module.HasDissolutionRule || module.HasDivisionRule)
                {
                    statusActive = BRulesStandardVar.getStatusCondition(module, StatusStates.ACTIVE);
                    //((_status = _NONEXIST) & (next(_status) = _ACTIVE)))
                    ICondition statusNonExistNEXTActive = BRulesStandardVar.getStatusNonExistNEXTActive(module);
                    statusActive = new CompoundBoolExpression(statusActive, BinaryOperator.OR, statusNonExistNEXTActive);
                    //((_status = _willDISSOLVE) | (_status = _willDIVIDE))
                    ICondition statusWillDissolveOrWillDivide = BRulesStandardVar.getStatusWillDissolveOrWillDivide(module);
                    statusActive = new CompoundBoolExpression(statusActive, BinaryOperator.OR, statusWillDissolveOrWillDivide);
                }

                //turn != Arbitrary
                ICondition turnNotEqualsArbitrary = BRulesStandardVar.getTurnCondition(arbState, NuSMV.RelationalOperator.NOT_EQUAL);
                //turn = Arbitrary
                ICondition nextTurnIsArbitrary = BRulesStandardVar.getNextTurnCondition(arbState);
                ICondition arbitraryStarts = new CompoundBoolExpression(turnNotEqualsArbitrary, BinaryOperator.AND, nextTurnIsArbitrary);

                caseLine1.Rule.Condition = new CompoundBoolExpression(statusActive, BinaryOperator.AND, arbitraryStarts);
                // BoolExp randZeroCondition = new BoolExp(CustomVariables.RAND, NuSMV.RelationalOperator.EQUAL, "0");
                // caseLine1.Rule.AppendBoolExpression(randZeroCondition, BinaryOperator.AND);
                string result = "1 .. " + BoundedNumbers.MAXRANDOM;
                caseLine1.Result = new Expression(result);

                //add right before the last item.(last item already added as the TRUE statement)
                int index = randVariable.Next.CaseStatement.CaseLines.Count - 1;
                randVariable.Next.addCaseLine(index, caseLine1);

                //((_status = _ACTIVE) & next (_turn) = _ARB1) & _arb1 != 0 : _rand;
                CaseLine caseLine2 = new CaseLine();

                //turn != Arbitrary
                ICondition turnEqualsArbitrary = BRulesStandardVar.getTurnCondition(arbState);
                ICondition arbitraryContinues = new CompoundBoolExpression(turnEqualsArbitrary, BinaryOperator.AND, nextTurnIsArbitrary);

                caseLine2.Rule.Condition = new CompoundBoolExpression(statusActive, BinaryOperator.AND, arbitraryContinues);
                //BoolExp arbNEQZero = new BoolExp(CustomVariables.ARBITRARY + strategyIndex, NuSMV.RelationalOperator.NOT_EQUAL, "0");
                //caseLine2.Rule.AppendBoolExpression(arbNEQZero, BinaryOperator.AND);
                caseLine2.Result = new Expression(CustomVariables.RAND);
                index = randVariable.Next.CaseStatement.CaseLines.Count - 1;//add right before the last item.
                randVariable.Next.addCaseLine(index, caseLine2);
            }
        }

        private static Case getSEQCase(Module module, ExecutionStrategy kpES, int strategyIndex, IVar sequence)
        {
            // Next next = new Next();
            Case sequenceCase = new Case();
            string sequenceState = TurnStates.SEQUENCE + strategyIndex;
            //((_status = _ACTIVE) & (_turn = _SEQ2)) : case
            CaseLine caseLine = new CaseLine();
            caseLine.Rule.Condition = BRulesStandardVar.getTurnCondition(sequenceState);

            Case innerCase = new Case();
            int count = 1;
            foreach (var rule in kpES.Rules)
            {
                CaseLine innerLine = new CaseLine();
                innerLine.Rule = BRulesStandardVar.extractStandardRuleFromKPRule(rule, module, strategyIndex);
                BoolExp countSeq = new BoolExp(CustomVariables.COUNT, NuSMV.RelationalOperator.EQUAL, count.ToString());
                //add conditions to the rule
                innerLine.Rule.AddBoolExpression(countSeq, BinaryOperator.AND);
                innerLine.Result = new Expression(count.ToString());
                innerCase.CaseLines.Add(innerLine);
                count++;
            }
            innerCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));
            caseLine.Result = new Expression(TUtil.tabInnerCase(innerCase));

            sequenceCase.CaseLines.Add(caseLine);
            sequenceCase.CaseLines.Add(BRulesStandardVar.trueCaseLine("0"));

            return sequenceCase;
        }
    }
}