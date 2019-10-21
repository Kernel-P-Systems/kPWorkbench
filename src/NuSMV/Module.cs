using System;
using System.Collections.Generic;

namespace NuSMV
{
    /// <summary>
    /// Includes Module data structure, and variables of Main Module
    /// </summary>
    public class MainModule
    {
        public MainModule()
        {
            initiliazeSync();
            initiliazePStep();
        }

        private void initiliazeSync()
        {
            Synch = new Variable();
            Synch.Name = CustomVariables.SYNCH;
            Synch.Behaviour = VariableBehaviour.CUSTOM;
            SEnum states = new SEnum();
            states.Values.Add(SynchStates.BUSY); //Modules still runs
            states.Values.Add(SynchStates.EXCHANGE);//All modules completed P Step, ready to update variable values
            Synch.Type = states;
            //Synch.Init = SynchStates.BUSY; //To enable initial state, we need to start in EXCHANGE state
            Synch.Init = SynchStates.EXCHANGE;
        }

        private void initiliazePStep()
        {
            PStep = new Variable();
            PStep.Name = CustomVariables.PSTEP;
            PStep.Behaviour = VariableBehaviour.CUSTOM;
            SBool boolean = new SBool();
            PStep.Type = boolean;
            // PStep.Init = Truth.FALSE; //To enable initial state, we need to start in TRUE state
            PStep.Init = Truth.TRUE;
        }

        public Variable Synch { get; set; }
        public Variable PStep { get; set; }
    }

    public class Module
    {
        //counts, the executed rules, used for arb, seq and max which has substeps
        private Variable count = null;

        private string type;

        private Variable turn = null;

        public Module()
        {
            //   HasSubSteps = false;
            TurnOrder = new List<string>();
            HasSequenceStrategy = false;
            HasArbitraryStrategy = false;
            HasMaxStrategy = false;

            HasConnection = false;
            Parameters = new HashSet<ParameterVar>();
            HasDivisionRule = false;
            HasDissolutionRule = false;
            ExecutionStrategies = new List<Strategy>();
            Variables = new HashSet<IVar>();
        }

        /// <summary>
        /// List of connection variables.
        /// </summary>
        private List<NoNextVar> connections = null;

        public List<NoNextVar> Connections
        {
            get
            {
                if (connections == null)
                {
                    connections = new List<NoNextVar>();
                    return connections;
                }
                else
                    return connections;
            }
            set
            {
                connections = value;
            }
        }

        public Variable Count
        {
            get
            {
                if (count == null)
                {
                    count = new Variable(CustomVariables.COUNT);
                    count.Behaviour = VariableBehaviour.CUSTOM;
                    count.Init = "0";
                    count.Type = new NuSMV.BoundInt(0, BoundedNumbers.MAXRANDOM);
                    return count;
                }
                else
                {
                    return count;
                }
            }
            set
            {
                count = value;
            }
        }

        public string Name { get; set; }

        public void setModuleName(string moduleName, string instanceName)
        {
            Name = instanceName + "_" + moduleName;
        }

        public bool HasConnection { get; set; }

        public bool HasDissolutionRule { get; set; }

        public bool HasDivisionRule { get; set; }

        public List<Strategy> ExecutionStrategies { get; set; }

        public bool HasChoiceStrategy { get; set; }

        public bool HasArbitraryStrategy { get; set; }

        public bool HasSequenceStrategy { get; set; }

        public bool HasMaxStrategy { get; set; }

        //will hold the instances are generated after division rules
        private List<ChildInstance> childInstances = null;

        public List<ChildInstance> ChildInstances
        {
            get
            {
                if (childInstances == null)
                {
                    childInstances = new List<ChildInstance>();
                    return childInstances;
                }
                else
                    return
                        childInstances;
            }
            set
            {
                childInstances = value;
            }
        }

        private Instance instance = null;

        public Instance Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Instance();
                    return instance;
                }
                else
                    return instance;
            }
            set
            {
                instance = value;
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                if (SMVUtil.notReserved(value))
                {
                    type = value;
                }
                else
                    throw new Exception("The '" + value + "' is a reserved keyword in NUSMV. Rename it from the model.");
            }
        }

        public HashSet<ParameterVar> Parameters { get; set; }

        public Variable Status { get; set; }

        public Variable Turn
        {
            get
            {
                if (turn == null)
                {
                    return new Variable(CustomVariables.TURN);
                }
                else
                {
                    return turn;
                }
            }
            set
            {
                turn = value;
            }
        }

        //tracks order of executions strategies.
        public List<String> TurnOrder { get; set; }

        public HashSet<IVar> Variables { get; set; } //includes parameters, and all variables comes from model

        public IVar getVariable(string variableName)
        {
            IVar result = null;
            foreach (var variable in this.Variables)
            {
                if (variable.Name == variableName)
                {
                    result = variable;
                    break;
                }
            }
            return result;
        }

        public bool isVariableExist(string variableName)
        {
            bool result = false;
            foreach (var variable in this.Variables)
            {
                if (variable.Name == variableName)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// If current module has more than one connection to
        /// target module, then returns true, false otherwise.
        /// </summary>
        /// <param name="targetModule"></param>
        /// <returns></returns>
        public bool connectionToModuleExist(Module targetModule)
        {
            bool result = false;
            foreach (var connection in connections)
            {
                if (connection.Name.Equals(SMVPreFix.getConnectionVar(targetModule)))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// returns connection Variable which corresponds to given module
        /// </summary>
        /// <param name="targetModule"></param>
        /// <returns></returns>
        public NoNextVar getConnectionToModule(Module targetModule)
        {
            NoNextVar result = null;
            foreach (var connection in connections)
            {
                if (connection.Name.Equals(SMVPreFix.getConnectionVar(targetModule)))
                {
                    result = connection;
                    break;
                }
            }
            return result;
        }
    }
}