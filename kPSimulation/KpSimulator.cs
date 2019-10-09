using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSimulation {

    public class KpSimulator {

        private KPsystem kp;
        public KPsystem KPsystem { get { return kp; } set { Reset(value); } }
        private KpSimulationParams sParams;
        public KpSimulationParams SimulationParams {
            get { return sParams; }
            set {
                sParams = value; resetParamCache();
            }
        }

        public delegate void SimulationStartedEventHandler(KPsystem initialConfiguration);
        public delegate void ReachedStepEventHandler(int step);
        public delegate void StepCompleteEventHandler(KpSimulationStep step);
        public delegate void RuleAppliedEventHandler(Rule r, MInstance instance);
        public delegate void TargetSelectedEventHandler(MInstance target, MType type, Rule r);
        public delegate void SystemHaltedEventHandler(int haltStep);
        public delegate void SimulationCompleteEventHandler(int endStep);
        //a handler called each time a new instance is created in the division process
        //useful for asigning an Id (or other properties) consistent with the containing P system
        public delegate void NewInstanceCreatedHandler(MInstance instance, MType type);
        public delegate void NewInstanceRegistrationHandler(MInstance instance);

        public SimulationStartedEventHandler SimulationStarted { get; set; }
        public ReachedStepEventHandler ReachedStep { get; set; }
        public StepCompleteEventHandler StepComplete { get; set; }
        public RuleAppliedEventHandler RuleApplied { get; set; }
        public TargetSelectedEventHandler TargetSelected { get; set; }
        public SystemHaltedEventHandler SystemHalted { get; set; }
        public SimulationCompleteEventHandler SimulationComplete { get; set; }
        public NewInstanceCreatedHandler NewInstanceCreated { get; set; }
        public NewInstanceRegistrationHandler RegisterNewInstance { get; set; }

        private bool isHalted;
        private int startStep;
        private int endStep;
        private bool recordRuleSelection;
        private bool recordTargetSelection;
        private bool recordInstanceCreation;
        private int step;
        private int seed;

        /// <summary>
        /// The set of all instances of any type in the P system.
        /// </summary>
        private HashSet<ActiveInstance> allInstances;
        /// <summary>
        /// A set of all instances whose types have a non-empty set of rules (i.e. execution strategy).
        /// </summary>
        private HashSet<ActiveInstance> allOperationalInstances;
        private Dictionary<MInstance, ActiveInstance> aiMapping;
        private List<IndexedMType> indexedTypes;
        private Dictionary<string, IndexedMType> indexedTypesByName;

        private HashSet<ActiveInstance> newInstanceSet;
        private Random rand;

        private int rulesApplied;

        public KpSimulator() {
            allInstances = new HashSet<ActiveInstance>();
            allOperationalInstances = new HashSet<ActiveInstance>();
            aiMapping = new Dictionary<MInstance, ActiveInstance>();
            indexedTypes = new List<IndexedMType>();
            indexedTypesByName = new Dictionary<string, IndexedMType>();

            newInstanceSet = new HashSet<ActiveInstance>();
        }

        public KpSimulator(KpSimulationParams parameters)
            : this() {
            SimulationParams = parameters;
        }

        public KPsystem Run(KPsystem kps) {
            if (kps != kp) {
                Reset(kps);
            }
            return Run();
        }

        public KPsystem Run() {
            KpSimulationStep simStep = new KpSimulationStep() { KPsystem = kp, Step = step };

            if (SimulationStarted != null) {
                SimulationStarted(kp);
            }

            List<ActiveInstance> activeInstances = new List<ActiveInstance>();
            List<ActiveInstance> targetSelections = new List<ActiveInstance>();
            HashSet<ActiveInstance> deadCells = new HashSet<ActiveInstance>();
            
            while (!isHalted && step <= endStep) {

                rulesApplied = 0;
                bool advanceStrategyBlock = false;
                bool endStepForInstance = false;

                bool dissolutionRuleApplied = false;
                bool divisionRuleApplied = false;
                bool linkRuleApplied = false;

                activeInstances.AddRange(allOperationalInstances);
                int aiCount = activeInstances.Count;

                while (aiCount > 0) {

                    //reset instance flags
                    advanceStrategyBlock = false;
                    endStepForInstance = false;

                    //select an instance
                    ActiveInstance selectedInstance = activeInstances[rand.Next(aiCount)];

                    ExecutionStrategy ex = selectedInstance.CurrentStrategySegment;

                    //this can only happen if the rule set was set to empty
                    if (selectedInstance.ApplicableRules.Count > 0) {
                        Rule selectedRule = null;
                        bool isRuleApplicable = true;
                        if (ex.Operator == StrategyOperator.SEQUENCE) {
                            selectedRule = selectedInstance.ApplicableRules.First();
                            if (selectedRule.IsGuarded) {
                                if (!selectedRule.Guard.IsSatisfiedBy(selectedInstance.Instance.Multiset)) {
                                    isRuleApplicable = false;
                                } else if (selectedInstance.FlagStructureRuleApplied) {
                                    if (selectedRule.IsStructureChangingRule()) {
                                        isRuleApplicable = false;
                                    }
                                }
                            }
                        } else {
                            int selectedIndex = 0;
                            int arCount = selectedInstance.ApplicableRules.Count;

                            if (ex.Operator == StrategyOperator.ARBITRARY) {
                                selectedIndex = rand.Next(arCount + 1);
                            } else {
                                selectedIndex = rand.Next(arCount);
                            }

                            if (selectedIndex == arCount) {
                                //this essentially means "skip this block" - always in an arbitrary block, never elsewhere
                                selectedRule = null;
                                isRuleApplicable = false;
                            } else {
                                selectedRule = selectedInstance.ApplicableRules[selectedIndex];
                                if (selectedInstance.FlagStructureRuleApplied) {
                                    if (selectedRule.IsStructureChangingRule()) {
                                        isRuleApplicable = false;
                                    }
                                }
                            }
                        }

                        if (isRuleApplicable) {
                            if (selectedRule is ConsumerRule) {
                                if ((selectedRule as ConsumerRule).Lhs > selectedInstance.Instance.Multiset) {
                                    isRuleApplicable = false;
                                }
                            }
                        }

                        //At this point, the isRuleApplicable basically confirms that no structure rule has been applied thus far 
                        //if the selected rule requires this, if guarded, the guard of the selected rule holds and 
                        //the left hand multiset is contained in the instance multiset
                        if (isRuleApplicable) {
                            switch (selectedRule.Type) {
                                case RuleType.REWRITE_COMMUNICATION: {
                                        RewriteCommunicationRule rcr = selectedRule as RewriteCommunicationRule;
                                        foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in rcr.TargetRhs) {
                                            if (kv.Key is InstanceIdentifier) {
                                                InstanceIdentifier iid = kv.Key as InstanceIdentifier;
                                                if (iid.Indicator == InstanceIndicator.TYPE) {
                                                    IndexedMType targetType = indexedTypesByName[iid.Value];
                                                    int cCount = selectedInstance.ConnectionCount(targetType);
                                                    if (cCount > 0) {
                                                        ActiveInstance selectedTarget = selectedInstance.GetTargetAtIndex(rand.Next(cCount), targetType);
                                                        selectedTarget.CommunicatedMultiset = kv.Value.Multiset;
                                                        targetSelections.Add(selectedTarget);
                                                    } else {
                                                        isRuleApplicable = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (isRuleApplicable) {
                                            selectedInstance.Instance.Multiset.Subtract(rcr.Lhs);
                                            selectedInstance.Buffer.Add(rcr.Rhs);
                                            ruleApplied(selectedRule, selectedInstance.Instance);
                                            foreach (ActiveInstance ai in targetSelections) {
                                                if (step >= startStep) {
                                                    if (recordTargetSelection) {
                                                        if (TargetSelected != null) {
                                                            TargetSelected(ai.Instance, ai.Type, selectedRule);
                                                        }
                                                    }
                                                }
                                                ai.CommitCommunication();
                                            }

                                        } else {
                                            foreach (ActiveInstance ai in targetSelections) {
                                                ai.ResetCommunication();
                                            }
                                        }
                                        targetSelections.Clear();
                                    } break;
                                case RuleType.MULTISET_REWRITING: {
                                        ruleApplied(selectedRule, selectedInstance.Instance);
                                        selectedInstance.Instance.Multiset.Subtract((selectedRule as ConsumerRule).Lhs);
                                        selectedInstance.Buffer.Add((selectedRule as RewritingRule).Rhs);
                                    } break;
                                case RuleType.MEMBRANE_DISSOLUTION: {
                                        ruleApplied(selectedRule, selectedInstance.Instance);
                                        dissolutionRuleApplied = true;
                                        selectedInstance.FlagDissolved = true;
                                        selectedInstance.Instance.Multiset.Subtract((selectedRule as ConsumerRule).Lhs);
                                    } break;
                                case RuleType.LINK_CREATION: {                                        
                                    LinkRule lr = selectedRule as LinkRule;
                                    if (lr.Target is InstanceIdentifier) {
                                        string typeName = (lr.Target as InstanceIdentifier).Value;
                                        IndexedMType targetType = indexedTypesByName[typeName];
                                        ActiveInstance selectedTarget = null;
                                        //check if there is a link-free instance we can connect to
                                        int freeLinkCount = targetType.Instances.Count - selectedInstance.ConnectionCount(targetType);
                                        if (freeLinkCount > 0) {
                                            int selection = rand.Next(freeLinkCount);
                                            int i = 0;
                                            foreach (ActiveInstance ai in targetType.Instances) {
                                                if (!selectedInstance.IsConnectedTo(ai)) {
                                                    if (i++ == selection) {
                                                        selectedTarget = ai;
                                                    }
                                                }
                                            }

                                            selectedInstance.Instance.Multiset.Subtract(lr.Lhs);
                                            ruleApplied(selectedRule, selectedInstance.Instance);
                                            linkRuleApplied = true;
                                            //selected target can't be null here
                                            selectedInstance.ConnectionToCreate = selectedTarget;
                                            selectedInstance.FlagLinkRuleApplied = true;
                                            if (step >= startStep) {
                                                if (recordTargetSelection) {
                                                    if (TargetSelected != null) {
                                                        TargetSelected(selectedTarget.Instance, selectedTarget.Type, selectedRule);
                                                    }
                                                }
                                            }

                                            //a structure rule can only be applied once per step so remove it
                                            selectedInstance.ApplicableRules.Remove(selectedRule);

                                        } else {
                                            isRuleApplicable = false;
                                        }
                                    }
                                    
                                    
                                    } break;
                                case RuleType.LINK_DESTRUCTION: {
                                    LinkRule lr = selectedRule as LinkRule;
                                    if (lr.Target is InstanceIdentifier) {
                                        string typeName = (lr.Target as InstanceIdentifier).Value;
                                        IndexedMType targetType = indexedTypesByName[typeName];
                                        int cCount = selectedInstance.ConnectionCount(targetType);
                                        if (cCount > 0) {
                                            ActiveInstance selectedTarget = selectedInstance.GetTargetAtIndex(rand.Next(cCount), targetType);

                                            selectedInstance.Instance.Multiset.Subtract(lr.Lhs);
                                            ruleApplied(selectedRule, selectedInstance.Instance);
                                            linkRuleApplied = true;
                                            //selected target can't be null here
                                            selectedInstance.ConnectionToDestroy = selectedTarget;
                                            selectedInstance.FlagLinkRuleApplied = true;
                                            if (step >= startStep) {
                                                if (recordTargetSelection) {
                                                    if (TargetSelected != null) {
                                                        TargetSelected(selectedTarget.Instance, selectedTarget.Type, selectedRule);
                                                    }
                                                }
                                            }

                                            //a structure rule can only be applied once per step so remove it
                                            selectedInstance.ApplicableRules.Remove(selectedRule);

                                        } else {
                                            isRuleApplicable = false;
                                        }
                                    }
                                    } break;
                                case RuleType.MEMBRANE_DIVISION: {
                                    DivisionRule dr = selectedRule as DivisionRule;
                                    selectedInstance.Instance.Multiset.Subtract(dr.Lhs);
                                    foreach (InstanceBlueprint ib in dr.Rhs) {
                                        selectedInstance.AddInstanceBlueprint(ib, indexedTypesByName[ib.Type.Name]);
                                    }
                                    selectedInstance.FlagDivided = true;
                                    ruleApplied(selectedRule, selectedInstance.Instance);
                                    divisionRuleApplied = true;
                                    } break;
                            }
                        }


                        if (selectedInstance.FlagDissolved || selectedInstance.FlagDivided) {
                            allOperationalInstances.Remove(selectedInstance);
                            endStepForInstance = true;
                        } else {
                            switch (ex.Operator) {
                                case StrategyOperator.SEQUENCE: {
                                        if (isRuleApplicable) {
                                            selectedInstance.ApplicableRules.Remove(selectedRule);
                                        } else {
                                            endStepForInstance = true;
                                        }
                                    } break;
                                case StrategyOperator.MAX: {
                                        if (!isRuleApplicable) {
                                            selectedInstance.ApplicableRules.Remove(selectedRule);
                                        }
                                    } break;
                                case StrategyOperator.CHOICE: {
                                        if (isRuleApplicable) {
                                            advanceStrategyBlock = true;
                                        } else {
                                            selectedInstance.ApplicableRules.Remove(selectedRule);
                                        }
                                    } break;
                                case StrategyOperator.ARBITRARY: {
                                        if (!isRuleApplicable) {
                                            if (selectedRule == null) {
                                                advanceStrategyBlock = true;
                                            } else {
                                                selectedInstance.ApplicableRules.Remove(selectedRule);
                                            }
                                        }
                                    } break;
                            }
                        }
                    }

                    if (endStepForInstance) {
                        activeInstances.Remove(selectedInstance);
                    } else {
                        if (selectedInstance.ApplicableRules.Count == 0) {
                            advanceStrategyBlock = true;
                        }

                        if (advanceStrategyBlock) {
                            ex = ex.Next;

                            //if the next execution block is null, then remove this compartment from the active instance array
                            if (ex == null) {
                                activeInstances.Remove(selectedInstance);
                            } else {
                                selectedInstance.CurrentStrategySegment = ex;
                            }
                        }
                    }

                    aiCount = activeInstances.Count;
                }

                if (rulesApplied == 0) {
                    isHalted = true;
                } else {
                    //Clear buffer for all instances;

                    foreach (ActiveInstance instance in allInstances) {
                        instance.CommitBuffer();

                        if (linkRuleApplied) {
                            if (instance.FlagLinkRuleApplied) {
                                instance.CommitConnectionUpdates();
                            }
                        }
                    }

                    if (dissolutionRuleApplied || divisionRuleApplied) {
                        foreach (ActiveInstance instance in allInstances) {
                            if (instance.FlagDissolved) {
                                instance.Instance.IsDissolved = true;
                                deadCells.Add(instance);
                                instance.CommitDissolution();
                            } else if (instance.FlagDivided) {
                                instance.Instance.IsDivided = true;
                                deadCells.Add(instance);
                                instance.CommitNewInstances(onNewInstanceCreated);
                            }
                        }

                        if (divisionRuleApplied) {
                            foreach (ActiveInstance ai in newInstanceSet) {
                                allInstances.Add(ai);
                            }
                            newInstanceSet.Clear();
                        }
                    }

                    foreach (ActiveInstance instance in deadCells) {
                        //remove from allInstances collection such that a dead instance will never be 
                        //considered for updates or any sort of operations
                        allInstances.Remove(instance);
                        //remove this instance from the set of instances associated to its type so 
                        //it will never be considered for link creation or destruction
                        instance.IndexedMType.Instances.Remove(instance);
                    }

                    if (deadCells.Count > 0) {
                        deadCells.Clear();
                    }

                    //reset execution strategy to begining
                    //it's important this is done after everthing has been commited, because the guards must be evaluated on the updated multiset
                    //in the instance
                    foreach (ActiveInstance ai in allOperationalInstances) {
                        ai.ResetCurrentStrategySegment();
                    }

                    if (step >= startStep) {
                        if (SimulationParams.RecordConfigurations) {
                            if (StepComplete != null) {
                                simStep.Step = step;
                                StepComplete(simStep);
                            }
                        }
                    }
                    ++step;
                }
            }

            --step;
            if (IsHalted() && SystemHalted != null) {
                SystemHalted(step);
            }
            if (SimulationComplete != null) {
                SimulationComplete(step);
            }

            return kp;
        }

        public bool IsHalted() {
            return isHalted;
        }

        public void Reset() {
            isHalted = false;
            step = 1;
            resetParamCache();
            if (seed > 0) {
                rand = new Random(seed);
            } else {
                rand = new Random();
            }
        }

        public void Reset(KPsystem newModel) {
            Reset();
            kp = newModel;

            allInstances.Clear();
            allOperationalInstances.Clear();
            indexedTypes.Clear();

            int i = 0;
            foreach (MType mtype in kp.Types) {
                IndexedMType imt = new IndexedMType(mtype, i++);
                indexedTypes.Add(imt);
                indexedTypesByName.Add(mtype.Name, imt);

                ExecutionStrategy ex = mtype.ExecutionStrategy;
                while (ex != null && ex.Rules.Count == 0) {
                    ex = ex.Next;
                }
                foreach (MInstance instance in mtype.Instances) {
                    ActiveInstance activeInstance = new ActiveInstance(imt, instance, kp);
                    allInstances.Add(activeInstance);
                    aiMapping.Add(instance, activeInstance);
                    if (ex != null) {
                        allOperationalInstances.Add(activeInstance);
                        activeInstance.CurrentStrategySegment = ex;
                    }
                }
            }

            foreach (ActiveInstance ai in allInstances) {
                ai.GenerateConnections(aiMapping);
                //set execution strategy to begining
                ai.ResetCurrentStrategySegment();
            }
        }

        private void resetParamCache() {
            startStep = sParams.SkipSteps + 1;
            endStep = SimulationParams.Steps + startStep - 1;
            seed = SimulationParams.Seed;

            recordRuleSelection = SimulationParams.RecordRuleSelection;
            recordTargetSelection = SimulationParams.RecordTargetSelection;
            recordInstanceCreation = SimulationParams.RecordInstanceCreation;
        }

        private void ruleApplied(Rule r, MInstance instance) {
            if (step >= startStep) {
                if (rulesApplied == 0) {
                    if (ReachedStep != null) {
                        ReachedStep(step);
                    }
                }

                if (recordRuleSelection) {
                    if (RuleApplied != null) {
                        RuleApplied(r, instance);
                    }
                }
            }
            ++rulesApplied;
        }

        private void onNewInstanceCreated(ActiveInstance activeInstance) {
            newInstanceSet.Add(activeInstance);
            ExecutionStrategy ex = activeInstance.IndexedMType.Type.ExecutionStrategy;
            while (ex != null && ex.Rules.Count == 0) {
                ex = ex.Next;
            }

            if (ex != null) {
                allOperationalInstances.Add(activeInstance);
            }

            if (RegisterNewInstance != null) {
                RegisterNewInstance(activeInstance.Instance);
            }

            if (step >= startStep) {
                if (recordInstanceCreation) {
                    if (NewInstanceCreated != null) {
                        NewInstanceCreated(activeInstance.Instance, activeInstance.Type);
                    }
                }
            }
        }
    }

    internal class ActiveInstance : TypedInstance {

        public delegate void NewActiveInstanceCreated(ActiveInstance instance);

        private ExecutionStrategy current;
        public ExecutionStrategy CurrentStrategySegment {
            get { return current; }
            set {
                current = value;
                applicableRules.Clear();
                if (current.Operator == StrategyOperator.SEQUENCE) {
                    applicableRules.AddRange(value.Rules);
                } else {
                    //only add applicable rules to the set, that is guard applicable, check for other constraints at each execution
                    //Guards however are evaluated once only, upon entry in repetitive (or CHOICE) block
                    foreach (Rule r in current.Rules) {
                        if (r.IsGuarded) {
                            if (!r.Guard.IsSatisfiedBy(Instance.Multiset)) {
                                continue;
                            }
                        }
                        applicableRules.Add(r);
                    }
                }
            }
        }

        private Multiset buffer;

        private KPsystem kp;

        public Multiset Buffer { get { return buffer; } set { buffer = value; } }

        private IndexedMType indexedType;
        public IndexedMType IndexedMType {
            get { return indexedType; }
            set {
                if (indexedType != null) {
                    indexedType.Instances.Remove(this);
                }
                indexedType = value; 
                if (indexedType != null) {
                    indexedType.Instances.Add(this);
                }
            }
        }

        public int TypeIndex { get { return indexedType.Index; } }

        private List<Rule> applicableRules;
        public List<Rule> ApplicableRules { get { return applicableRules; } private set { applicableRules = value; } }

        //the list of connections by type (represented by its index in the outer list)
        private List<ActiveInstance>[] connections;

        public Multiset CommunicatedMultiset { get; set; }

        public ActiveInstance ConnectionToCreate { get; set; }
        public ActiveInstance ConnectionToDestroy { get; set; }

        private HashSet<ActiveInstanceBlueprint> newInstances;
        public bool FlagDissolved { get; set; }
        public bool FlagDivided { get; set; }
        public bool FlagLinkRuleApplied { get; set; }
        public bool FlagStructureRuleApplied { get { return FlagDissolved || FlagDivided || FlagLinkRuleApplied; } }

        public ActiveInstance(IndexedMType itype, MInstance instance, KPsystem kp)
            : base(itype.Type, instance) {
            Buffer = new Multiset();
            applicableRules = new List<Rule>();
            this.kp = kp;
            IndexedMType = itype;
        }

        public void ResetCurrentStrategySegment() {
            CurrentStrategySegment = Type.ExecutionStrategy;
        }

        public void GenerateConnections(Dictionary<MInstance, ActiveInstance> instanceMapping) {
            //init connections array
            connections = new List<ActiveInstance>[kp.Types.Count];

            foreach (MInstance instance in Instance.Connections) {
                ActiveInstance ai = null;
                instanceMapping.TryGetValue(instance, out ai);
                if (ai != null) {
                    List<ActiveInstance> ail = connections[ai.TypeIndex];
                    if (ail == null) {
                        ail = new List<ActiveInstance>();
                        connections[ai.TypeIndex] = ail;
                    }
                    ail.Add(ai);
                }
            }
        }

        public int ConnectionCount(IndexedMType imt) {
            return connections[imt.Index] == null ? 0 : connections[imt.Index].Count;
        }


        /// <summary>
        /// No checks here, the method assumes parameters are non-violent, that is a valid active instance exists as a connection 
        /// at the specified index in the array of that type;
        /// </summary>
        /// <param name="index"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActiveInstance GetTargetAtIndex(int index, IndexedMType type) {
            return connections[type.Index][index];
        }

        public void CommitCommunication() {
            if (CommunicatedMultiset != null) {
                Buffer.Add(CommunicatedMultiset);
                CommunicatedMultiset = null;
            }
        }

        public void ResetCommunication() {
            CommunicatedMultiset = null;
        }

        /// <summary>
        /// Note: This method does not enforce the one link rule (one structure rule in fact) per step restriction attributed to Kernel P systems.
        /// This must be inforced outside the active instance.
        /// </summary>
        public void CommitConnectionUpdates() {
            if (ConnectionToCreate != null) {
                AddConnectionTo(ConnectionToCreate);
                ConnectionToCreate = null;
            }
            if (ConnectionToDestroy != null) {
                RemoveConnectionTo(ConnectionToDestroy);
                ConnectionToDestroy = null;
            }
            FlagLinkRuleApplied = false;
        }

        public void AddConnectionTo(ActiveInstance ai) {
            List<ActiveInstance> ail = connections[ai.IndexedMType.Index];
            if (ail == null) {
                ail = new List<ActiveInstance>();
                connections[ai.IndexedMType.Index] = ail;
            }
            ail.Add(ai);
            Instance.ConnectBidirectionalTo(ai.Instance);
            ail = ai.connections[IndexedMType.Index];
            if(ail == null) {
                ail = new List<ActiveInstance>();
                ai.connections[IndexedMType.Index] = ail;
            }
            ail.Add(this);
        }

        /// <summary>
        /// Do no use these methods when iterating on connections lists of other active membranes
        /// </summary>
        /// <param name="ai"></param>
        public void RemoveConnectionTo(ActiveInstance ai) {
            connections[ai.IndexedMType.Index].Remove(ai);
            Instance.DisconnectBidirectionalFrom(ai.Instance);
            ai.connections[IndexedMType.Index].Remove(this);
        }

        /// <summary>
        /// These are unidirectional methods, should be used with caution otherwise they will cause chaos
        /// </summary>
        /// <param name="ai"></param>
        private void disconnectFrom(ActiveInstance ai) {
            List<ActiveInstance> ail = connections[ai.IndexedMType.Index];
            if (ail != null) {
                ail.Remove(ai);
            }
        }

        private void connectTo(ActiveInstance ai) {
            List<ActiveInstance> ail = connections[ai.IndexedMType.Index];
            if (ail == null) {
                ail = new List<ActiveInstance>();
                connections[ai.IndexedMType.Index] = ail;
            }
            ail.Add(ai);
        }

        public void CommitBuffer() {
            Instance.Multiset.Add(Buffer);
            Buffer.Clear();
        }

        public bool IsConnectedTo(ActiveInstance ai) {
            List<ActiveInstance> ail = connections[ai.IndexedMType.Index];
            if (ail == null) {
                return false;
            }

            return ail.Contains(ai);
        }

        public void CommitDissolution() {
            //remove connections to this active instance
            for (int i = 0; i < connections.Length; i++) {
                List<ActiveInstance> ail = connections[i];
                if (ail != null) {
                    foreach (ActiveInstance connection in ail) {
                        Instance.DisconnectBidirectionalFrom(connection.Instance);
                        connection.disconnectFrom(this);
                    }
                    ail.Clear();
                }
            }
        }

        public void AddInstanceBlueprint(InstanceBlueprint ib, IndexedMType imt) {
            if (newInstances == null) {
                newInstances = new HashSet<ActiveInstanceBlueprint>();
            }
            newInstances.Add(new ActiveInstanceBlueprint(ib, imt));
        }

        public void CommitNewInstances(NewActiveInstanceCreated activeInstanceCreated) {
            foreach (ActiveInstanceBlueprint aib in newInstances) {
                MInstance newInstance = new MInstance();

                //add existing multiset. At this point everything in the Buffer should be commited according to
                //the kP system semantics
                newInstance.Multiset.Add(Instance.Multiset);
                newInstance.Multiset.Add(aib.InstanceBlueprint.Multiset);
                newInstance.IsCreated = true;

                //add the new instance to the KPsystem
                aib.IndexedMType.Type.Instances.Add(newInstance);
                //create the Active instance wrapper and register it to its appropriate type
                ActiveInstance ai = new ActiveInstance(aib.IndexedMType, newInstance, kp);
                ai.connections = new List<ActiveInstance>[kp.Types.Count];
                aib.IndexedMType.Instances.Add(ai);

                //remove connection from this instance and add them to the new active instance and inner MInstance
                for (int i = 0; i < connections.Length; i++) {
                    List<ActiveInstance> ail = connections[i];
                    if (ail != null) {
                        foreach (ActiveInstance connection in ail) {
                            connection.disconnectFrom(this);
                            Instance.DisconnectBidirectionalFrom(connection.Instance);
                            newInstance.ConnectBidirectionalTo(connection.Instance);
                            ai.connectTo(connection);
                            connection.connectTo(ai);
                        }
                    }
                }
                if (activeInstanceCreated != null) {
                    activeInstanceCreated(ai);
                }
            }
            //clear all connections from this instance
            for (int i = 0; i < connections.Length; i++) {
                List<ActiveInstance> ail = connections[i];
                if (ail != null) {
                    ail.Clear();
                }
            }
            newInstances.Clear();
            //will need to add each newly created Active instance to the allOperationalInstances and allInstances Lists outside this
            //method
        }
        
    }

    internal class IndexedMType {
        public MType Type { get; set; }
        public int Index { get; set; }
        public HashSet<ActiveInstance> Instances { get; set; }

        public IndexedMType(MType type, int index) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            Type = type;
            Index = index;
            Instances = new HashSet<ActiveInstance>();
        }
    }

    internal class ActiveInstanceBlueprint {
        public InstanceBlueprint InstanceBlueprint { get; set; }
        public IndexedMType IndexedMType { get; set; }

        public ActiveInstanceBlueprint(InstanceBlueprint ib, IndexedMType imt) {
            InstanceBlueprint = ib;
            IndexedMType = imt;
        }
    }
}
