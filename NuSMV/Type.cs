using System;
using System.Collections.Generic;

namespace NuSMV
{
    public interface Type
    {
    }

    public class BoundInt : Type
    {
        private int lowerBound = 0;
        private int upperBound = 1;

        public BoundInt()
        {
            this.LowerBound = 0;
            this.UpperBound = 1;
        }

        public BoundInt(int lowerBound, int upperBound)
        {
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
        }

        public int LowerBound
        {
            get { return lowerBound; }
            set
            {
                this.lowerBound = value;
            }
        }

        public int UpperBound
        {
            get { return upperBound; }
            set
            {
                if ((value >= lowerBound))
                {
                    upperBound = Math.Max(upperBound, value);
                }
                else
                    throw new Exception("Bound Exception, lower bound cannot be greater or equal to upper bound");
            }
        }

        public override string ToString()
        {
            return LowerBound + " .. " + UpperBound;
        }
    }

    public class SBool : Type
    {
        private string name = "boolean";

        public string Name { get { return name; } }

        public override string ToString()
        {
            return Name;
        }
    }

    public class SEnum : Type
    {
        public SEnum()
        {
            Values = new HashSet<string>();
        }

        public HashSet<string> Values { get; set; }

        public override string ToString()
        {
            return "{" + String.Join(",", Values) + "}";
        }
    }
}