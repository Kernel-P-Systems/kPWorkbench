using System;

namespace NuSMV
{
    public enum VariableBehaviour
    {
        REWRITING,
        COMMUNICATION,
        DIVISION,
        DISSOLUTION,
        CUSTOM
    }

    public enum VariableOrigin
    {
        Original,
        Copy,
        OriginalCommVar,
        CopyOfCommVar
    }

    public interface IVar
    {
        VariableBehaviour Behaviour { get; set; }

        string Name { get; set; }

        Type Type { get; set; }
    }

    /// <summary>
    /// NonDeterministic Variables for ability of choosing a random rule, such as choice, arbitrary, and max
    /// </summary>
    public class NonDeterVar : Var
    {
        private Case iNVARCase = null;

        private Case nonDetCase = null;

        public NonDeterVar()
        {
        }

        public NonDeterVar(string name)
        {
            Name = name;
        }

        public Case INVARCase
        {
            get
            {
                if (iNVARCase == null)
                {
                    iNVARCase = new Case();
                    return iNVARCase;
                }
                else
                    return iNVARCase;
            }
            set
            {
                iNVARCase = value;
            }
        }

        public Case NonDetCase
        {
            get
            {
                if (nonDetCase == null)
                {
                    nonDetCase = new Case();
                    return nonDetCase;
                }
                else
                    return nonDetCase;
            }
            set
            {
                this.nonDetCase = value;
            }
        }

        /// <summary>
        /// Replaces \n after each rule.(or operation)
        /// </summary>
        /// <returns></returns>
        public string addNewLineINVAR()
        {
            string result = INVARCase.ToString();
            result = result.Replace("|", "|\n\t\t");
            // result = result.Replace(":", "\n\t\t:");
            return result;
        }

        public string printINVAR()
        {
            return SMVKeys.INVAR + " " + addNewLineINVAR();
        }
    }

    public class NoNextVar : Var
    {
        public NoNextVar()
        {
            this.Init = "0";
        }

        public NoNextVar(string name)
        {
            Name = name;
        }

        public string Init { get; set; }
    }

    public class ParameterVar : Variable
    {
        public ParameterVar()
        {
            IsParamPrefixed = true;
        }

        public ParameterVar(string name)
        {
            this.Name = name;
            IsParamPrefixed = true;
        }

        public bool IsParamPrefixed { get; set; }

        public string ParamName
        {
            get
            {
                string str = this.Name;
                if (IsParamPrefixed)
                    str = SMVPreFix.PARAM + str;
                return str;
            }
        }

        public override string ToString()
        {
            return this.ParamName;
        }
    }

    public class Var : IVar
    {
        private VariableBehaviour behaviour;
        private string name;

        public Var()
        {
        }

        public VariableBehaviour Behaviour
        {
            get { return behaviour; }
            set
            {
                //give presence over other behaviours.
                if ((value == VariableBehaviour.CUSTOM) || (value == VariableBehaviour.DIVISION)
                    || (value == VariableBehaviour.DISSOLUTION))
                {
                    behaviour = value;
                }
                if (value == VariableBehaviour.REWRITING && this.behaviour != VariableBehaviour.COMMUNICATION)
                {
                    behaviour = value;
                }
                if (value == VariableBehaviour.COMMUNICATION)
                {
                    behaviour = value;
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (SMVUtil.notReserved(value))
                {
                    name = value;
                }
                else
                    throw new Exception("The '" + value + "' is a reserved keyword in NUSMV. Rename it from the model.");
            }
        }

        public Type Type { get; set; }

        public override bool Equals(Object obj)
        {
            if (!(obj is Var))
                return false;
            Var var2 = (Var)obj;
            if (this.Name.Equals(var2.Name))
                return true;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return this.Name + " : " + this.Type + ";";
        }
    }

    public class Variable : Var
    {
        private string init = "0";

        private Next next;

        public Variable()
        {
        }

        public Variable(string name)
        {
            this.Name = name;
            this.Init = "0";
            this.Origin = VariableOrigin.Original;
        }

        public string Init { get; set; }

        public Next Next
        {
            get
            {
                if (this.next == null)
                    this.next = new Next();
                return this.next;
            }
            set { this.next = value; }
        }

        public VariableOrigin Origin { get; set; }
    }
}