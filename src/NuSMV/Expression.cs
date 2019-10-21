namespace NuSMV
{
    /// <summary>
    /// Interface of any string expression
    /// </summary>
    public interface IExp
    {
        string Exp { get; set; }
    }

    /// <summary>
    /// Any string expression
    /// </summary>
    public class Expression : IExp
    {
        public Expression()
        {
        }

        public Expression(string exp)
        {
            this.Exp = exp;
        }

        public string Exp { get; set; }

        public override string ToString()
        {
            return Exp;
        }
    }

    /// <summary>
    /// Any expression with instance name such as self.protein
    /// </summary>
    public class InstancedExp : Expression
    {
        public InstancedExp()
        {
        }

        public InstancedExp(string exp)
        {
            this.Exp = exp;
        }

        public InstancedExp(string self, string expr)
        {
            this.Self = self;
            this.Exp = expr;
        }

        public string Self { get; set; }

        public override string ToString()
        {
            if (Self == null)
            {
                return string.Format("{0}", Exp);
            }
            return string.Format("{0}.{1}", Self, Exp);
        }
    }

    /// <summary>
    /// Expression which has operation
    /// e.g. (instace1.step + 7)
    /// </summary>
    public class OperExp : InstancedExp
    {
        public MathOper oper;

        public OperExp()
        { }

        public OperExp(string exp)
        {
            this.Exp = exp;
            this.Result = new Expression();
        }

        public OperExp(string self, string expr)
        {
            this.Self = self;
            this.Exp = expr;
            this.Result = new Expression();
        }

        public OperExp(string self, string expr, MathOper oper)
        {
            this.Self = self;
            this.Exp = expr;
            this.Oper = oper;
            this.Result = new Expression();
        }

        public OperExp(string self, string expr, MathOper oper, string result)
        {
            this.Self = self;
            this.Exp = expr;
            this.Oper = oper;
            this.Result = new Expression(result);
        }

        public MathOper Oper
        {
            get
            {
                if (oper == null)
                {
                    this.oper = new MathOper();
                    return this.oper;
                }
                else
                    return this.oper;
            }
            set
            {
                this.oper = value;
            }
        }

        public IExp Result { get; set; }

        public override string ToString()
        {
            string result = "";
            if (this.Exp != null)
            {
                result = Exp;
            }
            if (Self != null)
            {
                result = Self + "." + Exp;
            }
            if (Oper != null && Result != null)
                result += " " + Oper.Value + " " + Result;
            return result;
        }
    }
}