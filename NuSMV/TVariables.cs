using NuSMV;
using System.Reflection;

namespace NuSMV
{
    /// <summary>
    /// Translating SMV variables to output.
    /// </summary>
    public class TVariables
    {
        internal static string VariableDefinition(IVar variable)
        {
            if (variable != null)
                return variable.Name + " : " + variable.Type + ";\n";
            else
                return null;
        }

        internal static string InitVariable(IVar variable)
        {
            if (variable is NoNextVar)
            {
                return variable.Name + " := " + (variable as NoNextVar).Init + ";\n";
            }
            else if (variable is Variable)
            {
                return "init (" + variable.Name + ") := " + (variable as Variable).Init + ";\n";
            }
            else if (variable is NonDeterVar)
            {
                return variable.Name + " := " + (variable as NonDeterVar).NonDetCase + ";\n";
            }
            else
            {
                return "Couldn't return init for " + variable.Name + " at " + MethodBase.GetCurrentMethod().Name + ";\n"; ;
            }
        }

        internal static string VariableNext(IVar variable)
        {
            if (variable is NoNextVar)
            {
                return "";
            }
            else if (variable is Variable)
            {
                if (variable.Name == CustomVariables.SYNCH)
                {
                    return "next (" + variable.Name + ") := " + (variable as Variable).Next.CaseStatement.ToString().Replace("&", "\n\t&") + ";\n";
                }
                return "next (" + variable.Name + ") := " + (variable as Variable).Next + ";\n";
            }
            else if (variable is NonDeterVar)
            {
                return "";
            }
            else
            {
                return "Couldn't return init for " + variable.Name + " at " + MethodBase.GetCurrentMethod().Name + ";\n"; ;
            }
        }

        internal static string Instances(Instance instance)
        {
            string op = "\t" + instance.Name + " : " + instance.Module.Name;
            int count = 1;
            int paramCount = instance.Parameters.Count;
            if (paramCount > 0)
                op += "(";
            foreach (var param in instance.Parameters)
            {
                op += (param as ParameterVar).Init;
                if (count != instance.Parameters.Count)
                    op += ",";
                count++;
            }
            if (paramCount > 0)
                op += ")";
            op += ";\n";
            return op;
        }

        internal static string CommunicationVariableNext(NuSMV.Module module, Instance instance, IVar var)
        {
            if (var is Variable)
            {
                string op = "case\n";
                Variable variable = (Variable)var;
                Next next = variable.Next;

                foreach (var caseLine in next.CaseStatement.CaseLines)
                {
                    if ((caseLine.Rule is CommunicationRule))
                    {
                        op += caseLine.ToString().Replace(SMVKeys.SELF, instance.Name);
                    }
                }
                foreach (var caseLine in next.CaseStatement.CaseLines)
                {
                    // CaseLine newcaseLine = new CaseLine();
                    if (!(caseLine.Rule is CommunicationRule))
                    {
                        string temp = "";
                        temp += insertInstanceName(caseLine.Rule.Condition, instance);
                        temp += " : ";
                        if (caseLine.Result is InstancedExp)
                        {
                            if (!caseLine.Result.ToString().Contains(instance.Name))
                            {
                                temp += instance.Name + "." + caseLine.Result;
                            }
                            else
                                temp += caseLine.Result;
                        }
                        else if (caseLine.Result is Expression)
                        {
                            temp += caseLine.Result;
                        }
                        temp += ";\n";
                        op += temp;
                    }
                }
                op += "esac;\n";
                return "next (" + instance.Name + "." + var.Name + ") := " + op;
            }
            else
            {
                return "Couldn't generate next for " + var.Name + " at " + MethodBase.GetCurrentMethod().Name + ";\n"; ;
            }
        }

