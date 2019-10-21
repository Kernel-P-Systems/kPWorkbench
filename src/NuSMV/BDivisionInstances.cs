using KpCore;
using NuSMV;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuSMV
{
    internal class BDivisionInstances
    {
        private static Module module = null;
        private static MInstance kPInstance = null;

        //childOrderTracer, In recursive function detects which child should have the total number of division variables
        private static int childOrderTracer = 1;

        /// <summary>
        /// Determines how many child instances will eventually be generated after running all division rules. For each child
        /// instance, it generates KPChildInstance (sub class of MInstance) object and adds them as MInstance(regular
        /// instances) to MType. For each MInstance (including KPChildInstances) an SMV module will be generated.
        /// </summary>
        /// <param name="type">KP type (MType)</param>
        /// <returns>List of child instance of KPChildInstance type</returns>
        internal static List<KPChildInstance> generateKPChildInstances(MType type)
        {
            List<KPChildInstance> kpChildInstances = new List<KPChildInstance>();

            //After all divisions, how many child will be generated.
            int totalChildNumber = 1;
            ExecutionStrategy eS = type.ExecutionStrategy;
            while (eS != null)
            {
                foreach (var rule in eS.Rules)
                {
                    if (rule.Type == RuleType.MEMBRANE_DIVISION)
                    {
                        DivisionRule divisionRule = (DivisionRule)rule;
                        totalChildNumber *= divisionRule.Rhs.Count;
                    }
                }
                eS = eS.Next;
            }
            //for each parent instance, totalChildNumber, child instance will be generated
            foreach (MInstance kpInstance in type.Instances)
            {
                for (int childCount = 1; childCount <= totalChildNumber; childCount++)
                {
                    KPChildInstance kpChildInstance = new KPChildInstance(kpInstance);
                    kpChildInstance.Order = childCount;
                    kpChildInstance.Name = kpInstance.Name + SMVPreFix.CHILD + childCount;
                    kpChildInstances.Add(kpChildInstance);
                }
            }
            return kpChildInstances;
        }

        /// <summary>
        /// Generates the caselines for division variables. Assumes all division rules are executed simultaneously as well as
        /// child instances activated. Calculates the starting value of activation of division variables by getting
        /// permutation of all division rules.
        /// </summary>
        /// <param name="pType"></param>
        /// <param name="pKPInstance"></param>
        /// <param name="pModule"></param>
        internal static void generateDivisionRules(MType type, MInstance pKPInstance, Module pModule)
        {
            if (pKPInstance != null)
                kPInstance = pKPInstance;
            if (pModule != null)
                module = pModule;
            //Generate division rules of child instance
            if (kPInstance is KPChildInstance)
            {
                //restart the child order tracker from 1, it will recursively increase until becomes equal to kPInstance.Order
                childOrderTracer = 1;
                generateChildInstanceDivisionRules(type);
            }
        }

        private static void generateChildInstanceDivisionRules(MType type)
        {
            //List all existing division rules in all strategies.
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
                //perform calculation over list of division rules.
                List<InstanceBlueprint> result = new List<InstanceBlueprint>();
                getRulePermutation(divisionRules, 0, result);
            }
        }

        /// <summary>
        /// It recursively buffers the InstanceBlueprint division rules (in result list) for all child instances, in order to
        /// get the result of only relevant child, we use childOrderTracer which is kind of the ID of child instance, if the
        /// recursion arrives this child then performs calculations over buffered @param result.
        /// </summary>
        /// <param name="divisionRules"></param>
        /// <param name="ruleCount"></param>
        /// <param name="result">Holds list of InstanceBlueprint of division rules</param>
        private static void getRulePermutation(List<KpCore.Rule> divisionRules, int ruleCount, List<InstanceBlueprint> result)
        {
            //if it completes on cycle of recursion
            if (ruleCount == divisionRules.Count)
            {
                //check if this cycle belongs to requested child instance
                if (childOrderTracer == (kPInstance as KPChildInstance).Order)
                {
                    //then perform calculation
                    calculateValuesOfDivisionVariables(divisionRules, result);
                }
                childOrderTracer++;
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
                getRulePermutation(divisionRules, ruleCount, result);
            }
        }

        private static void calculateValuesOfDivisionVariables(List<KpCore.Rule> divisionRules, List<InstanceBlueprint> result)
        {
            //Get the instance of the module
            ChildInstance childSMVInstance = (module.Instance as ChildInstance);
            updateStatusAndAddDivisionVars2ChildModule(childSMVInstance);

            foreach (var variable in childSMVInstance.DivisionVariables)
            {
                int totalValue = getTotalAmountOfDivisionVar(variable, divisionRules, result);
                addRuleChildDivisionVariables(childSMVInstance, variable, totalValue);
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
            result.Exp = totalValue.ToString();

            newCase.Result = result;
            variable.Next.CaseStatement.CaseLines.Insert(0, newCase);
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

        /// <summary>
        /// Update status variable and For each variable inside InstanceBlueprint, create a new variable so called division
        /// variable and add them to division variable list of child instance, i.e., DivisionVariables
        /// </summary>
        /// <param name="childSMVInstance"></param>
        private static void updateStatusAndAddDivisionVars2ChildModule(ChildInstance childSMVInstance)
        {
            updateChildModuleStatus(childSMVInstance);
            addDivisionVariable2ChildModule(childSMVInstance);
        }

        /// <summary> Insert the caseline at the beginning of child status' next, If parent divides then child will activate
        /// case (parent._status = willDivide) & (next( parent._status) = DIVIDED) : _ACTIVE; </summary> <param
        /// name="childSMVInstance"></param>
        private static void updateChildModuleStatus(ChildInstance childSMVInstance)
        {
            //copy status variable and add custom activation case.
            Variable status = module.Status;
            status.Behaviour = VariableBehaviour.DIVISION;

            // add custom case (parent._status = willDivide) & (next( parent._status) = DIVIDED)
            CaseLine newCase = new CaseLine();
            string parentStatus = childSMVInstance.ParentInstance.Name + "." + status.Name;
            Expression left = new Expression();
            left.Exp = parentStatus;
            Expression right = new Expression(StatusStates.WILLDIVIDE);
            BoolExp condition1 = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);
            left = new Expression();
            left.Exp = SMVKeys.NEXT + "( " + parentStatus + ")";
            right = new Expression(StatusStates.DIVIDED);
            BoolExp condition2 = new BoolExp(left, NuSMV.RelationalOperator.EQUAL, right);
            CompoundBoolExpression compound = new CompoundBoolExpression(condition1, BinaryOperator.AND, condition2);
            Expression result = new Expression(StatusStates.ACTIVE);
            newCase.Rule.Condition = compound;
            newCase.Result = result;
            //insert at the beginning
            status.Next.CaseStatement.CaseLines.Insert(0, newCase);

            childSMVInstance.DivisionStatus = status;
        }

        /// <summary>
        /// Creates an extra variable for each division rule.
        /// </summary>
        /// <param name="childSmvInstance"></param>
        private static void addDivisionVariable2ChildModule(ChildInstance childSmvInstance)
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
    }
}