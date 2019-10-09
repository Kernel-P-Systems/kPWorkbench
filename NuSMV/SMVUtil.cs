using System;
using System.Collections.Generic;
using System.Linq;

namespace NuSMV
{
    public static class CustomVariables
    {
        public static string STATUS { get { return "_status"; } }

        public static string CONNECTIONS { get { return "_conn"; } }

        public static string TURN { get { return "_turn"; } }

        public static string CHOICE { get { return "_cho"; } }

        public static string SEQUENCE { get { return "_seq"; } }

        public static string ARBITRARY { get { return "_arb"; } }

        public static string MAX { get { return "_max"; } }

        public static string RAND { get { return "_rand"; } }

        public static string COUNT { get { return "_count"; } }

        public static string GUARD { get { return "_guard"; } }

        public static string SYNCH { get { return "_sync"; } }

        //Tobe compatible with Spin, PStep named as pInS
        public static string PSTEP { get { return "pInS"; } }
    }

    public static class StatusStates
    {
        public static string ACTIVE { get { return "_ACTIVE"; } }

        public static string WILLDISSOLVE { get { return "_willDISSOLVE"; } }

        public static string DISSOLVED { get { return "_DISSOLVED"; } }

        public static string WILLDIVIDE { get { return "_willDIVIDE"; } }

        public static string DIVIDED { get { return "_DIVIDED"; } }

        public static string NONEXIST { get { return "_NONEXIST"; } }
    }

    public class BoundedNumbers
    {
        public static int MAXRANDOM { get { return 3; } }
    }

    public static class TurnStates
    {
        public static string CHOICE { get { return "_CHO"; } }

        public static string SEQUENCE { get { return "_SEQ"; } }

        public static string ARBITRARY { get { return "_ARB"; } }

        public static string MAX { get { return "_MAX"; } }

        public static string READY { get { return "_READY"; } }

        public static string getState(KpCore.StrategyOperator strategyOperator)
        {
            if (strategyOperator == KpCore.StrategyOperator.CHOICE)
                return TurnStates.CHOICE;
            else if (strategyOperator == KpCore.StrategyOperator.SEQUENCE)
                return TurnStates.SEQUENCE;
            else if (strategyOperator == KpCore.StrategyOperator.ARBITRARY)
                return TurnStates.ARBITRARY;
            else if (strategyOperator == KpCore.StrategyOperator.MAX)
                return TurnStates.MAX;
            else
                return null;
        }
    }

    public static class SynchStates
    {
        public static string BUSY { get { return "_BUSY"; } }

        public static string EXCHANGE { get { return "_EXCH"; } }
    }

    public class BinaryOperator
    {
        public static String AND { get { return "&"; } }

        public static String OR { get { return "|"; } }

        public String Operator { get; set; }
    }

    public class MathOper
    {
        public MathOper()
        {
        }

        public MathOper(string value)
        {
            this.Value = value;
        }

        public static string ADD { get { return "+"; } }

        public static string DIV { get { return "/"; } }

        public static string MOD { get { return "%"; } }

        public static string MULT { get { return "*"; } }

        public static string SUB { get { return "-"; } }

        public string Value { get; set; }
    }

    public class RelationalOperator
    {
        public static String EQUAL { get { return "="; } }

        public static String GEQ { get { return ">="; } }

        public static String GT { get { return ">"; } }

        public static String LEQ { get { return "<="; } }

        public static String LT { get { return "<"; } }

        public static String NOT_EQUAL { get { return "!="; } }

        public String Operator { get; set; }
    }

    public class SMVKeys
    {
        public static string ASSIGN { get { return "ASSIGN"; } }

        public static string CASE { get { return "case"; } }

        public static string CTLSPEC { get { return "CTLSPEC"; } }

        public static string ESAC { get { return "esac"; } }

        public static string INIT { get { return "init"; } }

        public static string INVAR { get { return "INVAR"; } }

        public static string LTLSPEC { get { return "LTLSPEC"; } }

        public static string MAIN { get { return "main"; } }

        public static string MODULE { get { return "MODULE"; } }

        public static string NEXT { get { return "next"; } }

        public static string SELF { get { return "self"; } }

        public static string VAR { get { return "VAR"; } }
    }

    public class SMVPreFix
    {
        public static String PARAM { get { return "p_"; } }

        public static String TO { get { return "to_"; } }

        public static String COPY { get { return "_cp"; } }

        public static string CHILD { get { return "_child"; } }

        /// <summary>
        /// Get varname +COPY
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public static string getCopyName(Variable var)
        {
            return var.Name + SMVPreFix.COPY;
        }

        public static string getCopyName(string varName)
        {
            return varName + SMVPreFix.COPY;
        }

        /// <summary>
        /// Returns variableName_InstanceName
        /// </summary>
        public static string getConnectedCopyCommVarName(string varName, Module targetModule)
        {
            return varName + "_" + targetModule.Instance.Name;
        }

        /// <summary>
        /// Returns variableName_InstanceName
        /// </summary>
        public static string getConnectedCopyCommVarName(Variable var, Module targetModule)
        {
            return var.Name + "_" + targetModule.Instance.Name;
        }