        public static ICondition insertInstanceName(ICondition left, Instance instance)
        {
            // ICondition condition = null;
            if (left is BoolExp)
            {
                BoolExp condition = new BoolExp();
                BoolExp boolExp = (BoolExp)left;
                if (boolExp.Left.Exp.Contains(SMVKeys.SELF))
                {
                    if (boolExp.Left is OperExp)
                    {
                        OperExp tempOper = new OperExp();
                        tempOper.Exp = (boolExp.Left as OperExp).Exp;
                        tempOper.Oper.Value = (boolExp.Left as OperExp).Oper.Value;
                        tempOper.Result = (boolExp.Left as OperExp).Result;
                        condition.Left = new Expression();
                        condition.Left.Exp = tempOper.ToString().Replace(SMVKeys.SELF, instance.Name);
                    }
                    else
                    {
                        condition.Left = new Expression();
                        condition.Left.Exp = condition.ToString().Replace(SMVKeys.SELF, instance.Name);
                    }
                    condition.RelationalOperator = boolExp.RelationalOperator;
                    condition.Right = new Expression();
                    condition.Right.Exp = boolExp.Right.Exp;
                }
                else if (!boolExp.Left.Exp.Contains(instance.Name))
                {
                    // if it is a shared variable (such as main's synch variable), then don't call with compartment name
                    if (boolExp.Left.Exp == CustomVariables.SYNCH)
                    {
                        // actually do nothing.
                        condition.Left = new Expression();
                        condition.Left.Exp = boolExp.Left.Exp;
                    }
                    else if (boolExp.Left is OperExp)
                    {
                        condition.Left = new Expression();
                        condition.Left.Exp = insertInstanceName2Expression(boolExp.Left, instance);
                    }
                    else
                    {
                        condition.Left = new Expression();
                        condition.Left.Exp = instance.Name + "." + boolExp.Left.Exp;
                    }
                    condition.RelationalOperator = boolExp.RelationalOperator;
                    condition.Right = new Expression();
                    condition.Right.Exp = boolExp.Right.Exp;
                }
                else
                {
                    condition = boolExp;
                }
                return condition;
            }
            else if (left is BoolVariable)
            {
                BoolVariable boolGuard = (BoolVariable)left;
                BoolVariable condition = new BoolVariable();
                if (boolGuard.BoolGuard.Exp == SMVKeys.SELF)
                {
                    (condition as BoolVariable).BoolGuard.Exp = boolGuard.BoolGuard.Exp.Replace(SMVKeys.SELF, instance.Name);
                }
                else
                {
                    (condition as BoolVariable).BoolGuard.Exp = instance.Name + "." + boolGuard.BoolGuard.Exp;
                }
                return condition; ;
            }
            else if (left is NegatedBoolExpression)
            {
                NegatedBoolExpression negated = (NegatedBoolExpression)left;
                NegatedBoolExpression condition = new NegatedBoolExpression();

                BoolExp boolExp = (BoolExp)negated.BoolExpression;
                if (boolExp.Left.Exp == SMVKeys.SELF)
                {
                    ((condition as NegatedBoolExpression).BoolExpression as BoolExp).Left.Exp = boolExp.Left.Exp.Replace(SMVKeys.SELF, instance.Name);
                }
                else
                {
                    ((condition as NegatedBoolExpression).BoolExpression as BoolExp).Left.Exp = instance.Name + "." + boolExp.Left.Exp;
                }
                return condition;
            }
            else if (left is TruthValue)
            {
                return left;
            }
            else
            {
                if (left != null)
                {
                    CompoundBoolExpression comp = (CompoundBoolExpression)left;
                    CompoundBoolExpression compoundBoolExpr = new CompoundBoolExpression();
                    compoundBoolExpr.LeftCondition = insertInstanceName(comp.LeftCondition, instance);
                    compoundBoolExpr.BinaryOperator = comp.BinaryOperator;
                    compoundBoolExpr.RightCondition = insertInstanceName(comp.RightCondition, instance);
                    // condition = compoundBoolExpr;
                    return compoundBoolExpr;
                }
                else
                {
                    return null;
                }
            }
        }

        internal static string WriteStatusNextWithInstance(NuSMV.Module module, Instance instance)
        {
            string op = "";
            Variable status = module.Status;
            op = "next (" + instance.Name + "." + module.Status.Name + ") := case\n";
            foreach (var caseLine in status.Next.CaseStatement.CaseLines)
            {
                // CaseLine newcaseLine = new CaseLine();
                if (!(caseLine.Rule is CommunicationRule))
                {
                    string temp = "";
                    temp += TVariables.insertInstanceName(caseLine.Rule.Condition, instance);
                    temp += " : ";
                    // for last true case
                    if (caseLine.Rule.Condition is TruthValue)
                    {
                        temp += instance.Name + "." + caseLine.Result;
                    }
                    else
                    {
                        temp += caseLine.Result;
                    }
                    temp += ";\n";
                    op += temp;
                }
            }
            op += "esac;\n";
            return op;
        }

