namespace NuSMV
{
    public interface ICondition
    {
    }

    /// <summary>
    /// Atomic proposition. Boolean expression
    /// e.g (self.proteing >= 5)
    /// </summary>
    public class BoolExp : ICondition
    {
        public BoolExp()
        {
            //default operator for bool expr is greater or equal
            this.RelationalOperator = new RelationalOperator();
            this.RelationalOperator.Operator = RelationalOperator.GEQ;
        }

        public BoolExp(IExp left, string relationalOperator, IExp right)
        {
            this.Left = left;
            this.RelationalOperator = new RelationalOperator();
            this.RelationalOperator.Operator = relationalOperator;
            this.Right = right;
        }

        public BoolExp(string left, string relationalOperator, string right)
        {
            this.Left = new Expression(left);
            this.RelationalOperator = new RelationalOperator();
            this.RelationalOperator.Operator = relationalOperator;
            this.Right = new Expression(right);
        }

        public IExp Left { get; set; }

        public RelationalOperator RelationalOperator { get; set; }

        public IExp Right { get; set; }

        public override string ToString()
        {
            return "(" + Left + " " + RelationalOperator.Operator + " " + Right + ")";
        }
    }

    /// <summary>
    /// A Boolean type variable
    /// eg. flag : boolean, flag is bool variable
    /// </summary>
    public class BoolVariable : ICondition
    {
        public IExp BoolGuard { get; set; }

        public BoolVariable()
        {
        }

        public BoolVariable(string variable)
        {
            BoolGuard = new Expression(variable);
        }

        public BoolVariable(IExp variable)
        {
            BoolGuard = variable;
        }

        public override string ToString()
        {
            return BoolGuard.ToString();
        }
    }

    public class CompoundBoolExpression : ICondition
    {
        public CompoundBoolExpression()
        {
            this.BinaryOperator = new BinaryOperator();
        }

        public CompoundBoolExpression(ICondition leftCondition, string binaryOperator, ICondition rightCondition)
        {
            this.LeftCondition = leftCondition;
            this.RightCondition = rightCondition;
            this.BinaryOperator = new BinaryOperator();
            if (leftCondition != null && rightCondition != null)
            {
                this.BinaryOperator.Operator = binaryOperator;
            }
        }

        public BinaryOperator BinaryOperator { get; set; }

        public ICondition LeftCondition { get; set; }

        public ICondition RightCondition { get; set; }

        public override string ToString()
        {
             string result = "";
            //both null, cannot use any
            if (LeftCondition == null && RightCondition == null)
            {
                return result;
            }
            //use left
            else if (LeftCondition != null && RightCondition == null)
            {
                result = "(" + LeftCondition + ")";
            }
            //use right
            else if (LeftCondition == null && RightCondition != null)
            {
                result = "(" + RightCondition + ")";
            }
            //use both side operands
            else if (LeftCondition != null && RightCondition != null)
            {
                result = "(" + LeftCondition + " " + BinaryOperator.Operator + " " + RightCondition + ")";
            }
            return result;
        }
    }

    /// <summary>
    /// The only operation is negation.
    /// It negates the proposition.
    /// </summary>
    public class NegatedBoolExpression : ICondition
    {
        public NegatedBoolExpression(ICondition guard)
        {
            this.BoolExpression = guard;
        }

        public NegatedBoolExpression()
        {
        }

        public ICondition BoolExpression { get; set; }
    }

    /// <summary>
    /// TRUE OR FALSE
    /// </summary>
    public class TruthValue : ICondition
    {
        public TruthValue()
        {
            Truth = new Truth();
        }

        public TruthValue(string value)
        {
            Truth = new Truth();
            Truth.Value = value;
        }

        public Truth Truth { get; set; }

        public override string ToString()
        {
            return Truth.Value;
        }
    }
}