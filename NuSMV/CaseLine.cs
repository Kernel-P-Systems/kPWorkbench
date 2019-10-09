namespace NuSMV
{
    public enum SMVRuleType
    {
        REWRITING,
        COMMUNICATION
    }

    public interface ICaseLine
    {
        IExp Result { get; set; }

        Rule Rule { get; set; }
    }

    public class CaseLine : ICaseLine
    {
        private Rule rule;

        public IExp Result { get; set; }

        public Rule Rule
        {
            get
            {
                if (this.rule == null)
                    this.rule = new Rule();

                return this.rule;
            }
            set
            {
                this.rule = value;
            }
        }

        public override string ToString()
        {
            return Rule + " : " + Result + ";\n";
        }
    }

    public class CommunicationRule : Rule
    {
        /// <summary>
        /// Rule belogs to which module type
        /// </summary>
        public string SourceName { get; set; }
    }

    public class Rule
    {
        public ICondition Condition { get; set; }

        public int ID { get; set; }

        public SMVRuleType Type { get; set; }

        public void AddBoolExpression(ICondition newCondition, string binaryOperator)
        {
            this.Condition = new CompoundBoolExpression(newCondition, binaryOperator, this.Condition);
        }

        public void AppendBoolExpression(ICondition newCondition, string binaryOperator)
        {
            this.Condition = new CompoundBoolExpression(this.Condition, binaryOperator, newCondition);
        }

        public override string ToString()
        {
            return Condition.ToString();
        }
    }
}