        /// <summary>
        /// Print status rules of each child instance
        /// </summary>
        internal static string WriteChildStatusNext(NuSMV.Module module, ChildInstance childInstance)
        {
            string op = "";
            Variable status = childInstance.DivisionStatus;
            op = "next (" + childInstance.Name + "." + status.Name + ") := case\n";

            bool firstRule = true;// first rule related with parent, so, its rule shouldn't be named with child instances
            foreach (var caseLine in status.Next.CaseStatement.CaseLines)
            {
                string temp = "";
                if (!firstRule)
                {
                    // add child name as prefix
                    temp += TVariables.insertInstanceName(caseLine.Rule.Condition, childInstance);
                }
                else
                {
                    temp += caseLine.Rule.Condition;
                    firstRule = false;
                }
                temp += " : ";
                // for last true case
                if (caseLine.Rule.Condition is TruthValue)
                {
                    temp += childInstance.Name + "." + caseLine.Result;
                }
                else
                {
                    temp += caseLine.Result;
                }
                temp += ";\n";
                op += temp;
            }
            op += "esac;\n";
            return op;
        }

        internal static string WriteParentDivisionVariableNext(NuSMV.Module module, ParentInstance parentInstance)
        {
            string op = "";
            foreach (var variable in module.Variables)
            {
                if (variable.Behaviour == VariableBehaviour.DIVISION)
                {
                    op += "next (" + parentInstance.Name + "." + variable.Name + ") := case\n";
                    foreach (var caseLine in (variable as Variable).Next.CaseStatement.CaseLines)
                    {
                        string temp = "\t";
                        temp += TVariables.insertInstanceName(caseLine.Rule.Condition, parentInstance);
                        temp += " : ";
                        temp += insertInstanceName2Expression(caseLine.Result, parentInstance);
                        //if (caseLine.Result is InstancedExp)
                        //    temp += parentInstance.Name + "." + caseLine.Result;
                        //else if (caseLine.Result is Expression)
                        //    temp += caseLine.Result;

                        temp += ";\n";
                        op += temp;
                    }
                    op += "esac;\n";
                }
            }
            return op;
        }

        internal static string WriteChildDivisionVariablesNext(ChildInstance childInstance)
        {
            string op = "";
            foreach (var variable in childInstance.DivisionVariables)
            {
                op += "next (" + childInstance.Name + "." + variable.Name + ") := case\n";

                bool firstRule = true;// first rule related with parent, so, its rule shouldn't be named with child instances
                foreach (var caseLine in variable.Next.CaseStatement.CaseLines)
                {
                    string temp = "\t";
                    if (!firstRule)
                    {
                        // add child name as prefix
                        temp += insertInstanceName(caseLine.Rule.Condition, childInstance);
                    }
                    else
                    {
                        temp += caseLine.Rule.Condition;
                    }
                    temp += " : ";
                    temp += insertInstanceName2Expression(caseLine.Result, childInstance);
                    temp += ";\n";
                    op += temp;
                    firstRule = false;
                }
                op += "esac;\n";
            }
            return op;
        }

        private static string insertInstanceName2Expression(IExp expression, Instance instance)
        {
            string temp = "";
            //_c1.e + self.e_cp
            if (expression is OperExp)
            {
                OperExp operExp = (OperExp)expression;
                // c1.e
                temp += instance.Name + "." + operExp.Exp;
                // if right of operation requires instance name
                //c1.e + c1.e_cp
                if (operExp.Result is InstancedExp)
                {
                    temp += " " + operExp.Oper.Value + " " + instance.Name + "." + operExp.Result;
                }
                else
                {
                    temp += " " + operExp.Oper.Value + " " + operExp.Result;
                }
            }
            else if (expression is InstancedExp)
                temp += instance.Name + "." + expression;
            else if (expression is Expression)
                temp += expression;
            return temp;
        }
    }
}