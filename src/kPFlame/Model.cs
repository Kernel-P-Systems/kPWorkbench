using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore;

namespace KpFLAME
{
    public class Model
    {
        private List<Agent> agents;
        public List<Agent> Agents { get { return agents; } }

        public Model()
        {
            agents = new List<Agent>();
        }

        public void AddAgent(Agent agent)
        {
            agents.Add(agent);
        }
    }

    public class Condition
    {
        public enum RelationalOperators
        {
            EQ, NEQ, LEQ, GEQ, LT, GT
        }

        public enum LogicOperators
        {
            OR, AND
        }

        static readonly Dictionary<LogicOperators, string> logicOperators;
        static readonly Dictionary<RelationalOperators, string> relationalOperators;
        static Condition()
        {
            logicOperators = new Dictionary<LogicOperators, string>();
            logicOperators.Add(LogicOperators.OR, "OR");
            logicOperators.Add(LogicOperators.AND, "AND");

            relationalOperators = new Dictionary<RelationalOperators, string>();
            relationalOperators.Add(RelationalOperators.EQ, "EQ");
            relationalOperators.Add(RelationalOperators.GEQ, "GEQ");
            relationalOperators.Add(RelationalOperators.GT, "GT");
            relationalOperators.Add(RelationalOperators.LEQ, "LEQ");
            relationalOperators.Add(RelationalOperators.LT, "LT");
            relationalOperators.Add(RelationalOperators.NEQ, "NEQ");
        }
        public string LOp { get; set; }
        public string Lhs { get; set; }
        public string Op { get; set; }
        public string Rhs { get; set; }
        public Condition(string lhs, RelationalOperators relationalOperator, string rhs)
        {
            LOp = null;
            Lhs = lhs;
            Op = Condition.relationalOperators[relationalOperator];
            Rhs = rhs;
        }
        public Condition(LogicOperators logicOperator, string lhs, RelationalOperators relationalOperator, string rhs) :
            this(lhs, relationalOperator, rhs)
        {
            LOp = logicOperators[logicOperator];
        }
    }

    public class Input
    {
        public string IsLogicOperators {
            get
            {
                if (Filter == null)
                    return null;
                else if (Filter.Count <= 1)
                    return null;
                else
                    return "";
            }
            set {}
        }
        public string Name { get; set; }
        public List<Condition> Filter { get; set; }

        public Input(string name)
        {
            Name = name;
            Filter = null;
        }

        public void AddCondition(Condition condition)
        {
            if(Filter == null)
                Filter = new List<Condition>();
            Filter.Add(condition);
        }
    }

    public class Inputs
    {
        private List<Input> inputs;
        public List<Input> Messages { get { return inputs; } private set { } }

        public Inputs()
        {
            inputs = new List<Input>();
        }

        public void AddMessage(string name)
        {
            inputs.Add(new Input(name));
        }
    }
    public class Outputs
    {
        private List<string> outputs;
        public List<string> Messages { get { return outputs; } private set { } }

        public Outputs()
        {
            outputs = new List<string>();
        }

        public void AddMessage(string name)
        {
            outputs.Add(name);
        }
    }

