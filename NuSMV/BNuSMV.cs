using KpCore;
using NuSMV;
using System;
using System.Collections.Generic;

namespace NuSMV
{
    /// <summary>
    /// Translates KP model to SMV model.
    /// </summary>
    public class BNuSMV
    {
        private static KPsystem kpSystem = null;
        private static SMVModel nuSMV = null;

        public static SMVModel buildModel(KPsystem param_kpSystem)
        {
            kpSystem = param_kpSystem;
            if (kpSystem != null)
            {
                //set an id to each rules
                prepareKpSystem(kpSystem);
                nuSMV = new SMVModel();
                try
                {
                    //If there are division rules, generate child instances as kPInstance
                    foreach (var kpType in kpSystem.Types)
                    {
                        if (kpTypeHasDivisionRule(kpType))
                        {
                            List<KPChildInstance> kpChildInstances = BDivisionInstances.generateKPChildInstances(kpType);
                            if (kpChildInstances != null)
                                kpType.Instances.AddRange(kpChildInstances);
                        }
                    }

                    //Generate a module for each kPInstance, including child instances(if division rule exist)
                    foreach (var kpType in kpSystem.Types)
                    {
                        foreach (MInstance kpInstance in kpType.Instances)
                        {
                            Module module = new Module();
                            prepareSMVModule(module, kpType, kpInstance);
                            nuSMV.Modules.Add(module);
                        }
                    }
                    foreach (var kpType in kpSystem.Types)
                    {
                        foreach (MInstance kpInstance in kpType.Instances)
                        {
                            buildInstanceConnections(kpType, kpInstance);
                        }
                    }
                    foreach (var kpType in kpSystem.Types)
                    {
                        foreach (MInstance kpInstance in kpType.Instances)
                        {
                            Module module = nuSMV.getModule(kpType, kpInstance);
                            BVariables.generateCustomVariables(kpSystem, kpType, module);
                        }
                    }
                    foreach (var kpType in kpSystem.Types)
                    {
                        foreach (MInstance kpInstance in kpType.Instances)
                        {
                            buildModule(kpType, kpInstance);
                        }
                    }

                    if (nuSMV != null)
                    {
                        foreach (var kpType in kpSystem.Types)
                        {
                            foreach (MInstance kpInstance in kpType.Instances)
                            {
                                Module module = nuSMV.getModule(kpType, kpInstance);
                                if (module.HasDivisionRule)
                                {
                                    BDivisionInstances.generateDivisionRules(kpType, kpInstance, module);
                                }
                            }
                        }
                    }
                    //assign cases for synch variable of main module.
                    buildMain(nuSMV);
                    //finalize model, add caseLast caselines, do modifications, sorting etc.
                    assignLastCasesToNext(nuSMV);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return nuSMV;
        }

        private static void assignLastCasesToNext(SMVModel nuSMV)
        {
            foreach (var module in nuSMV.Modules)
            {
                BRulesStandardVar.assignLastCasesToNext(module);

                if (module.HasDivisionRule)
                {
                    BRulesStandardVar.assignLastCasesToDivisionVars(module);
                }
            }
        }

        /// <summary>
        /// It combines module instances with other, this should be done at the end, since all modules' instances should had
        /// been created
        /// </summary>
        /// <param name="kpType"></param>
        private static void buildInstanceConnections(MType kpType, MInstance kpInstance)
        {
            if (kpType != null)
            {
                Module module = nuSMV.getModule(kpType, kpInstance);
                module.Instance.ConnectedTo = BInstances.getInstanceConnections(nuSMV, kpSystem, kpInstance);
            }
        }

        private static void buildMain(SMVModel nuSMV)
        {
            MainModule main = new MainModule();
            //build next case for synch variable
            Case synchNext = buildSynchNext(nuSMV);
            main.Synch.Next.CaseStatement = synchNext;
            //build next case for PStep variable
            Case pStepNext = buildPStepNext(nuSMV);
            main.PStep.Next.CaseStatement = pStepNext;

            nuSMV.MainModule = main;
        }

        private static void buildModule(MType kpType, MInstance kpInstance)
        {
            if (kpType != null)
            {
                Module module = nuSMV.getModule(kpType, kpInstance);
                BStrategy.buildExecutionStrategies(module, kpType);
                //Extract variables and add next rules
                BVariables.buildVariables(kpSystem, nuSMV, module, kpType);
            }
        }

        private static Case buildPStepNext(SMVModel nuSMV)
        {
            Case newCase = new Case();
            CaseLine caseLine = new CaseLine();
            //_sync = _EXCH
            ICondition synchIsExch = new BoolExp(CustomVariables.SYNCH, NuSMV.RelationalOperator.EQUAL, SynchStates.EXCHANGE);
            caseLine.Rule.Condition = synchIsExch;
            caseLine.Result = new Expression(Truth.TRUE);
            newCase.CaseLines.Add(caseLine);
            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(Truth.FALSE));
            return newCase;
        }

        private static Case buildSynchNext(SMVModel nuSMV)
        {
            Case newCase = new Case();
            CaseLine caseLine = new CaseLine();

            foreach (var module in nuSMV.Modules)
            {
                string next = SMVKeys.NEXT + "(" + module.Instance.Name + "." + CustomVariables.TURN + ")";
                string ready = TurnStates.READY;
                caseLine.Rule.AppendBoolExpression(new BoolExp(next, NuSMV.RelationalOperator.EQUAL, ready), BinaryOperator.AND);
                if (module.HasDivisionRule)
                {
                    foreach (var childIntstance in module.ChildInstances)
                    {
                        next = SMVKeys.NEXT + "(" + childIntstance.Name + "." + CustomVariables.TURN + ")";
                        ready = TurnStates.READY;
                        caseLine.Rule.AppendBoolExpression(new BoolExp(next, NuSMV.RelationalOperator.EQUAL, ready), BinaryOperator.AND);
                    }
                }
            }
            caseLine.Result = new Expression(SynchStates.EXCHANGE);
            newCase.CaseLines.Add(caseLine);
            newCase.CaseLines.Add(BRulesStandardVar.trueCaseLine(SynchStates.BUSY));
            return newCase;
        }

        /// <summary>
        /// Check if the KP type has some division rules.
        /// </summary>
        /// <param name="kpType"></param>
        /// <returns></returns>
        private static bool kpTypeHasDivisionRule(MType kpType)
        {
            ExecutionStrategy kpES = kpType.ExecutionStrategy;
            while (kpES != null)
            {
                //check types if they have division rules
                foreach (var rule in kpES.Rules)
                {
                    if (rule.Type == RuleType.MEMBRANE_DIVISION)
                    {
                        return true;
                    }
                }
                kpES = kpES.Next;
            }
            return false;
        }

        /// <summary>
        /// Sets an ID to each rule. Generate name for unnamed instances.
        /// </summary>
        /// <param name="kpSystem"></param>
        private static void prepareKpSystem(KPsystem kpSystem)
        {
            foreach (var kpType in kpSystem.Types)
            {
                ExecutionStrategy kpES = kpType.ExecutionStrategy;
                while (kpES != null)
                {
                    int id = 1;
                    foreach (var kpRule in kpES.Rules)
                    {
                        kpRule.Id = id;
                        id++;
                    }
                    kpES = kpES.Next;
                }
                //produce a name for unnamed instances
                int count = 0;
                foreach (var kpInstance in kpType.Instances)
                {
                    if (String.IsNullOrWhiteSpace(kpInstance.Name))
                    {
                        kpInstance.Name = "_" + kpType.Name.ToLower() + count;
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// Check if type includes any communication, division, or dissolution rules
        /// </summary>
        /// <param name="module"></param>
        /// <param name="kpType"></param>
        private static void prepareSMVModule(Module module, MType kpType, MInstance kpInstance)
        {
            module.Type = kpType.Name;
            module.setModuleName(kpType.Name, kpInstance.Name);
            ExecutionStrategy kpES = kpType.ExecutionStrategy;
            bool communicationRuleExist = false;
            while (kpES != null)
            {
                if (kpES.Operator == StrategyOperator.CHOICE)
                {
                    module.HasChoiceStrategy = true;
                    module.TurnOrder.Add(TurnStates.CHOICE);
                }
                //check strategies needs substeps( seq, abrt, or max)
                else if (kpES.Operator == StrategyOperator.SEQUENCE)
                {
                    module.HasSequenceStrategy = true;
                    module.TurnOrder.Add(TurnStates.SEQUENCE);
                }
                else if (kpES.Operator == StrategyOperator.ARBITRARY)
                {
                    module.HasArbitraryStrategy = true;
                    module.TurnOrder.Add(TurnStates.ARBITRARY);
                }
                else if (kpES.Operator == StrategyOperator.MAX)
                {
                    module.HasMaxStrategy = true;
                    module.TurnOrder.Add(TurnStates.MAX);
                }
                //check types if they have comm, division or dissolution rules
                foreach (var rule in kpES.Rules)
                {
                    if (rule.Type == RuleType.REWRITE_COMMUNICATION)
                    {
                        communicationRuleExist = true;
                    }
                    else if (rule.Type == RuleType.MEMBRANE_DIVISION)
                    {
                        module.HasDivisionRule = true;
                    }
                    else if (rule.Type == RuleType.MEMBRANE_DISSOLUTION)
                    {
                        module.HasDissolutionRule = true;
                    }
                }
                kpES = kpES.Next;
            }
            bool connectionExist = false;
            foreach (MInstance minstance in kpInstance.Connections)
            {
                connectionExist = true;
                break;
            }
            // if module instances has connection and there is a communication rule, then it needs,
            //connection variable
            module.HasConnection = connectionExist && communicationRuleExist;
            // Build instances, give a name if there is not given
            BInstances.generateSMVInstances(nuSMV, module, kpType, kpInstance);
        }
    }
}