        /// <summary>
        /// Returns to_connectedInstanceName, e.g. to_c1
        /// </summary>
        /// <param name="connectedInstance"></param>
        /// <returns></returns>
        public static string getConnectedTo(Instance connectedInstance)
        {
            return SMVPreFix.TO + connectedInstance.Name;
        }

        public static string getConnectionVar(KpCore.MType mType)
        {
            return CustomVariables.CONNECTIONS + mType.Name;
        }

        public static string getConnectionVar(Module targetModule)
        {
            return CustomVariables.CONNECTIONS + targetModule.Type;
        }

        public static string getGuardName(Module module, int strategyIndex, KpCore.Rule rule)
        {
            Strategy ex = module.ExecutionStrategies.ElementAt(strategyIndex);
            string turnState = ex.Name + strategyIndex;
            return turnState.ToLower() + CustomVariables.GUARD + rule.Id;
        }
    }

    public class SMVUtil
    {
        public static string generateAnInstanceName(Instance instance)
        {
            string name = "";
            if (instance.Module != null)
            {
                if (instance.Module.ChildInstances != null)
                {
                    name = "_" + instance.Module.Name.ToLower() + instance.Module.ChildInstances.Count;
                }
                else
                {
                    //first instance
                    name = "_" + instance.Module.Name.ToLower() + 0;
                }
            }
            return name;
        }

        public static BinaryOperator getBinaryOperator(KpCore.BinaryGuardOperator binaryGuardOperator)
        {
            NuSMV.BinaryOperator binaryOperator = new BinaryOperator();
            switch (binaryGuardOperator)
            {
                case KpCore.BinaryGuardOperator.AND:
                    {
                        binaryOperator.Operator = NuSMV.BinaryOperator.AND;
                        break;
                    }
                case KpCore.BinaryGuardOperator.OR:
                    {
                        binaryOperator.Operator = NuSMV.BinaryOperator.OR;
                        break;
                    }
                default:
                    {
                        binaryOperator.Operator = "undefined binary operator";
                        break;
                    }
            }
            return binaryOperator;
        }

        public static NuSMV.RelationalOperator getRelationalOperator(KpCore.RelationalOperator oper)
        {
            NuSMV.RelationalOperator relationalOperator = new RelationalOperator();
            switch (oper)
            {
                case KpCore.RelationalOperator.EQUAL:
                    {
                        relationalOperator.Operator = NuSMV.RelationalOperator.EQUAL;
                        break;
                    }
                case KpCore.RelationalOperator.GEQ:
                    {
                        relationalOperator.Operator = NuSMV.RelationalOperator.GEQ;
                        break;
                    }
                case KpCore.RelationalOperator.GT:
                    {
                        relationalOperator.Operator = NuSMV.RelationalOperator.GT;
                        break;
                    }
                case KpCore.RelationalOperator.LT:
                    {
                        relationalOperator.Operator = NuSMV.RelationalOperator.LT;
                        break;
                    }
                case KpCore.RelationalOperator.LEQ:
                    {
                        relationalOperator.Operator = NuSMV.RelationalOperator.LEQ;
                        break;
                    }
                case KpCore.RelationalOperator.NOT_EQUAL:
                    {
                        relationalOperator.Operator = NuSMV.RelationalOperator.NOT_EQUAL;
                        break;
                    }
                default:
                    {
                        relationalOperator.Operator = "undefined relational operator";
                        break;
                    }
            }
            return relationalOperator;
        }

        public static bool notReserved(string value)
        {
            bool notReserved = true;
            string[] reservedWords ={
                                 "MODULE", "DEFINE", "MDEFINE", "CONSTANTS", "VAR", "IVAR", "FROZENVAR",
                                 "INIT", "TRANS", "INVAR", "SPEC", "CTLSPEC", "LTLSPEC", "PSLSPEC", "COMPUTE",
                                 "NAME", "INVARSPEC", "FAIRNESS", "JUSTICE", "COMPASSION", "ISA", "ASSIGN",
                                 "CONSTRAINT", "SIMPWFF", "CTLWFF", "LTLWFF", "PSLWFF", "COMPWFF", "IN", "MIN",
                                 "MAX", "MIRROR", "PRED", "PREDICATES", "process", "array", "of", "boolean",
                                 "integer", "real", "word", "word1", "bool", "signed", "unsigned", "extend",
                                 "resize", "sizeof", "uwconst", "swconst", "EX", "AX", "EF", "AF", "EG", "AG", "E", "F", "O", "G",
                                 "H", "X", "Y", "Z", "A", "U", "S", "V", "T", "BU", "EBF", "ABF", "EBG", "ABG", "case", "esac",
                                 "mod", "next","init", "union", "in", "xor", "xnor", "self", "TRUE", "FALSE", "count "
                            };
            if (reservedWords.Contains(value))
                notReserved = false;
            return notReserved;
        }

        public static HashSet<IVar> sortVariable(HashSet<IVar> variables)
        {
            //sort them
            IEnumerable<IVar> orderedSet = variables.OrderBy(instance => instance.Name);

            HashSet<IVar> tempSet = new HashSet<IVar>();
            foreach (var item in orderedSet)
            {
                tempSet.Add(item);
            }
            return tempSet;
        }
    }

    public class Truth
    {
        public static String FALSE { get { return "FALSE"; } }

        public static String TRUE { get { return "TRUE"; } }

        public String Value { get; set; }
    }
}