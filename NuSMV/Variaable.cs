using System;

namespace NuSMV
{
    [Obsolete("These structures are obsolete, they are kept just for preventing error. When FormNuSMV is removed, then these file can be deleted as well.")]
    /// <summary>
    /// VariableType presents where var has generated, if it exist in many rule types,
    /// then COMMUNICATION type will override REWRITING, and DIVISION will override
    /// the COMMUNICATION.
    /// </summary>
    public enum VariableType
    {
        REWRITING,
        COMMUNICATION,
        DIVISION,
        DISSOLUTION,
        FLAG
    }

    [Obsolete("These structures are obsolete, they are kept just for preventing error. When FormNuSMV is removed, then these file can be deleted as well.")]
    public class Variaable
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private int minVal;

        public int MinVal
        {
            get { return minVal; }
            set { minVal = value; }
        }

        private int maxVal = 1;

        public int MaxVal
        {
            get { return maxVal; }
            set
            {
                maxVal = Math.Max(maxVal, value);
            }
        }

        private VariableType type = VariableType.REWRITING;

        public VariableType Type
        {
            get { return type; }
            set
            {
                if (value == VariableType.COMMUNICATION & type != VariableType.DIVISION)
                {
                    type = value;
                }
                else
                {
                    type = value;
                }
            }
        }

        public Variaable(string name)
        {
            this.name = name;
            this.minVal = 0;
            this.maxVal = 1;
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is Variaable))
                return false;
            Variaable var2 = (Variaable)obj;
            if (this.Name.Equals(var2.Name))
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            string op = this.Name;
            return op;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}