    public class Function
    {
        private List<int> ruleNumbers;
        public List<int> RuleNumbers {get { return ruleNumbers; }}
        public StrategyOperator? StrategyType { get; set; }
        public string StrategyName
        {
            get
            {
                if (StrategyType != null)
                    switch (StrategyType)
                    {
                        case StrategyOperator.SEQUENCE: return "applyRules_Sequence";
                        case StrategyOperator.CHOICE: return "applyRules_Choice";
                        case StrategyOperator.ARBITRARY: return "applyRules_Arbitrary_Parallel";
                        case StrategyOperator.MAX: return "applyRules_Maximal_Parallel";
                    }
                return "";
            }
        }
        protected string name;
        protected int? number;
        public int? Number {get { return number; }}
        public string AgentName { get; set; }
        public string Name { get { return number != null ? string.Format("{0}_{1}_{2}", name, StrategyType, number) : name; } }
        public string Description { get; set; }
        public string CurrentState { get; set; }
        public string NextState { get; set; }
        public Condition Condition { get; set; }
        protected Inputs inputs;
        public Inputs Inputs { get { return inputs; } set { inputs = value; } }
        protected Outputs outputs;
        public Outputs Outputs { get { return outputs; } set { outputs = value; } }
        public Function(string agentName, string name, string currentState, string nextState)
        {
            AgentName = agentName;
            ruleNumbers = null;
            this.name = name;
            number = null;
            Description = name;
            this.CurrentState = currentState;
            this.NextState = nextState;
        }
        public Function(string agentName, string name, string currentState, string nextState, int number)
            : this(agentName, name, currentState, nextState)
        {
            this.number = number;
            Description = Name;
            this.NextState = Name;
        }
        public void Add(int start, int count)
        {
            ruleNumbers = new List<int>(Enumerable.Range(start, count));
        }
    }

    public class StrategyFunction : Function
    {
        public List<int> Rules { get; set; }
        public List<int> Guards { get; set; }
        public StrategyFunction(string agentName, string name, string currentState, string nextState) : base(agentName, name, currentState, nextState) { }
        public StrategyFunction(string agentName, string name, string currentState, string nextState, int number) : base(agentName, name, currentState, nextState, number) { }
    }

    public class Agent
    {
        private MType membraneType;
        private List<Function> functions;
        public int TypeId { get; private set; }
        public int RulesNumber { get; private set; }
        public List<Function> Functions { get { return functions; } }
        public string Name { get; set; }
        public string Description { get; set; }
        public Agent(MType membraneType, int typeId, FlameCodeRulesGenerator rulesGenerator)
        {
            this.membraneType = membraneType;
            this.TypeId = typeId;
            Name = membraneType.Name;
            Description = Name;
            RulesNumber = 0;
            functions = new List<Function>();
            Strategy(membraneType.ExecutionStrategy, rulesGenerator);
        }

