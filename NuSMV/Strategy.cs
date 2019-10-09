using System;
using System.Collections.Generic;

namespace NuSMV
{
    public enum StrategyTypes
    {
        SEQUENCE,
        CHOICE,
        ARBITRARY,
        MAX
    }

    public class Strategy
    {
        public List<IVar> CustomVars { get; set; }

        public Strategy()
        {
            CustomVars = new List<IVar>();
        }

        public IVar getCustomVariable(string variableName)
        {
            IVar result = null;
            foreach (var variable in this.CustomVars)
            {
                if (variable.Name == variableName)
                {
                    result = variable;
                    break;
                }
            }
            return result;
        }

        public int TotalRule { get; set; }

        private StrategyTypes type;

        public StrategyTypes Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
            }
        }

        public string Name
        {
            get
            {
                if (Type == StrategyTypes.CHOICE)
                {
                    return TurnStates.CHOICE;
                }
                else if (Type == StrategyTypes.ARBITRARY)
                {
                    return TurnStates.ARBITRARY;
                }
                else if (Type == StrategyTypes.SEQUENCE)
                {
                    return TurnStates.SEQUENCE;
                }
                else if (Type == StrategyTypes.MAX)
                {
                    return TurnStates.MAX;
                }
                else
                {
                    throw new Exception("Unknown execution strategy type '" + Type + "'.");
                }
            }
        }

        public override string ToString()
        {
            return String.Format(String.Join(", ", CustomVars));
        }
    }

    public class ArbitraryStrategy : Strategy
    {
        public ArbitraryStrategy()
        {
            Type = StrategyTypes.ARBITRARY;
        }
    }

    public class ChoiceStrategy : Strategy
    {
        public ChoiceStrategy()
        {
            Type = StrategyTypes.CHOICE;
        }
    }

    public class MAXStrategy : Strategy
    {
        public MAXStrategy()
        {
            Type = StrategyTypes.MAX;
        }
    }

    public class SequenceStrategy : Strategy
    {
        public SequenceStrategy()
        {
            Type = StrategyTypes.SEQUENCE;
        }
    }
}