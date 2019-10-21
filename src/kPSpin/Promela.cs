using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin
{
    public class Promela
    {
        public static string nl = System.Environment.NewLine;
        public static string Indent(int indent)
        {
            if (indent == 0)
            {
                return "";
            }
            else if (indent == 1)
            {
                return "\t";
            }
            else if (indent == 2)
            {
                return "\t\t";
            }
            else
            {
                StringBuilder buf = new StringBuilder();
                for (int i = 0; i < indent; i++)
                {
                    buf.Append("\t");
                }
                return buf.ToString();
            }
        }
    }

    public class PromelaModel
    {
        private string name;
        private string description;
        private Dictionary<string, string> meta;

        public string Name { get { return name; } set { name = value; } }
        public string Description { get { return description; } set { description = value; } }

        public List<string> PropertyTranslations { get; protected set; }

        public Dictionary<string, string> Meta { get { return meta; } protected set { meta = value; } }

        private List<ConstantDeclaration> constants;
        public List<ConstantDeclaration> Constants { get { return constants; } protected set { constants = value; } }

        private MTypeDeclaration mtypeDecl;
        public MTypeDeclaration MTypeDeclaration { get { return mtypeDecl; } protected set { mtypeDecl = value; } }

        private List<TypeDef> typedefs;
        public List<TypeDef> TypeDefs { get { return typedefs; } protected set { typedefs = value; } }

        public List<TypedDeclaration> globalVars;
        public List<TypedDeclaration> GlobalVariables { get { return globalVars; } protected set { globalVars = value; } }

        private List<Inline> inlineDefinitions;
        public List<Inline> InlineDefinitions { get { return inlineDefinitions; } protected set { inlineDefinitions = value; } }

        private List<ProcessDef> processes;
        public List<ProcessDef> Processes { get { return processes; } protected set { processes = value; } }

        private Init init;
        public Init Init { get { return init; } protected set { init = value; } }

        public PromelaModel()
        {
            meta = new Dictionary<string, string>();
            constants = new List<ConstantDeclaration>();
            mtypeDecl = new MTypeDeclaration();
            typedefs = new List<TypeDef>();
            globalVars = new List<TypedDeclaration>();
            inlineDefinitions = new List<Inline>();
            processes = new List<ProcessDef>();
            init = new Init();
            PropertyTranslations = new List<string>();
        }
    }

    public enum StatementType
    {
        CONSTANT_DECLARATION,
        VARIABLE_DECLARATION,
        MTYPE_DECLARATION,
        ARRAY_DECLARATION,
        INLINE_DEFINITION,
        PROC_DEFINITION,
        INIT,
        PARAM,
        TYPEDEF,
        ASSIGNMENT,
        INLINE_CALL,
        PROCESS_RUN,
        SKIP,
        BREAK,
        INCREMENT,
        GOTO,
        PRINTF,
        LABEL,
        IF,
        DO,
        FOR,
        ATOMIC,
        D_STEP,
        ATTR_EVAL,
        PREDICATE
    }

    public abstract class Statement : IPromela
    {
        protected StatementType type;
        public StatementType Type { get { return type; } }

        protected string nl = System.Environment.NewLine;

        public Statement(StatementType stype)
        {
            type = stype;
        }

        public abstract string ToPromela();
        public abstract string ToPromela(int indent);
    }

    public abstract class CompoundStatement : Statement
    {
        private List<Statement> nestedStatements;
        public List<Statement> NestedStatements { get { return nestedStatements; } private set { nestedStatements = value; } }

        public CompoundStatement(StatementType stype)
            : base(stype)
        {
            nestedStatements = new List<Statement>();
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            foreach (IPromela t in nestedStatements)
            {
                buf.Append(t.ToPromela());
                buf.Append(nl);
            }

            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            foreach (IPromela t in nestedStatements)
            {
                buf.Append(t.ToPromela(indent));
                buf.Append(nl);
            }

            return buf.ToString();
        }
    }

    public interface IValue : IPromela
    {
    }

    public class Constant : IValue
    {
        public string Value { get; set; }

        public Constant(string val)
        {
            Value = val;
        }

        public Constant(int val)
        {
            Value = val.ToString();
        }

        public string ToPromela()
        {
            return Value;
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public enum VarType
    {
        INT, BIT, MTYPE, TYPEDEF
    }

    public abstract class TypedDeclaration : Statement
    {
        public VarType VType { get; set; }
        public TypeDef typeDef;
        public TypeDef TypeDef { get { return typeDef; } set { typeDef = value; VType = VarType.TYPEDEF; } }

        public TypedDeclaration(StatementType st)
            : base(st)
        {
        }

        public string TypeString
        {
            get
            {
                switch (VType)
                {
                    case VarType.INT: return "int";
                    case VarType.BIT: return "bit";
                    case VarType.MTYPE: return "mtype";
                    case VarType.TYPEDEF: return TypeDef.Name;
                }

                return null;
            }
        }
    }

    public class VariableDeclaration : TypedDeclaration
    {
        public string Name { get; set; }
        public IValue InitValue { get; set; }
        public int? InitValueInt { get; set; }
        public string InitValueString { get; set; }

        public VariableDeclaration(TypeDef type, string name, int? initvalue = null)
            : base(StatementType.VARIABLE_DECLARATION)
        {
            TypeDef = type;
            Name = name;
            InitValueInt = initvalue;
        }

        public VariableDeclaration(VarType type, string name, int? initvalue = null)
            : base(StatementType.VARIABLE_DECLARATION)
        {
            VType = type;
            Name = name;
            InitValueInt = initvalue;
        }

        public VariableDeclaration(VarType type, string name, string initvalue)
            : base(StatementType.VARIABLE_DECLARATION)
        {
            VType = type;
            Name = name;
            InitValueString = initvalue;
        }

        public VariableDeclaration(VarType type, string name, IValue initvalue)
            : base(StatementType.VARIABLE_DECLARATION)
        {
            VType = type;
            Name = name;
            InitValue = initvalue;
        }

        public Attribute ToAttribute()
        {
            return new Attribute(Name);
        }

        public override string ToPromela()
        {
            return TypeString + " " + Name + (InitValue == null ?
                (InitValueInt == null ? (InitValueString == null ? ";" : " = " + InitValueString + ";") :
                " = " + InitValueInt.Value + ";") :
                (" = " + InitValue.ToPromela() + ";"));
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class ConstantDeclaration : Statement
    {
        public string Name { get; set; }
        public string InitValue { get; set; }

        public ConstantDeclaration(string name, string initvalue)
            : base(StatementType.CONSTANT_DECLARATION)
        {
            Name = name;
            InitValue = initvalue;
        }

        public ConstantDeclaration(string name, int initvalue)
            : this(name, initvalue.ToString())
        {
        }

        public ConstantDeclaration(string name, Constant initValue)
            : this(name, initValue.Value)
        {
        }

        public override string ToPromela()
        {
            return "#define " + Name + " " + InitValue;
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class MTypeDeclaration : TypedDeclaration
    {
        public HashSet<string> Mtypes { get; set; }

        public MTypeDeclaration()
            : base(StatementType.MTYPE_DECLARATION)
        {
            Mtypes = new HashSet<string>();
        }

        public MTypeDeclaration Add(params string[] items)
        {
            foreach (string item in items)
            {
                Mtypes.Add(item);
            }

            return this;
        }

        public override string ToPromela()
        {
            if (Mtypes.Count == 0)
            {
                return "";
            }
            int i = 1;
            int c = Mtypes.Count;
            StringBuilder buf = new StringBuilder();
            buf.Append("mtype = {");
            foreach (string item in Mtypes)
            {
                buf.Append(item);
                if (i++ < c)
                {
                    buf.Append(", ");
                }
            }
            buf.Append("}");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class ArrayDeclaration : TypedDeclaration
    {
        public string Name { get; set; }
        public int? Size { get; set; }
        public Attribute SizeAttr { get; set; }
        public int? InitialValue { get; set; }

        public ArrayDeclaration(VarType type, string name, int size)
            : this(type, name, size, null)
        {
        }

        public ArrayDeclaration(VarType type, string name, int size, int? initialValue = null)
            : base(StatementType.ARRAY_DECLARATION)
        {
            VType = type;
            Name = name;
            Size = size;
            InitialValue = initialValue;
        }

        public ArrayDeclaration(TypeDef type, string name, int size, int? initialValue = null)
            : base(StatementType.ARRAY_DECLARATION)
        {
            TypeDef = type;
            Name = name;
            Size = size;
            InitialValue = initialValue;
        }

        public ArrayDeclaration(VarType type, string name, Attribute size, int? initialValue = null)
            : base(StatementType.ARRAY_DECLARATION)
        {
            VType = type;
            Name = name;
            SizeAttr = size;
            InitialValue = initialValue;
        }

        public Attribute ToAttribute(int index)
        {
            return new Attribute(Name, index);
        }

        public Attribute ToAttribute(string index)
        {
            return new Attribute(Name, index);
        }

        public override string ToPromela()
        {
            return TypeString + " " + Name + "[" + (Size == null ? SizeAttr.Name : Size.ToString()) + "]" +
                (InitialValue == null ? "" : (" = " + InitialValue.Value)) + ";";
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class ProcessParam : TypedDeclaration
    {
        public string Name { get; set; }

        public ProcessParam(VarType type, string name)
            : base(StatementType.PARAM)
        {
            Name = name;
            VType = type;
        }

        public override string ToPromela()
        {
            return TypeString + " " + Name;
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class ProcessDef : CompoundStatement
    {
        public string Name { get; set; }
        private List<ProcessParam> parameters;
        public List<ProcessParam> Parameters { get { return parameters; } private set { parameters = value; } }

        public ProcessDef(string name)
            : base(StatementType.PROC_DEFINITION)
        {
            Name = name;
            parameters = new List<ProcessParam>();
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("proctype ").Append(Name).Append("(");
            int count = parameters.Count;
            int i = 1;
            foreach (ProcessParam var in parameters)
            {
                buf.Append(var.ToPromela());
                if (i++ < count)
                {
                    buf.Append(", ");
                }
            }
            buf.Append(") {").Append(nl);
            buf.Append(base.ToPromela());
            buf.Append("}");

            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("proctype ").Append(Name).Append("(");
            int count = parameters.Count;
            int i = 1;
            foreach (ProcessParam var in parameters)
            {
                buf.Append(var.ToPromela());
                if (i++ < count)
                {
                    buf.Append(", ");
                }
            }
            buf.Append(") {").Append(nl);
            buf.Append(base.ToPromela(indent + 1));
            buf.Append(Promela.Indent(indent));
            buf.Append("}");

            return buf.ToString();
        }
    }

    public class Init : CompoundStatement
    {
        public Init()
            : base(StatementType.INIT)
        {
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("init {");
            buf.Append(base.ToPromela());
            buf.Append("}");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent)).Append("init {").Append(nl);
            buf.Append(base.ToPromela(indent + 1));
            buf.Append(Promela.Indent(indent)).Append("}");
            return buf.ToString();
        }
    }

    public class Printf : Statement
    {
        public List<IValue> Parameters { get; set; }
        public string Format { get; set; }

        public Printf()
            : base(StatementType.PRINTF)
        {
            Parameters = new List<IValue>();
        }

        public Printf(string format, params IValue[] parameters)
            : this()
        {
            Format = format;
            Parameters.AddRange(parameters);
        }

        public Printf(int val)
            : this()
        {
            Format = val.ToString();
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("printf(\"");
            if (!String.IsNullOrEmpty(Format))
            {
                buf.Append(Format);
            }
            else
            {
                for (int i = 0; i < Parameters.Count; i++)
                {
                    buf.Append("%d");
                    if (i < Parameters.Count - 1)
                    {
                        buf.Append(", ");
                    }
                }
            }
            buf.Append("\"");

            if (Parameters.Count > 0)
            {
                buf.Append(", ");
                int j = 0;
                foreach (IValue attr in Parameters)
                {
                    buf.Append(attr.ToPromela());
                    if (j++ < Parameters.Count - 1)
                    {
                        buf.Append(", ");
                    }
                }
            }
            buf.Append(");");

            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class Inline : CompoundStatement
    {

        public string Name { get; set; }
        public List<string> Params { get; set; }

        public Inline(string name)
            : base(StatementType.INLINE_DEFINITION)
        {
            Name = name;
            Params = new List<string>();
        }

        public Inline(string name, params string[] parameters)
            : this(name)
        {
            Params.AddRange(parameters);
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("inline ").Append(Name).Append("(");
            int i = 0;
            foreach (string p in Params)
            {
                buf.Append(p);
                i++;
                if (i < Params.Count)
                {
                    buf.Append(", ");
                }
            }
            buf.Append(") {").Append(nl);
            buf.Append(base.ToPromela());
            buf.Append("}");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("inline ").Append(Name).Append("(");
            int i = 0;
            foreach (string p in Params)
            {
                buf.Append(p);
                i++;
                if (i < Params.Count)
                {
                    buf.Append(", ");
                }
            }
            buf.Append(") {").Append(nl);
            buf.Append(base.ToPromela(indent + 1));
            buf.Append(Promela.Indent(indent)).Append("}");
            return buf.ToString();
        }
    }

    public class TypeDef : CompoundStatement
    {

        public string Name { get; set; }

        public TypeDef(string name)
            : base(StatementType.TYPEDEF)
        {
            Name = name;
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("typedef " + Name + " {");
            buf.Append(nl);
            buf.Append(base.ToPromela());
            buf.Append(nl).Append("}");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("typedef " + Name + " {");
            buf.Append(nl);
            buf.Append(base.ToPromela(indent + 1));
            buf.Append("}").Append(nl);
            return buf.ToString();
        }
    }

    public class Instruction : Statement
    {

        private IEnumerable<string> Args;
        private String Name;

        private Instruction(StatementType st)
            : base(st)
        {
        }

        private Instruction(StatementType st, string name, IEnumerable<string> args)
            : this(st)
        {
            Name = name;
            Args = args;
        }

        public static Instruction InlineCall(string name, params string[] args)
        {
            return new Instruction(StatementType.INLINE_CALL, name, args);
        }

        public static Instruction ProcessRun(string name, params string[] args)
        {
            return new Instruction(StatementType.PROCESS_RUN, name, args);
        }

        public static Instruction ProcessRun(ProcessDef proc, params string[] args)
        {
            return new Instruction(StatementType.PROCESS_RUN, proc.Name, args);
        }

        public static Instruction Skip()
        {
            return new Instruction(StatementType.SKIP);
        }

        public static Instruction Break()
        {
            return new Instruction(StatementType.BREAK);
        }

        public static Instruction Increment(Attribute attr)
        {
            return new Instruction(StatementType.INCREMENT, attr.ToPromela(), null);
        }

        public static Instruction Goto(string label)
        {
            return new Instruction(StatementType.GOTO, label, null);
        }

        public static Instruction Attribute(Attribute attr)
        {
            return new Instruction(StatementType.ATTR_EVAL, attr.ToPromela(), null);
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            switch (type)
            {
                case StatementType.SKIP:
                    {
                        buf.Append("skip;");
                    } break;
                case StatementType.BREAK:
                    {
                        buf.Append("break;");
                    } break;
                case StatementType.INLINE_CALL:
                    {
                        buf.Append(Name + "(");
                        if (Args != null)
                        {
                            int i = 0;
                            foreach (string arg in Args)
                            {
                                buf.Append(arg);
                                buf.Append(", ");
                                i++;
                            }
                            if (i > 0)
                            {
                                buf.Remove(buf.Length - 2, 2);
                            }
                        }
                        buf.Append(");");
                    } break;
                case StatementType.PROCESS_RUN:
                    {
                        buf.Append("run " + Name + "(");
                        if (Args != null)
                        {
                            int i = 0;
                            foreach (string arg in Args)
                            {
                                buf.Append(arg);
                                buf.Append(", ");
                                i++;
                            }
                            if (i > 0)
                            {
                                buf.Remove(buf.Length - 2, 2);
                            }
                        }
                        buf.Append(");");
                    } break;
                case StatementType.INCREMENT:
                    {
                        buf.Append(Name + "++;");
                    } break;
                case StatementType.GOTO:
                    {
                        buf.Append("goto ").Append(Name).Append(";");
                    } break;
                default: buf.Append(Name).Append(";"); break;
            }

            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class Label : Statement
    {
        public string Name { get; set; }
        public Statement Action { get; set; }

        public Label(string name)
            : base(StatementType.LABEL)
        {
            Name = name;
        }

        public Label(string name, Statement action)
            : this(name)
        {
            Action = action;
        }

        public override string ToPromela()
        {
            return Name + ": " + (Action == null ? "skip;" : Action.ToPromela());
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class Attribute : IValue, IPromela
    {
        public Attribute Parent { get; set; }
        public string Name { get; set; }
        public Attribute Index { get; set; }

        public Attribute(Attribute attr, string name, Attribute index)
        {
            Parent = attr;
            Name = name;
            Index = index;
        }

        public Attribute(string name, Attribute index)
            : this(null, name, index)
        {
        }

        public Attribute(string name)
        {
            Parent = null;
            Name = name;
            Index = null;
        }

        public Attribute(VariableDeclaration varDecl)
            : this(varDecl.Name)
        {
        }

        public Attribute(ArrayDeclaration arr, Attribute index)
            : this(arr.Name, index)
        {
        }

        public Attribute(string name, string index)
            : this(name, new Attribute(index))
        {

        }

        public Attribute(string name, int index)
            : this(name, new Attribute(index.ToString()))
        {
        }

        public Attribute(VariableDeclaration varDecl, int index)
            : this(varDecl.Name, index)
        {
        }

        public Attribute(ArrayDeclaration arr, int index)
            : this(arr.Name, index)
        {
        }

        public Attribute Dot(Attribute attr)
        {
            Attribute attribute = attr.Copy();
            attribute.Parent = this;

            return attribute;
        }

        public Attribute Dot(string attrName)
        {
            return Dot(new Attribute(attrName));
        }

        public Attribute Dot(string attrName, int index)
        {
            Attribute x = new Attribute(attrName, index);
            x.Parent = this;

            return x;
        }

        public Attribute Copy()
        {
            Attribute attr = new Attribute(this.Name, this.Index);

            //copy parents
            Attribute at = attr;
            Attribute par = this.Parent;
            while (par != null)
            {
                at.Parent = new Attribute(par.Name, par.Index);
                at = at.Parent;
                par = par.Parent;
            }

            return attr;
        }

        public string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            if (Parent != null)
            {
                buf.Append(Parent.ToPromela());
                buf.Append(".");
            }
            buf.Append(Name);
            if (Index != null)
            {
                buf.Append("[").Append(Index.ToPromela()).Append("]");
            }

            return buf.ToString();
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class Assignment : Statement
    {

        public Attribute Attribute { get; set; }
        public int ValueInt { get; set; }
        public IValue Value { get; set; }

        public Assignment(string varName, int val)
            : this(new Attribute(varName), val)
        {

        }

        public Assignment(Attribute attr, int val)
            : base(StatementType.ASSIGNMENT)
        {
            Attribute = attr;
            ValueInt = val;
        }

        public Assignment(Attribute attr, string val)
            : this(attr, new Attribute(val))
        {

        }

        public Assignment(Attribute lhs, IValue rhs)
            : base(StatementType.ASSIGNMENT)
        {
            Attribute = lhs;
            Value = rhs;
        }

        public override string ToPromela()
        {
            return Attribute.ToPromela() + " = " + (Value == null ? ValueInt.ToString() : Value.ToPromela()) + ";";
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public enum PromelaRelOperator
    {
        EQ, LEQ, GEQ, LT, GT, NEQ
    }

    public enum PromelaLogicalOperator
    {
        AND, OR
    }

    public interface ICondition : IValue, IPromela
    {
    }

    public class Condition : ICondition
    {
        public Attribute Var { get; set; }

        public Condition(Attribute var)
        {
            Var = var;
        }

        public Condition(string c)
            : this(new Attribute(c))
        {

        }

        public static Condition Else()
        {
            return new Condition("else");
        }

        public string ToPromela()
        {
            return Var.ToPromela();
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class RelCondition : ICondition
    {

        public PromelaRelOperator Op { get; set; }
        public IValue Lhs { get; set; }
        public IValue Rhs { get; set; }


        public RelCondition(PromelaRelOperator op, IValue left, IValue right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }
            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            Lhs = left;
            Rhs = right;
            Op = op;
        }

        public CompositeCondition And(ICondition cond)
        {
            return new CompositeCondition(PromelaLogicalOperator.AND, this, cond);
        }

        public CompositeCondition Or(ICondition cond)
        {
            return new CompositeCondition(PromelaLogicalOperator.OR, this, cond);
        }

        public string ToPromela()
        {
            return Lhs.ToPromela() + " " + RelOpToString(Op) + " " + Rhs.ToPromela();
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }

        public static string RelOpToString(PromelaRelOperator op)
        {
            switch (op)
            {
                case PromelaRelOperator.EQ: return "==";
                case PromelaRelOperator.GEQ: return ">=";
                case PromelaRelOperator.LEQ: return "<=";
                case PromelaRelOperator.LT: return "<";
                case PromelaRelOperator.GT: return ">";
                case PromelaRelOperator.NEQ: return "!=";
            }
            return "";
        }
    }

    public class NegatedCondition : ICondition
    {
        ICondition Condition { get; set; }

        public NegatedCondition(ICondition condition)
        {
            Condition = condition;
        }

        public string ToPromela()
        {
            if (Condition is CompositeCondition || Condition is RelCondition)
            {
                return "!(" + Condition.ToPromela() + ")";
            }
            else
            {
                return "!" + Condition.ToPromela();
            }
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }
    }

    public class CompositeCondition : ICondition
    {
        public ICondition Lhs { get; set; }
        public ICondition Rhs { get; set; }
        public PromelaLogicalOperator Op { get; set; }

        public CompositeCondition(PromelaLogicalOperator op, ICondition lhs, ICondition rhs)
        {
            if (lhs == null)
            {
                throw new ArgumentNullException("lhs");
            }

            if (rhs == null)
            {
                throw new ArgumentNullException("rhs");
            }

            Lhs = lhs;
            Rhs = rhs;
            Op = op;
        }

        public CompositeCondition And(ICondition cond)
        {
            return new CompositeCondition(PromelaLogicalOperator.AND, this, cond);
        }

        public CompositeCondition Or(ICondition cond)
        {
            return new CompositeCondition(PromelaLogicalOperator.OR, this, cond);
        }

        public NegatedCondition Negated
        {
            get
            {
                return new NegatedCondition(this);
            }
        }

        public string ToPromela()
        {
            StringBuilder buf = new StringBuilder();

            bool lparant = false;
            if (Lhs is CompositeCondition)
            {
                if ((Lhs as CompositeCondition).Op != Op)
                {
                    lparant = true;
                }
            }

            bool rparant = false;
            if (Rhs is CompositeCondition)
            {
                if ((Rhs as CompositeCondition).Op != Op)
                {
                    rparant = true;
                }
            }

            if (lparant)
            {
                buf.Append("(");
            }
            buf.Append(Lhs.ToPromela());
            if (lparant)
            {
                buf.Append(")");
            }
            buf.Append(" ").Append(LogicalOpToString(Op)).Append(" ");
            if (rparant)
            {
                buf.Append("(");
            }
            buf.Append(Rhs.ToPromela());
            if (rparant)
            {
                buf.Append(")");
            }
            return buf.ToString();
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + ToPromela();
        }

        public static string LogicalOpToString(PromelaLogicalOperator op)
        {
            switch (op)
            {
                case PromelaLogicalOperator.AND: return "&&";
                case PromelaLogicalOperator.OR: return "||";
            }
            return "";
        }
    }

    public class PredicateStatement : Statement
    {
        public ICondition Condition { get; set; }

        public PredicateStatement(ICondition cond)
            : base(StatementType.PREDICATE)
        {
            Condition = cond;
        }

        public override string ToPromela()
        {
            return Condition.ToPromela() + ";";
        }

        public override string ToPromela(int indent)
        {
            return Promela.Indent(indent) + this.ToPromela();
        }
    }

    public class Branch : IPromela
    {

        private ICondition condition;
        public ICondition Condition { get { return condition; } set { condition = value; } }

        private List<Statement> actions;
        public List<Statement> Actions { get { return actions; } protected set { actions = value; } }

        public Branch()
        {
            actions = new List<Statement>();
        }

        public Branch(ICondition c, params Statement[] acts)
        {
            Condition = c;
            actions = new List<Statement>();
            if (actions != null)
            {
                Actions.AddRange(acts);
            }
        }

        public Branch(Statement action)
            : this(null, action)
        {
        }

        public string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(":: ").Append(Condition.ToPromela()).Append(" ->");
            buf.Append(Promela.nl);
            foreach (Statement action in actions)
            {
                buf.Append(action.ToPromela()).Append(Promela.nl);
            }

            return buf.ToString();
        }

        public string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("::");
            if (Condition != null)
            {
                buf.Append(" ").Append(Condition.ToPromela()).Append(" ->");
            }
            if (actions.Count == 1)
            {
                Statement a = actions[0];
                if (a != null && (a.Type == StatementType.SKIP || a.Type == StatementType.BREAK ||
                    a.Type == StatementType.INCREMENT || a.Type == StatementType.GOTO || a.Type == StatementType.INLINE_CALL || a.Type == StatementType.ASSIGNMENT))
                {
                    buf.Append(" ").Append(a.ToPromela());
                    return buf.ToString();
                }
            }

            int i = 0;
            if (i < actions.Count)
            {
                foreach (Statement action in actions)
                {
                    buf.Append(Promela.nl).Append(action.ToPromela(indent + 1));
                }
            }
            return buf.ToString();
        }
    }

    public abstract class ConditionalStatement : Statement
    {

        private List<Branch> branches;
        public List<Branch> Branches { get { return branches; } set { branches = value; } }

        public ConditionalStatement(StatementType st)
            : base(st)
        {
            branches = new List<Branch>();
        }

        public ConditionalStatement Branch(Branch b)
        {
            branches.Add(b);
            return this;
        }

        public ConditionalStatement Else(params Statement[] ss)
        {
            branches.Add(new Branch(Condition.Else(), ss));
            return this;
        }
    }

    public class IfStatement : ConditionalStatement
    {

        public IfStatement()
            : base(StatementType.IF)
        {
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("if");
            foreach (Branch b in Branches)
            {
                buf.Append(b.ToPromela());
            }
            buf.Append("fi;");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("if").Append(nl);
            foreach (Branch b in Branches)
            {
                buf.Append(b.ToPromela(indent + 1));
                buf.Append(nl);
            }
            buf.Append(Promela.Indent(indent));
            buf.Append("fi;");
            return buf.ToString();
        }
    }

    public class DoStatement : ConditionalStatement
    {

        public DoStatement()
            : base(StatementType.DO)
        {
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("do");
            foreach (Branch b in Branches)
            {
                buf.Append(b.ToPromela());
            }
            buf.Append("od;");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("do").Append(nl);
            foreach (Branch b in Branches)
            {
                buf.Append(b.ToPromela(indent + 1));
                buf.Append(nl);
            }
            buf.Append(Promela.Indent(indent));
            buf.Append("od;");
            return buf.ToString();
        }
    }

    public class ForStatement : CompoundStatement
    {

        private IValue lowBound;
        private IValue highBound;
        private string iterator;

        public IValue LowBound { get { return lowBound; } set { lowBound = value; } }
        public IValue HighBound { get { return highBound; } set { highBound = value; } }
        public string Iterator { get { return iterator; } set { iterator = value; } }

        public ForStatement(string iterator, IValue lBound, IValue hBound)
            : base(StatementType.FOR)
        {
            LowBound = lBound;
            HighBound = hBound;
            Iterator = iterator;
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("for(").Append(iterator).Append(": ").Append(lowBound.ToPromela()).Append(" .. ").Append(highBound.ToPromela()).Append(") {");
            buf.Append(nl).Append(base.ToPromela()).Append(nl);
            buf.Append("}");

            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            buf.Append("for(").Append(iterator).Append(": ").Append(lowBound.ToPromela()).Append(" .. ").Append(highBound.ToPromela()).Append(") {");
            buf.Append(nl).Append(base.ToPromela(indent + 1));
            buf.Append(Promela.Indent(indent)).Append("}");

            return buf.ToString();
        }
    }

    public class BracedStatement : CompoundStatement
    {

        private BracedStatement(StatementType st, params Statement[] innerStatements)
            : base(st)
        {
            NestedStatements.AddRange(innerStatements);
        }

        public static BracedStatement Atomic(params Statement[] innerStatements)
        {
            return new BracedStatement(StatementType.ATOMIC, innerStatements);
        }

        public static BracedStatement D_step(params Statement[] innerStatements)
        {
            return new BracedStatement(StatementType.D_STEP, innerStatements);
        }

        public override string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            if (Type == StatementType.ATOMIC)
            {
                buf.Append("atomic {");
            }
            else
            {
                buf.Append("d_step {");
            }
            buf.Append(nl);
            buf.Append(base.ToPromela()).Append(nl);
            buf.Append("}");
            return buf.ToString();
        }

        public override string ToPromela(int indent)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Promela.Indent(indent));
            if (Type == StatementType.ATOMIC)
            {
                buf.Append("atomic {");
            }
            else
            {
                buf.Append("d_step {");
            }
            buf.Append(nl);
            buf.Append(base.ToPromela(indent + 1));
            buf.Append(Promela.Indent(indent)).Append("}");
            return buf.ToString();
        }

    }

    public enum PromelaArithmeticOperator
    {
        PLUS, MINUS, TIMES, DIVIDES
    }

    public class ArithmeticExpression : IValue, IPromela
    {
        public PromelaArithmeticOperator Operator { get; set; }
        public IValue Lhs { get; set; }
        public IValue Rhs { get; set; }
        public int? IntLhs { get; set; }
        public int? IntRhs { get; set; }

        public ArithmeticExpression(PromelaArithmeticOperator op, IValue lhs, IValue rhs)
        {
            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
        }

        public ArithmeticExpression(PromelaArithmeticOperator op, IValue lhs, int rhs)
        {
            Operator = op;
            Lhs = lhs;
            IntRhs = rhs;
        }

        public ArithmeticExpression(PromelaArithmeticOperator op, int lhs, IValue rhs)
        {
            Operator = op;
            IntLhs = lhs;
            Rhs = rhs;
        }

        public ArithmeticExpression(PromelaArithmeticOperator op, int lhs, int rhs)
        {
            Operator = op;
            IntLhs = lhs;
            IntRhs = rhs;
        }

        public string ToPromela()
        {
            StringBuilder buf = new StringBuilder();
            if (Lhs is ArithmeticExpression)
            {
                buf.Append("(" + IntLhs ?? Lhs.ToPromela() + ")");
            }
            else
            {
                buf.Append(IntLhs == null ? Lhs.ToPromela() : IntLhs.ToString());
            }

            switch (Operator)
            {
                case PromelaArithmeticOperator.PLUS: buf.Append(" + "); break;
                case PromelaArithmeticOperator.MINUS: buf.Append(" - "); break;
                case PromelaArithmeticOperator.TIMES: buf.Append(" * "); break;
                case PromelaArithmeticOperator.DIVIDES: buf.Append(" / "); break;
            }

            if (Rhs is ArithmeticExpression)
            {
                buf.Append("(" + IntRhs ?? Rhs.ToPromela() + ")");
            }
            else
            {
                buf.Append(IntRhs == null ? Rhs.ToPromela() : IntRhs.ToString());
            }

            return buf.ToString();
        }

        public string ToPromela(int indent)
        {
            return Promela.Indent(indent) + this.ToPromela();
        }
    }
}