        private void Strategy(ExecutionStrategy executionStrategy, FlameCodeRulesGenerator rulesGenerator)
        {
            Dictionary<StrategyOperator, int> strategyNumber = new Dictionary<StrategyOperator, int>();
            StrategyNumberCreate(strategyNumber);
            Function function = new Function(Name, JoinStrings(Name, "show"), JoinStrings(Name, "show"), JoinStrings(Name, "start"));
            AddFunction(function);
            function = new Function(Name, JoinStrings(Name, "initialization"), JoinStrings(Name, "start"), JoinStrings(Name, "initialization"));
            function.Condition = new Condition("a.structure_rule", Condition.RelationalOperators.EQ, "-1");
            AddFunction(function);
            function = new Function(Name, "idle", JoinStrings(Name, "start"), JoinStrings(Name, "end"));
            function.Condition = new Condition("a.structure_rule", Condition.RelationalOperators.NEQ, "-1");
            AddFunction(function);
            int ruleNumber = 0;
            string previousState = JoinStrings(Name, "initialization");
            string functionName = "";
            string currentState = "";
            string nextState = "";
            bool b = false;
            ExecutionStrategy strategy = membraneType.ExecutionStrategy;
            while (strategy != null)
            {
                RulesNumber += strategy.Rules.Count;
                strategyNumber[strategy.Operator]++;
                functionName = Name;
                currentState = previousState;
                StrategyFunction strategyFunction = new StrategyFunction(Name, Name, currentState, nextState, strategyNumber[strategy.Operator]);
                strategyFunction.StrategyType = strategy.Operator;
                strategyFunction.Rules = rulesGenerator.Rules(strategy);
                strategyFunction.Guards = rulesGenerator.Guards(strategy);
                if (strategy.Next != null)
                {
                    nextState = strategyFunction.Name;
                    strategyFunction.NextState = nextState;
                }
                else
                {
                    nextState = JoinStrings(Name, "execution_completed");
                    strategyFunction.NextState = nextState;
                }
                strategyFunction.Add(ruleNumber, strategy.Rules.Count);
                ruleNumber += strategy.Rules.Count;
                strategyFunction.Outputs = new Outputs();
                strategyFunction.Outputs.AddMessage("msgSendObject");
                if (b)
                {
                    strategyFunction.Condition = new Condition("a.stop", Condition.RelationalOperators.EQ, "0");
                    AddFunction(strategyFunction);
                    function = new Function(Name, "idle", currentState, JoinStrings(Name, "execution_completed"));
                    function.Condition = new Condition("a.stop", Condition.RelationalOperators.NEQ, "0");
                    AddFunction(function);
                }
                else
                    AddFunction(strategyFunction);
                previousState = nextState;
                b = true;
                strategy = strategy.Next;
            }
            function = new Function(Name, JoinStrings(Name, "PrepareTheNewMembrane"), JoinStrings(Name, "execution_completed"), JoinStrings(Name, "prepare_the_new_membrane"));
            function.Condition = new Condition("a.rule_type_selected", Condition.RelationalOperators.EQ, "2");
            function.Outputs = new Outputs();
            function.Outputs.AddMessage("msgRequestId");
            AddFunction(function);

            function = new Function(Name, JoinStrings(Name, "CreateNewMembrane"), JoinStrings(Name, "prepare_the_new_membrane"), JoinStrings(Name, "remove_membrane"));
            function.Inputs = new Inputs();
            function.Inputs.AddMessage("msgReceiveId");
            function.Inputs.Messages[0].AddCondition(new Condition(Condition.LogicOperators.AND, TypeId.ToString(), Condition.RelationalOperators.EQ, "m.type_id"));
            function.Inputs.Messages[0].AddCondition(new Condition("a.instanceId", Condition.RelationalOperators.EQ, "m.instance_id"));
            AddFunction(function);

            function = new Function(Name, JoinStrings(Name, "removeMembrane"), JoinStrings(Name, "remove_membrane"), JoinStrings(Name, "applyChanges"));
            AddFunction(function);

            function = new Function(Name, "idle", JoinStrings(Name, "execution_completed"), JoinStrings(Name, "remove_membrane"));
            function.Condition = new Condition("a.rule_type_selected", Condition.RelationalOperators.EQ, "3");
            AddFunction(function);

            function = new Function(Name, "idle", JoinStrings(Name, "execution_completed"), JoinStrings(Name, "applyChanges"));
            function.Condition = new Condition("a.rule_type_selected", Condition.RelationalOperators.LEQ, "1");
            AddFunction(function);

            function = new Function(Name, JoinStrings(Name, "applyChanges"), JoinStrings(Name, "applyChanges"), JoinStrings(Name, "receive"));
            AddFunction(function);
            function = new Function(Name, JoinStrings(Name, "receive"), JoinStrings(Name, "receive"), JoinStrings(Name, "end"));
            function.Inputs = new Inputs();
            function.Inputs.AddMessage("msgSendObject");
            function.Inputs.Messages[0].AddCondition(new Condition(Condition.LogicOperators.AND, "a.id", Condition.RelationalOperators.EQ, "m.idTo"));
            function.Inputs.Messages[0].AddCondition(new Condition("a.instanceId", Condition.RelationalOperators.EQ, "m.instanceIdTo"));
            AddFunction(function);
        }

        private void StrategyNumberCreate(Dictionary<StrategyOperator, int> strategyNumber)
        {
            strategyNumber.Add(StrategyOperator.SEQUENCE, 0);
            strategyNumber.Add(StrategyOperator.CHOICE, 0);
            strategyNumber.Add(StrategyOperator.ARBITRARY, 0);
            strategyNumber.Add(StrategyOperator.MAX, 0);
        }

        private string JoinStrings(string s1, string s2)
        {
            return string.Format("{0}_{1}", s1, s2);
        }

        private void AddFunction(Function function)
        {
            functions.Add(function);
        }
    }
}
