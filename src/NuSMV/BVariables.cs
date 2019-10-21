using KpCore;
using NuSMV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace NuSMV
{
    public class BVariables
    {
        private static bool manageBoundsByUser = true;

        public static void buildVariables(KPsystem kpSystem, SMVModel nuSMV, NuSMV.Module module, KpCore.MType kpType)
        {
            // variables comes from KP model.
            buildStandardVariables(nuSMV, module, kpSystem, kpType);
        }


        #region build standard variables and their next values

        /// <summary>
        /// Build variables comes from KP model.
        /// </summary>
        /// <param name="nuSMV"></param>
        /// <param name="module"></param>
        /// <param name="kpType"></param>
        public static void buildStandardVariables(SMVModel nuSMV, NuSMV.Module module, KPsystem kpSystem, KpCore.MType kpType)
        {
            ExecutionStrategy eS = kpType.ExecutionStrategy;
            int strategyIndex = 0;
            //First get variables from current type
            while (eS != null)
            {
                foreach (var rule in eS.Rules)
                {
                    //check if it is has guards, then create its variables.
                    if (rule.IsGuarded)
                    {
                        buildGuardVariable(kpSystem, kpType, module, strategyIndex, rule);
                    }

                    if (rule.Type == RuleType.MULTISET_REWRITING)
                    {
                        buildReWritingVariables(kpSystem, kpType, module, strategyIndex, rule);
                    }
                    else if (rule.Type == RuleType.REWRITE_COMMUNICATION)
                    {
                        buildCommunicationVariables(nuSMV, module, kpSystem, kpType, strategyIndex, rule);
                    }
                    else if (rule.Type == RuleType.MEMBRANE_DIVISION)
                    {
                        buildDivisionVariables(module, kpSystem, kpType, strategyIndex, rule);
                    }
                    else if (rule.Type == RuleType.MEMBRANE_DISSOLUTION)
                    {
                        buildDissolutionVariables(kpSystem, kpType, module, strategyIndex, rule);
                    }
                }
                strategyIndex++;
                eS = eS.Next;
            }
        }

        private static void buildGuardVariable(KPsystem kpSystem, MType kpType, Module module, int strategyIndex, KpCore.Rule rule)
        {
            extractVarsFromGuards(kpSystem, kpType, module, rule.Guard);
        }

        private static void extractVarsFromGuards(KPsystem kpSystem, MType kpType, Module module, IGuard guard)
        {
            if (guard is BasicGuard)
            {
                BasicGuard basicGuard = (BasicGuard)guard;
                extractVarFromBasicGuards(kpSystem, kpType, module, basicGuard);
            }
            else if (guard is NegatedGuard)
            {
                NegatedGuard negatedGuard = (NegatedGuard)guard;
                Console.Error.WriteLine("This part has not implemented yet.");
                // ICondition negatedBooleanExpression = getNegatedGuard(negatedGuard);
            }
            else if (guard is CompoundGuard)
            {
                CompoundGuard compoundGuard = (CompoundGuard)guard;
                extractVarsFromGuards(kpSystem, kpType, module, compoundGuard.Lhs);
                extractVarsFromGuards(kpSystem, kpType, module, compoundGuard.Rhs);
            }
        }

        private static void extractVarFromBasicGuards(KPsystem kpSystem, MType kpType, Module module, BasicGuard basicGuard)
        {
            Multiset ms = basicGuard.Multiset;
            NuSMV.RelationalOperator oper = SMVUtil.getRelationalOperator(basicGuard.Operator);
            Variable variable = null;
            foreach (var varName in ms.Objects)
            {
                if (!module.isVariableExist(varName))
                {
                    variable = new Variable(varName);
                    int upperBound = ms[varName];
                    //if the guard requires a number greater, or greaterEqual, then it upperbound should be at least one number greater then,
                    //the condition
                    if (oper.Operator == NuSMV.RelationalOperator.GEQ || oper.Operator == NuSMV.RelationalOperator.GT)
                    {
                        upperBound = upperBound + 1;
                    }
                    variable.Type = new BoundInt(0, upperBound);
                    setBoundIntType(kpSystem, kpType, module, variable);
                    variable.Behaviour = VariableBehaviour.REWRITING;
                    variable.Init = setOrUpdateInit(module, variable);
                    module.Variables.Add(variable);
                }
            }
        }

        private static void buildReWritingVariables(KPsystem kpSystem, KpCore.MType kpType, NuSMV.Module module, int strategyIndex, KpCore.Rule kpRule)
        {
            RewritingRule rwr = (RewritingRule)kpRule;
            string varName = "";
            VariableOrigin origin = VariableOrigin.Original;
            bool isLeft = true;
            foreach (var leftHRule in rwr.Lhs)
            {
                varName = leftHRule.Key;
                origin = VariableOrigin.Original;
                isLeft = true;
                buildReWritingVariable(kpSystem, kpType, module, strategyIndex, kpRule, varName, origin, isLeft);
            }
            foreach (var rigthHRule in rwr.Rhs)
            {
                varName = rigthHRule.Key;
                origin = VariableOrigin.Original;
                isLeft = false;
                //first generate original one, then its copy
                if (!module.isVariableExist(varName))
                {
                    buildReWritingVariable(kpSystem, kpType, module, strategyIndex, kpRule, varName, origin, isLeft);
                }
                string copyVarName = varName + SMVPreFix.COPY;
                origin = VariableOrigin.Copy;
                buildReWritingVariable(kpSystem, kpType, module, strategyIndex, kpRule, copyVarName, origin, isLeft);
            }
        }

        private static void buildReWritingVariable(KPsystem kpSystem, KpCore.MType kpType, NuSMV.Module module, int strategyIndex, KpCore.Rule kpRule, string newVarName, VariableOrigin origin, bool isLeft)
        {
            Variable newVar = null;
            //if variable does not exist then create it,
            if (!module.isVariableExist(newVarName))
            {
                if (origin == VariableOrigin.Original)
                {
                    newVar = new Variable(newVarName);
                    setBoundIntType(kpSystem, kpType, module, newVar);
                    newVar.Behaviour = VariableBehaviour.REWRITING;
                    newVar.Init = setOrUpdateInit(module, newVar);
                    newVar.Origin = VariableOrigin.Original;
                    if (isLeft)
                    {
                        //if it is on left then add it, but do not add rules to first not-copy variable on right.
                        BRulesStandardVar.addCaseLineToStandardVariable(newVar, kpRule, module, strategyIndex);
                    }
                }
                else if (origin == VariableOrigin.Copy)
                {
                    newVar = new Variable(newVarName);
                    Variable orginalVariable = (Variable)module.getVariable(newVarName.Replace(SMVPreFix.COPY, ""));
                    newVar.Type = orginalVariable.Type;
                    newVar.Behaviour = VariableBehaviour.REWRITING;
                    newVar.Init = "0";
                    newVar.Origin = VariableOrigin.Copy;
                    //add result of rule to caseline
                    BRulesStandardVar.addCaseLineToStandardVariable(newVar, kpRule, module, strategyIndex);
                }
                if (newVar != null)
                {
                    module.Variables.Add(newVar);
                }
                else
                {
                    throw new Exception("Cannot create variable : " + newVarName);
                }
            }
            else
            {
                //bring variable to add new rules.
                newVar = (Variable)module.Variables.First(item => item.Name.Equals(newVarName));
                BRulesStandardVar.addCaseLineToStandardVariable(newVar, kpRule, module, strategyIndex);
            }
        }

        private static void buildCommunicationVariables(SMVModel nuSMV, NuSMV.Module module, KPsystem kpSystem, KpCore.MType type, int strategyIndex, KpCore.Rule rule)
        {
            RewriteCommunicationRule rcr = (RewriteCommunicationRule)rule;
            string varName = "";
            VariableOrigin origin = VariableOrigin.Original;
            bool isLeft = true;
            //regular left hand-side rules
            foreach (var leftHRule in rcr.Lhs)
            {
                varName = leftHRule.Key;
                isLeft = true;
                buildReWritingVariable(kpSystem, type, module, strategyIndex, rule, varName, origin, isLeft);
            }
            //regular right hand-side rules
            foreach (var rigthHRule in rcr.Rhs)
            {
                varName = rigthHRule.Key;
                origin = VariableOrigin.Original;
                isLeft = false;
                //first generate original one, then its copy
                if (!module.isVariableExist(varName))
                {
                    buildReWritingVariable(kpSystem, type, module, strategyIndex, rule, varName, origin, isLeft);
                }
                string copyVarName = varName + SMVPreFix.COPY;
                origin = VariableOrigin.Copy;
                buildReWritingVariable(kpSystem, type, module, strategyIndex, rule, copyVarName, origin, isLeft);
            }
            //Targeted rules
            foreach (var target in rcr.TargetRhs.Values)
            {
                TargetedMultiset targetMultiSet = (TargetedMultiset)target;
                InstanceIdentifier targetTypeIdentifier = (InstanceIdentifier)targetMultiSet.Target;
                MType targetType = null;
                foreach (var tempType in kpSystem.Types)
                {
                    if (tempType.Name == targetTypeIdentifier.Value)
                        targetType = tempType;
                }
                //for each connected instance of the target type, create a copy variable foreach object in the multiset
                foreach (var connectedInstance in module.Instance.ConnectedTo)
                {
                    if (connectedInstance.Module.Type == targetType.Name)
                    {
                        Module targetModule = connectedInstance.Module;
                        Multiset ms = targetMultiSet.Multiset;
                        foreach (var obj in ms.Objects)
                        {
                            varName = obj;
                            Variable targetVariable = new Variable(varName);

                            string currentCpVarName = SMVPreFix.getConnectedCopyCommVarName(varName, targetModule);
                            Variable currentCpVar = new Variable(currentCpVarName);

                            //create original variable inside target module
                            if (!targetModule.isVariableExist(varName))
                            {
                                setBoundIntType(kpSystem, targetType, targetModule, targetVariable);
                                targetVariable.Behaviour = VariableBehaviour.COMMUNICATION;
                                targetVariable.Origin = VariableOrigin.OriginalCommVar;
                                targetVariable.Init = setOrUpdateInit(targetModule, targetVariable);
                                targetModule.Variables.Add(targetVariable);
                            }
                            else
                            {
                                //if variable is already in target module, then make sure, it is set as communication var.
                                targetVariable = (Variable)targetModule.Variables.First(item => item.Name.Equals(varName));
                                targetVariable.Behaviour = VariableBehaviour.COMMUNICATION;
                                targetVariable.Origin = VariableOrigin.OriginalCommVar;
                                targetVariable.Init = setOrUpdateInit(targetModule, targetVariable);
                            }
                            //create a varName_InstanceName_TargetModule, variable (as copy) inside current module.
                            if (!module.isVariableExist(currentCpVarName))
                            {
                                Variable orginalVariable = (Variable)targetModule.getVariable(varName);
                                currentCpVar.Type = orginalVariable.Type;
                                currentCpVar.Behaviour = VariableBehaviour.REWRITING;
                                currentCpVar.Origin = VariableOrigin.CopyOfCommVar;
                                currentCpVar.Init = "0";
                                module.Variables.Add(currentCpVar);
                            }
                            else
                            {
                                //if variable exists then update the values.
                                currentCpVar = (Variable)module.Variables.First(item => item.Name.Equals(currentCpVarName));
                            }
                            //add result of rule to caseline
                            BRulesComVar.addCaseLineToCurrentCopyCommVar(targetVariable, currentCpVar, rule, module, targetModule, strategyIndex);
                        }
                    }
                }
            }
        }

        private static void buildDivisionVariables(NuSMV.Module module, KPsystem kpSystem, KpCore.MType type, int strategyIndex, KpCore.Rule rule)
        {
            DivisionRule divisionRule = (DivisionRule)rule;
            foreach (var leftHRule in divisionRule.Lhs)
            {
                Variable variable = new Variable(leftHRule.Key);
                if (!module.Variables.Contains(variable))
                {
                    variable.Type = new BoundInt(0, setMax(kpSystem, type, module, variable));
                    variable.Behaviour = VariableBehaviour.REWRITING;
                    variable.Init = setOrUpdateInit(module, variable);
                    module.Variables.Add(variable);
                }
                else
                {
                    //if variable exists then update the upperbound value.
                    variable = (Variable)module.Variables.First(item => item.Name.Equals(leftHRule.Key));
                }
                //add result of rule to caseline
                BRulesStandardVar.addCaseLineToStandardVariable(variable, rule, module, strategyIndex);
            }

            foreach (InstanceBlueprint compartment in divisionRule.Rhs)
            {
                MType compType = compartment.Type;
                if (type.Name.Equals(compType.Name))
                {
                    Multiset ms = compartment.Multiset;
                    foreach (var obj in ms.Objects)
                    {
                        Variable variable = new Variable(obj);
                        if (!module.Variables.Contains(variable))
                        {
                            variable.Type = new BoundInt(0, setMax(kpSystem, compType, module, variable));
                            variable.Behaviour = VariableBehaviour.DIVISION;
                            variable.Init = setOrUpdateInit(module, variable);
                            module.Variables.Add(variable);
                        }
                        else
                        {
                            variable = (Variable)module.Variables.First(item => item.Name.Equals(obj));
                            variable.Behaviour = VariableBehaviour.DIVISION;
                        }
                    }
                }
            }
            // add rule to status variable
            BRulesCustomVar.addRuleToStatusVariable(rule, module, strategyIndex);
        }

        private static void buildDissolutionVariables(KPsystem kpSystem, KpCore.MType type, NuSMV.Module module, int strategyIndex, KpCore.Rule rule)
        {
            //Preserve variable value and update status value.
            DissolutionRule dissolutionRule = (DissolutionRule)rule;
            foreach (var leftHRule in dissolutionRule.Lhs)
            {
                Variable variable = new Variable(leftHRule.Key);
                if (!module.Variables.Contains(variable))
                {
                    variable.Type = new BoundInt(0, setMax(kpSystem, type, module, variable));
                    variable.Behaviour = VariableBehaviour.REWRITING;
                    variable.Init = setOrUpdateInit(module, variable);
                    module.Variables.Add(variable);
                }
                else
                {
                    //if variable exists then update the upperbound value.
                    variable = (Variable)module.Variables.First(item => item.Name.Equals(leftHRule.Key));
                }
                //add result of rule to caseline
                BRulesStandardVar.addCaseLineToStandardVariable(variable, rule, module, strategyIndex);
            }
            // add rule to status variable
            BRulesCustomVar.addRuleToStatusVariable(rule, module, strategyIndex);
        }

        private static void setBoundIntType(KPsystem kpSystem, MType type, Module module, Variable variable)
        {
            bool setByXML = false;
            if (manageBoundsByUser)
            {
                if (isBoundXMLFileExist(module.Type))
                {
                    setByXML = readBounds(variable as Variable);
                }
            }
            if (!(manageBoundsByUser && setByXML))
            {
                if (variable.Type == null)
                {
                    BoundInt boundInt = new BoundInt();
                    int lowerBound = 0;
                    int upperBound = setMax(kpSystem, type, module, variable);
                    variable.Type = new BoundInt(lowerBound, upperBound);
                }
                else
                {
                    int upperBound = setMax(kpSystem, type, module, variable);
                    if (upperBound > (variable.Type as BoundInt).UpperBound)
                    {
                        (variable.Type as BoundInt).UpperBound = upperBound;
                    }
                }
            }
        }

        /// <summary>
        /// if variable has NOT been set by XML file then set max automatically
        /// </summary>
        /// <param name="kpSystem"></param>
        /// <param name="myType"></param>
        /// <param name="module"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        private static int setMax(KPsystem kpSystem, MType myType, NuSMV.Module module, IVar variable)
        {
            int maxVal = 0;
            foreach (var type in kpSystem.Types)
            {
                ExecutionStrategy eS = type.ExecutionStrategy;
                while (eS != null)
                {
                    //Inside this type
                    if (myType.Name.Equals(type.Name))
                    {
                        foreach (var rule in eS.Rules)
                        {
                            //Normal variable
                            if (rule.Type == RuleType.MULTISET_REWRITING)
                            {
                                RewritingRule rwr = (RewritingRule)rule;
                                foreach (var leftHRule in rwr.Lhs)
                                {
                                    if (variable.Name.Equals(leftHRule.Key))
                                    {
                                        maxVal = Math.Max(maxVal, leftHRule.Value);
                                    }
                                }
                                foreach (var rigthHRule in rwr.Rhs)
                                {
                                    if (variable.Name.Equals(rigthHRule.Key))
                                    {
                                        maxVal = Math.Max(maxVal, rigthHRule.Value);
                                    }
                                }
                            }
                            // Communication Var
                            else if (rule.Type == RuleType.REWRITE_COMMUNICATION)
                            {
                                RewriteCommunicationRule rcr = (RewriteCommunicationRule)rule;
                                foreach (var leftHRule in rcr.Lhs)
                                {
                                    if (variable.Name.Equals(leftHRule.Key))
                                    {
                                        maxVal = Math.Max(maxVal, leftHRule.Value);
                                    }
                                }
                            }
                            else if (rule.Type == RuleType.MEMBRANE_DIVISION)
                            {
                                DivisionRule divisionRule = (DivisionRule)rule;
                                foreach (var leftHRule in divisionRule.Lhs)
                                {
                                    if (variable.Name.Equals(leftHRule.Key))
                                    {
                                        maxVal = Math.Max(maxVal, leftHRule.Value);
                                    }
                                }
                                foreach (InstanceBlueprint compartment in divisionRule.Rhs)
                                {
                                    MType compType = compartment.Type;
                                    if (myType.Name.Equals(compType.Name))
                                    {
                                        Multiset ms = compartment.Multiset;
                                        foreach (var obj in ms.Objects)
                                        {
                                            if (variable.Name.Equals(obj))
                                            {
                                                maxVal = Math.Max(maxVal, ms[obj]);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (rule.Type == RuleType.MEMBRANE_DISSOLUTION)
                            {
                                DissolutionRule dissolutionRule = (DissolutionRule)rule;
                                foreach (var leftHRule in dissolutionRule.Lhs)
                                {
                                    if (variable.Name.Equals(leftHRule.Key))
                                    {
                                        maxVal = Math.Max(maxVal, leftHRule.Value);
                                    }
                                }
                            }
                        }
                    }
                    //Inside communication rule of other types
                    else
                    {
                        foreach (var rule in eS.Rules)
                        {
                            if (rule.Type == RuleType.REWRITE_COMMUNICATION)
                            {
                                RewriteCommunicationRule rcr = (RewriteCommunicationRule)rule;
                                foreach (var target in rcr.TargetRhs.Values)
                                {
                                    TargetedMultiset tg = (TargetedMultiset)target;
                                    InstanceIdentifier identifier = (InstanceIdentifier)tg.Target;
                                    if (myType.Name.Equals(identifier.Value))
                                    {
                                        Multiset ms = tg.Multiset;
                                        foreach (var rightVar in ms.Objects)
                                        {
                                            if (variable.Name.Equals(rightVar))
                                            {
                                                maxVal = Math.Max(maxVal, ms[rightVar]);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (rule.Type == RuleType.MEMBRANE_DIVISION)
                            {
                                DivisionRule divisionRule = (DivisionRule)rule;
                                foreach (InstanceBlueprint compartment in divisionRule.Rhs)
                                {
                                    MType compType = compartment.Type;
                                    if (myType.Name.Equals(compType.Name))
                                    {
                                        Multiset ms = compartment.Multiset;
                                        foreach (var obj in ms.Objects)
                                        {
                                            if (variable.Name.Equals(obj))
                                            {
                                                maxVal = Math.Max(maxVal, ms[obj]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    eS = eS.Next;
                }
                //also compare parameter values
                foreach (var instance in type.Instances)
                {
                    foreach (var param in instance.Multiset.Objects)
                    {
                        if (variable.Name.Equals(param))
                        {
                            maxVal = Math.Max(maxVal, instance.Multiset[param]);
                        }
                    }
                }
            }
            return maxVal;
        }

        /// <summary>
        /// if variable parametrized then returns the init s with parameter,
        /// otherwise init s with given default value.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="variable"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string setOrUpdateInit(Module module, IVar variable, string defaultValue)
        {
            string initialValue = "";
            //is it initialized with parameter
            foreach (var parameter in module.Parameters)
            {
                if (variable.Name == parameter.Name)
                {
                    initialValue = parameter.ParamName;
                    break;
                }
            }
            //if it is not parametrized.
            if (String.IsNullOrWhiteSpace(initialValue))
            {
                initialValue = defaultValue;
            }
            return initialValue;
        }

        /// <summary>
        /// if variable parametrized then returns the init s with parameter,
        /// otherwise init s with given default value will be 0.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public static string setOrUpdateInit(Module module, IVar variable)
        {
            string initialValue = "0";
            //is it initialized with parameter
            foreach (var parameter in module.Parameters)
            {
                if (variable.Name == parameter.Name)
                {
                    initialValue = parameter.ParamName;
                    break;
                }
            }
            return initialValue;
        }

        #endregion build standard variables and their next values


/*****************************************************************************/

        #region generate custom variables

        //variables required for NuSMV model
        public static void generateCustomVariables(KPsystem kpSystem, MType kpType, Module module)
        {
            if (module.HasDivisionRule || module.HasDissolutionRule)
            {
                Variable status = generateStatusVariable(module, kpType);
                if (status != null)
                    module.Status = status;
            }
            Variable turn = generateTurnVariable(module, kpType);
            if (turn != null)
                module.Turn = turn;
            if (module.HasConnection)
            {
                List<NoNextVar> connections = generateConnectionVariable(kpSystem, kpType, module);
                if (connections != null)
                    module.Connections = connections;
            }
        }

        private static List<NoNextVar> generateConnectionVariable(KPsystem kpSystem, MType kpType, Module module)
        {
            List<NoNextVar> connections = new List<NoNextVar>();
            foreach (MType mType in kpSystem.Types)
            {
                NoNextVar connection = new NoNextVar(SMVPreFix.getConnectionVar(mType));
                connection.Behaviour = VariableBehaviour.CUSTOM;
                SEnum connEnums = new SEnum();
                HashSet<Instance> connectedTo = new HashSet<Instance>();
                foreach (var connectedInstance in module.Instance.ConnectedTo)
                {
                    // if it has connection
                    if (connectedInstance.Module.Type == mType.Name)
                    {
                        bool communicationRuleToTargetExist = communicationRuleIncludesTargetType(kpType, mType);
                        if (communicationRuleToTargetExist)
                            connectedTo.Add(connectedInstance);
                    }
                }
                //if there is more than one connection of same type compartments exists, then add it to connections
                if (connectedTo.Count > 1)
                {
                    //sort them
                    IEnumerable<Instance> orderedConns = connectedTo.OrderBy(instance => instance.Name);

                    foreach (var connectedInstance in orderedConns)
                    {
                        connEnums.Values.Add(SMVPreFix.getConnectedTo(connectedInstance));
                    }
                    connection.Type = connEnums;
                    // connection.Init = setOrUpdateInit(module, connection); Commented out, since no need to get from parameter.
                    connections.Add(connection);
                }
            }
            return connections;
        }

        /// <summary>
        /// if current type, has communication rules which sends to target type
        /// </summary>
        /// <param name="currentType">current KP type</param>
        /// <param name="targetType">target KP Type</param>
        /// <returns></returns>
        private static bool communicationRuleIncludesTargetType(MType currentType, MType targetType)
        {
            bool communicationRuleToTargetExist = false;
            ExecutionStrategy kpEs = currentType.ExecutionStrategy;
            while (kpEs != null)
            {
                foreach (var rule in kpEs.Rules)
                {
                    if (rule.Type == RuleType.REWRITE_COMMUNICATION)
                    {
                        RewriteCommunicationRule rcr = (RewriteCommunicationRule)rule;
                        foreach (var target in rcr.TargetRhs.Values)
                        {
                            TargetedMultiset targetMultiSet = (TargetedMultiset)target;
                            InstanceIdentifier targetTypeIdent = (InstanceIdentifier)targetMultiSet.Target;
                            if (targetType.Name == targetTypeIdent.Value)
                            {
                                communicationRuleToTargetExist = true;
                                break;
                            }
                        }
                    }
                    if (communicationRuleToTargetExist)
                        break;
                }

                if (communicationRuleToTargetExist)
                    break;
                kpEs = kpEs.Next;
            }
            return communicationRuleToTargetExist;
        }

        public static Variable generateStatusVariable(Module module, MType kpType)
        {
            Variable status = new Variable(CustomVariables.STATUS);
            status.Behaviour = VariableBehaviour.CUSTOM;
            SEnum statusStates = new SEnum();
            statusStates.Values.Add(StatusStates.ACTIVE);
            if (module.HasDivisionRule)
            {
                statusStates.Values.Add(StatusStates.NONEXIST);
                statusStates.Values.Add(StatusStates.WILLDIVIDE);
                statusStates.Values.Add(StatusStates.DIVIDED);
            }
            if (module.HasDissolutionRule)
            {
                statusStates.Values.Add(StatusStates.WILLDISSOLVE);
                statusStates.Values.Add(StatusStates.DISSOLVED);
            }
            status.Type = statusStates;
            status.Init = setOrUpdateInit(module, status, StatusStates.ACTIVE);
            return status;
        }

        private static Variable generateTurnVariable(Module module, MType type)
        {
            Variable turn = new Variable(CustomVariables.TURN);
            turn.Behaviour = VariableBehaviour.CUSTOM;
            SEnum turnStates = new SEnum();
            int count = 0;
            foreach (var turnState in module.TurnOrder)
            {
                turnStates.Values.Add(turnState + count);
                count++;
            }
            turnStates.Values.Add(TurnStates.READY);
            turn.Type = turnStates;
            //turn.Init = setOrUpdateInit(module, turn, turnStates.Values.First());//To enable initial state, we need to start in READY state
            turn.Init = setOrUpdateInit(module, turn, TurnStates.READY);
            // turn.Next  =  getTurnNext(module) the next value of turn must be set after execution strategies are build, we need
            //number of rules inside blocks.
            return turn;
        }

        #endregion generate custom variables

/****************************************************************************/

        #region read/write bounds of variables to XML files

        internal static void writeBounds2XML(Module module)
        {
            if (!isBoundXMLFileExist(module.Type))
            {
                writeBoundValues(module);
            }
        }

        private static string boundXMLFile = "";

        public static string BoundXMLFile
        {
            get { return boundXMLFile; }
            set
            {
                boundXMLFile = value + ".xml";
            }
        }

        private static bool isBoundXMLFileExist(string fileName)
        {
            BoundXMLFile = fileName;
            if (File.Exists(BoundXMLFile))
            {
                return true;
            }
            return false;
        }

        private static bool readBounds(Variable variable)
        {
            bool found = false;
            using (XmlReader reader = XmlReader.Create(BoundXMLFile))
            {
                while (reader.Read())
                {
                    if (!found)
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "Name":
                                    // Next read will contain text.
                                    if (reader.Read())
                                    {
                                        string varName = reader.Value.Trim();
                                        if (varName == variable.Name)
                                        {
                                            if (reader.Read())
                                            {
                                                if (reader.Read())
                                                {
                                                    if (reader.Read())
                                                    {
                                                        if (reader.Read())
                                                        {
                                                            int lowerBound = 0;

                                                            if (Int32.TryParse(reader.Value.Trim(), out lowerBound))
                                                            {
                                                                if (variable.Type == null)
                                                                {
                                                                    variable.Type = new BoundInt();
                                                                }
                                                                (variable.Type as BoundInt).LowerBound = lowerBound;
                                                            }
                                                            else
                                                            {
                                                                throw new Exception("Can not read lower bound of " + variable + "." + reader.Value.Trim() + " must be a numeric value.");
                                                            }
                                                        }
                                                    }
                                                    if (reader.Read())
                                                    {
                                                        if (reader.Read())
                                                        {
                                                            if (reader.Read())
                                                            {
                                                                if (reader.Read())
                                                                {
                                                                    int upperBound = 0;
                                                                    if (variable.Type == null)
                                                                    {
                                                                        variable.Type = new BoundInt();
                                                                    }
                                                                    if (Int32.TryParse(reader.Value.Trim(), out upperBound))
                                                                    {
                                                                        (variable.Type as BoundInt).UpperBound = upperBound;
                                                                        found = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        throw new Exception("Can not read upper bound of " + variable + "." + reader.Value.Trim() + " must be a numeric value.");
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    else
                        break;
                }
            }
            return found;
        }

        private static void writeBoundValues(Module module)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true
            };
            using (XmlWriter writer = XmlWriter.Create(BoundXMLFile, xmlWriterSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement(module.Type);

                foreach (Variable variable in module.Variables)
                {
                    writer.WriteStartElement("Variable");

                    writer.WriteElementString("Name", variable.Name);
                    writer.WriteElementString("LowerBound", (variable.Type as BoundInt).LowerBound.ToString());
                    writer.WriteElementString("UpperBound", (variable.Type as BoundInt).UpperBound.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        #endregion max values of standard variable with xml
    }
}