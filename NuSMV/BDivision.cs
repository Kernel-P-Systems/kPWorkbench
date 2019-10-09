using KpCore;
using NuSMV;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuSMV
{
    /// <summary>
    /// A Deprecated class. It generates child instance of modules which has division rules, it does not create separate
    /// module for each child instance.
    /// </summary>
    [Obsolete("This class is deprecated, BDivisionInstances class is used for division rule management.")]
    public class BDivision
    {
        private static Module module = null;

        public static void generateDivisionRules(MType type, Module paramModule)
        {
            if (paramModule != null)
                module = paramModule;
            //get all division rules
            List<KpCore.Rule> divisionRules = new List<KpCore.Rule>();
            ExecutionStrategy eS = type.ExecutionStrategy;
            while (eS != null)
            {
                foreach (var rule in eS.Rules)
                {
                    if (rule.Type == RuleType.MEMBRANE_DIVISION)
                    {
                        divisionRules.Add(rule);
                    }
                }
                eS = eS.Next;
            }
            if (divisionRules.Count > 0)
            {
                //or each parent instance, generate a permutation of rules.
                ParentInstance parentInstance = null;
                Instance smvInstance = module.Instance;
                if (smvInstance.DivisionType == DIVISIONTYPE.PARENT)
                {
                    parentInstance = (ParentInstance)smvInstance;
                    generateDivisionInstances(parentInstance, divisionRules);
                }
            }
        }

        private static void addChildInstance2Module(MType type, MInstance newInstance, ParentInstance parentInstance, ChildInstance childSmvInstance)
        {
            childSmvInstance.ParentInstance = parentInstance;
            parentInstance.ChildInstances.Add(childSmvInstance);

            addChildSMVInstance(module, parentInstance, childSmvInstance);

            addDivisionVariableToChildInstance(childSmvInstance);

            addStatusToChildInstance(parentInstance, childSmvInstance);
        }

        private static void addChildSMVInstance(Module module, Instance parentSMVInstance, ChildInstance childSmvInstance)
        {
            childSmvInstance.ConnectedTo = parentSMVInstance.ConnectedTo;
            //build cross relation
            foreach (var target in parentSMVInstance.ConnectedTo)
            {
                //if it is not added already, then add
                if (!target.ConnectedTo.Contains(childSmvInstance))
                    target.ConnectedTo.Add(childSmvInstance);
            }
            childSmvInstance.DivisionType = DIVISIONTYPE.CHILD;
            childSmvInstance.Module = parentSMVInstance.Module;
            childSmvInstance.Name = SMVUtil.generateAnInstanceName(childSmvInstance);
            foreach (var parameter in parentSMVInstance.Parameters)
            {
                if (parameter is ParameterVar)
                {
                    ParameterVar childParam = new ParameterVar();
                    ParameterVar parentParam = (ParameterVar)parameter;
                    // if status variable, set NONEXIST
                    if (parentParam.Behaviour == VariableBehaviour.CUSTOM)
                    {
                        if (parentParam.Name == CustomVariables.STATUS)
                        {
                            childParam.Behaviour = parentParam.Behaviour;
                            childParam.Name = parentParam.Name;
                            childParam.Init = StatusStates.NONEXIST;
                        }
                        else if (parentParam.Name == CustomVariables.TURN)
                        {
                            childParam.Behaviour = parentParam.Behaviour;
                            childParam.Name = parentParam.Name;
                            childParam.Init = TurnStates.READY;
                        }
                        else if (parameter.Name == CustomVariables.SYNCH)
                        {
                            childParam = parentParam;
                        }
                    }
                    else
                    //set remained parameters as zero
                    {
                        childParam.Behaviour = parentParam.Behaviour;
                        childParam.Name = parentParam.Name;
                        childParam.Init = "0";
                    }
                    childSmvInstance.Parameters.Add(childParam);
                }
                else
                {
                    //if there is something else just take as it is.
                    childSmvInstance.Parameters.Add(parameter);
                }
            }
            module.ChildInstances.Add(childSmvInstance);
        }

        private static void addDivisionVariableToChildInstance(ChildInstance childSmvInstance)
        {
            //copy division rules to child instance.
            foreach (var variable in module.Variables)
            {
                if (variable.Behaviour == VariableBehaviour.DIVISION)
                {
                    Variable divVar = new Variable(variable.Name);
                    divVar.Behaviour = VariableBehaviour.DIVISION;
                    divVar.Type = variable.Type;
                    if (variable is Variable)
                    {
                        foreach (var caseLine in (variable as Variable).Next.CaseStatement.CaseLines)
                        {
                            divVar.Next.CaseStatement.CaseLines.Add(caseLine);
                        }
                        childSmvInstance.DivisionVariables.Add(divVar);
                    }
                }
            }
        }

        /// <summary>
        /// Add the rule which defines the values of division variables. Remained rules will be added later in
        /// BRulesStandardVar.assignLastCasesToDivisionVars
        /// </summary>
        /// <param name="childSmvInstance"></param>
        /// <param name="variable"></param>
        /// <param name="totalValue"></param>
        private static void addRuleChildDivisionVariables(ChildInstance childSmvInstance, Variable variable, int totalValue)
        {
            CaseLine newCase = new CaseLine();

            string childActivation = childSmvInstance.Name + "." + childSmvInstance.DivisionStatus.Name;
            Expression left = new Expression();
            left.Exp = childActivation;
            Expression right = new Expression(StatusStates.NONEXIST);
            BoolExp condition1 = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);

            //next(child.status)=ACTIVE
            left = new Expression();
            left.Exp = SMVKeys.NEXT + "(" + childActivation + ")";
            right = new Expression(StatusStates.ACTIVE);
            BoolExp condition2 = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);

            CompoundBoolExpression compound = new CompoundBoolExpression(condition1, NuSMV.BinaryOperator.AND, condition2);
            newCase.Rule.Condition = compound;

            Expression result = new Expression();
            //inherit the parent's remained multiset objects
            //result.Exp = SMVKeys.NEXT + "(" + childSmvInstance.ParentInstance.Name + "." + variable.Name + ") + " + totalValue.ToString();
            //newCase.Rule.AppendBoolExpression(BRulesStandardVar.getBoundCondition(variable, result), BinaryOperator.AND);
            result.Exp = totalValue.ToString();

            newCase.Result = result;
            variable.Next.CaseStatement.CaseLines.Insert(0, newCase);

            ////((((_status = _willDISSOLVE) | (_status = _willDIVIDE))) & (_sync = _EXCH)) : 0;
            //CaseLine caseLine2 = new CaseLine();
            //ICondition willDissolve = null;
            //ICondition willDivide = null;
            //ICondition willDissolveOrDivide = null;

            //if (module.HasDissolutionRule)
            //{
            //    willDissolve = BRulesComVar.getInstancedStatus(module, StatusStates.WILLDISSOLVE);
            //}
            //if (module.HasDivisionRule)
            //{
            //    willDivide = BRulesComVar.getInstancedStatus(module, StatusStates.WILLDIVIDE);
            //}
            //willDissolveOrDivide = new CompoundBoolExpression(willDissolve, BinaryOperator.OR, willDivide);
            //// this is non-instanced _sync = _EXCH
            //BoolExp synchIsExch = new BoolExp();
            //synchIsExch.Left = new Expression(CustomVariables.SYNCH);
            //synchIsExch.RelationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
            //synchIsExch.Right = new Expression(SynchStates.EXCHANGE);

            //CompoundBoolExpression statusAndSync = new CompoundBoolExpression(willDissolveOrDivide, BinaryOperator.AND, synchIsExch);
            //caseLine2.Rule.Condition = statusAndSync;
            //caseLine2.Result = new Expression("0");
            //variable.Next.CaseStatement.CaseLines.Insert(1, caseLine2);

            ////((_status = _DISSOLVED) | (_status = _DIVIDED)) : 0;
            //CaseLine caseLine3 = new CaseLine();
            //ICondition dissolved = null;
            //ICondition divided = null;
            //ICondition dissolvedOrDivided= null;

            //if (module.HasDissolutionRule)
            //{
            //    dissolved = BRulesComVar.getInstancedStatus(module, StatusStates.DISSOLVED);
            //}
            //if (module.HasDivisionRule)
            //{
            //    divided = BRulesComVar.getInstancedStatus(module, StatusStates.DIVIDED);
            //}
            //dissolvedOrDivided = new CompoundBoolExpression(dissolved, BinaryOperator.OR, divided);
            //caseLine3.Rule.Condition = dissolvedOrDivided;
            //caseLine3.Result = new Expression("0");
            //variable.Next.CaseStatement.CaseLines.Insert(2, caseLine3);
        }

        private static void addStatusToChildInstance(ParentInstance parentInstance, ChildInstance childSmvInstance)
        {
            //copy status variable and add custom activation case.
            Variable status = module.Status;
            Variable childStatus = new Variable(status.Name);
            childStatus.Behaviour = VariableBehaviour.DIVISION;
            childStatus.Type = status.Type;
            foreach (var caseLine in status.Next.CaseStatement.CaseLines)
            {
                childStatus.Next.CaseStatement.CaseLines.Add(caseLine);
            }
            // add custom case (parent._status = willDivide) & (next( parent._status) = DIVIDED)
            CaseLine newCase = new CaseLine();
            string parentStatus = parentInstance.Name + "." + status.Name;
            Expression left = new Expression();
            left.Exp = parentStatus;
            Expression right = new Expression(StatusStates.WILLDIVIDE);//I changed here for willDivide
            BoolExp condition1 = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);
            left = new Expression();
            left.Exp = SMVKeys.NEXT + "( " + parentStatus + ")";
            right = new Expression(StatusStates.DIVIDED);
            BoolExp condition2 = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);
            CompoundBoolExpression compound = new CompoundBoolExpression(condition1, NuSMV.BinaryOperator.AND, condition2);
            Expression result = new Expression(StatusStates.ACTIVE);
            newCase.Rule.Condition = compound;
            newCase.Result = result;
            //insert at the beginning
            childStatus.Next.CaseStatement.CaseLines.Insert(0, newCase);

            childSmvInstance.DivisionStatus = childStatus;
        }

        private static void formDivisionInstance(ParentInstance parentInstance, List<KpCore.Rule> divisionRules, List<InstanceBlueprint> result)
        {
            MType type = result.ElementAt(0).Type;
            MInstance instance = new MInstance();
            ChildInstance childSmvInstance = new ChildInstance();
            addChildInstance2Module(type, instance, parentInstance, childSmvInstance);

            foreach (var variable in childSmvInstance.DivisionVariables)
            {
                int totalValue = getTotalAmountOfDivisionVar(variable, divisionRules, result);
                addRuleChildDivisionVariables(childSmvInstance, variable, totalValue);
            }
        }

        private static void generateDivisionInstances(ParentInstance parentInstance, List<KpCore.Rule> divisionRules)
        {
            List<InstanceBlueprint> result = new List<InstanceBlueprint>();
            getRulePermutation(parentInstance, divisionRules, 0, result);
        }

        private static void getRulePermutation(ParentInstance parentInstance, List<KpCore.Rule> divisionRules, int ruleCount, List<InstanceBlueprint> result)
        {
            if (ruleCount == divisionRules.Count)
            {
                formDivisionInstance(parentInstance, divisionRules, result);
                return;
            }
            DivisionRule divisionRule = (DivisionRule)divisionRules.ElementAt(ruleCount);
            int compartCount = divisionRule.Rhs.Count;
            ruleCount++;
            for (int j = 0; j < compartCount; j++)
            {
                InstanceBlueprint compartment = divisionRule.Rhs.ElementAt(j);
                int index = ruleCount - 1;
                if (result.Count < ruleCount)
                {
                    result.Insert(index, compartment);
                }
                else
                {
                    result.RemoveAt(index);
                    result.Insert(index, compartment);
                }
                getRulePermutation(parentInstance, divisionRules, ruleCount, result);
            }
        }
        private static int getTotalAmountOfDivisionVar(IVar variable, List<KpCore.Rule> divisionRules, List<InstanceBlueprint> result)
        {
            int totalAmount = 0;
            bool isFirstParent = true;
            string typeName = "";
            foreach (InstanceBlueprint item in result)
            {
                typeName = item.Type.Name;
                int childValue = 0;
                int leftValue = 0;
                foreach (var divisionRule in divisionRules)
                {
                    DivisionRule divRule = (DivisionRule)divisionRule;
                    if (divRule.Rhs.Contains(item))
                    {
                        Multiset leftOfRule = divRule.Lhs;
                        foreach (var leftObj in leftOfRule.Objects)
                        {
                            if (leftObj.Equals(variable.Name))
                            {
                                if (isFirstParent)
                                {
                                    totalAmount = leftOfRule[leftObj];
                                    isFirstParent = false;
                                }
                                leftValue = leftOfRule[leftObj];
                            }
                        }
                    }
                }
                Multiset ms = item.Multiset;
                foreach (var obj in ms.Objects)
                {
                    if (obj.Equals(variable.Name))
                    {
                        childValue = ms[obj];
                    }
                }

                totalAmount = totalAmount - leftValue + childValue;
            }
            //update max value.
            if (variable.Type is BoundInt)
            {
                (variable.Type as BoundInt).UpperBound = Math.Max((variable.Type as BoundInt).UpperBound, totalAmount);
            }

            return totalAmount;
        }
    }
}