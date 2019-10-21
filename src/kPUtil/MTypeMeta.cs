using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class MTypeMeta {

        private MType mtype;
        public MType MType { get { return mtype; } private set { mtype = value; } }

        private KpMetaModel kpmm;
        public KpMetaModel KpMetaModel { get { return kpmm; } protected set { kpmm = value; } }
        public KPsystem KPsystem { get { return kpmm.KPsystem; } }

        public bool HasCommunication { get; private set; }
        public bool HasDivision { get; private set; }
        public bool HasDissolution { get; private set; }
        public bool HasLinkCreation { get; private set; }
        public bool HasLinkDestruction { get; private set; }
        public bool HasLinkOps { get { return HasLinkCreation || HasLinkDestruction; } }
        public bool HasStructureChangingRules { get { return HasDissolution || HasDivision || HasLinkOps; } }
        public int MaxLinks { get; private set; }
        /// <summary>
        /// The maximum number of compartments of this type, that can be created per step by all types in the P system. This has to be multiplied
        /// with the number of instances which are capable (i.e. of the type) of producing new compartments of this type;
        /// </summary>
        public int MaxDivisionRate { get; private set; }

        private int id;
        public int Id { get { return id; } private set { id = value; } }

        private Dictionary<int, MInstanceMeta> iMeta;
        public Dictionary<int, MInstanceMeta> MInstanceMeta { get { return iMeta; } private set { iMeta = value; } }
        public Dictionary<int, MInstanceMeta>.ValueCollection MInstanceMetaSet { get { return iMeta.Values; } }
        private MInstance[] instances;
        public MInstance[] Instances { get { return instances; } }

        private Dictionary<int, RuleMeta> rMeta;
        public Dictionary<int, RuleMeta> RuleMeta { get { return rMeta; } private set { rMeta = value; } }
        private Rule[] rules;
        public Rule[] RuleSet { get { return rules; } }

        private Dictionary<int, Symbol> alphabet;
        private Dictionary<string, Symbol> symbols;

        private Dictionary<MTypeMeta, int> connections;
        public Dictionary<MTypeMeta, int> Connections { get { return connections; } private set { connections = value; } }

        private HashSet<MTypeMeta> divisibleTypes;
        public HashSet<MTypeMeta> DivisibleTypes { get { return divisibleTypes; } private set { divisibleTypes = value; } }

        private HashSet<MTypeMeta> dynasty;
        public HashSet<MTypeMeta> Dynasty { get { return dynasty; } private set { dynasty = value; } }

        private HashSet<MTypeMeta> mayConnectTo;
        public HashSet<MTypeMeta> MayConnectTo { get { return mayConnectTo; } private set { mayConnectTo = value; } }

        private Dictionary<MTypeMeta, int> hasLinksTo;
        public Dictionary<MTypeMeta, int> HasLinksTo { get { return hasLinksTo; } private set { hasLinksTo = value; } }

        private HashSet<MTypeMeta> interests;
        public HashSet<MTypeMeta> IsInterestedIn { get { return interests; } private set { interests = value; } }

        public Dictionary<string, Symbol>.KeyCollection Alphabet { get { return symbols.Keys; } }
        public int AlphabetSize { get { return alphabet.Count; } }
        public Dictionary<string, Symbol>.ValueCollection Symbols { get { return symbols.Values; } }

        public bool IsEmpty { get { return (Instances.Length == 0 || (AlphabetSize == 0 && RuleSet.Length == 0)) && MaxDivisionRate == 0; } }

        private int instanceIdgen = 0;
        private int ruleIdgen = 0;
        private int symbolIdgen = 0;

        public MTypeMeta(KpMetaModel kpMeta, MType mt, int id, bool autobuild = true) {
            Id = id;
            mtype = mt;
            kpmm = kpMeta;

            iMeta = new Dictionary<int, MInstanceMeta>();
            rMeta = new Dictionary<int, RuleMeta>();
            alphabet = new Dictionary<int, Symbol>();
            symbols = new Dictionary<string, Symbol>();

            connections = new Dictionary<MTypeMeta, int>();
            divisibleTypes = new HashSet<MTypeMeta>();
            dynasty = new HashSet<MTypeMeta>();
            mayConnectTo = new HashSet<MTypeMeta>();
            hasLinksTo = new Dictionary<MTypeMeta, int>();
            interests = new HashSet<MTypeMeta>();

            if (autobuild) {
                build();
            }
        }

        internal void build() {
            int id;
            MaxLinks = 0;
            MaxDivisionRate = 0;
            foreach (MInstance instance in mtype.Instances) {
                id = nextInstanceId();
                MInstanceMeta im = new MInstanceMeta(this, instance, id);
                iMeta.Add(id, im);
                kpmm.InstanceRegistry.Add(instance, im);
                AddSymbols(instance.Multiset.Objects);
                if (instance.Connections.Count > MaxLinks) {
                    MaxLinks = instance.Connections.Count;
                }
            }
            instances = mtype.Instances.ToArray();

            List<Rule> rs = new List<Rule>();
            ExecutionStrategy strategy = mtype.ExecutionStrategy;
            while (strategy != null) {
                foreach (Rule r in strategy.Rules) {
                    id = nextRuleId();
                    RuleMeta rm = new RuleMeta(this, r, id);
                    rMeta.Add(id, rm);
                    rs.Add(r);
                    kpmm.RuleRegistry[r] = rm;
                    if (r.IsGuarded) {
                        extractGuardAlphabet(r.Guard);
                    }

                    switch (r.Type) {
                        case RuleType.MULTISET_REWRITING: {
                                AddSymbols((r as RewritingRule).Lhs.Objects);
                                AddSymbols((r as RewritingRule).Rhs.Objects);
                            } break;
                        case RuleType.REWRITE_COMMUNICATION: {
                                RewriteCommunicationRule rc = r as RewriteCommunicationRule;
                                AddSymbols(rc.Lhs.Objects);
                                AddSymbols(rc.Rhs.Objects);
                                foreach (TargetedMultiset tarm in rc.TargetRhs.Values) {
                                    if (tarm.Target is InstanceIdentifier) {
                                        InstanceIdentifier iId = tarm.Target as InstanceIdentifier;
                                        if (iId.Indicator == InstanceIndicator.TYPE) {
                                            AddSymbols(tarm.Multiset.Objects, iId.Value);
                                            //add interest in this connection
                                            interests.Add(kpmm.GetTypeMetaByName(iId.Value, true));
                                        }
                                    }
                                }
                            } break;
                        case RuleType.MEMBRANE_DIVISION: {
                                DivisionRule dr = r as DivisionRule;
                                AddSymbols(dr.Lhs.Objects);
                                foreach (InstanceBlueprint ib in dr.Rhs) {
                                    AddSymbols(ib.Multiset.Objects, ib.Type.Name);
                                    if (ib.Type == MType) {
                                        MaxDivisionRate++;
                                        divisibleTypes.Add(this);
                                    } else {
                                        kpmm.GetTypeMetaByName(ib.Type.Name).MaxDivisionRate++;
                                        divisibleTypes.Add(kpmm.GetTypeMetaByType(ib.Type));
                                    }
                                }
                            } break;
                        default: {
                                if (r is LinkRule) {
                                    LinkRule lr = r as LinkRule;
                                    AddSymbols(lr.Lhs.Objects);
                                    if (lr.Target is InstanceIdentifier) {
                                        InstanceIdentifier iid = lr.Target as InstanceIdentifier;
                                        if (iid.Indicator == InstanceIndicator.TYPE) {
                                            MTypeMeta mtm = kpmm.GetTypeMetaByName(iid.Value, true);
                                            interests.Add(mtm);
                                            if (lr.Type == RuleType.LINK_CREATION) {
                                                mayConnectTo.Add(mtm);
                                            }
                                        }
                                    }
                                } else if (r is ConsumerRule) {
                                    AddSymbols((r as ConsumerRule).Lhs.Objects);
                                }
                            } break;
                    }
                    HasCommunication = HasCommunication || r.Type == RuleType.REWRITE_COMMUNICATION;
                    HasDivision = HasDivision || r.Type == RuleType.MEMBRANE_DIVISION;
                    HasDissolution = HasDissolution || r.Type == RuleType.MEMBRANE_DISSOLUTION;
                    HasLinkCreation = HasLinkCreation || r.Type == RuleType.LINK_CREATION;
                    HasLinkDestruction = HasLinkDestruction || r.Type == RuleType.LINK_DESTRUCTION;
                }
                strategy = strategy.Next;
            }
            rules = rs.ToArray();
        }

        internal void postBuild() {

            buildDynasty(this);

            foreach (MInstance mi in instances) {
                foreach (MInstance miConnection in mi.Connections) {
                    MInstanceMeta mim = kpmm.InstanceRegistry[miConnection];
                    MTypeMeta mtm = mim.MTypeMeta;
                    int count = 0;
                    HasLinksTo.TryGetValue(mtm, out count);
                    HasLinksTo[mtm] = count + 1;
                }
            }

            if (HasLinkOps) {
                kpmm.AreLinksNecessary = true;
            }

            foreach (MTypeMeta mtm in IsInterestedIn) {
                if (MayConnectTo.Contains(mtm)) {
                    if (mtm.HasDivision) {
                        Connections[mtm] = -1;
                    } else {
                        Connections[mtm] = mtm.Instances.Length;
                    }
                } else {
                    int x = 0;
                    HasLinksTo.TryGetValue(mtm, out x);

                    if (x > 0) {
                        if (mtm.HasDivision) {
                            Connections[mtm] = -1;
                        } else {
                            Connections[mtm] = x;
                        }
                    }
                }

                //if the type has more than one instance, then mark the requirement of links
                //links should be considered when they have an effect in the choice of a target for a communication rule
                //or a link rule
                if (mtm.Instances.Length > 1) {
                    kpmm.AreLinksNecessary = true;
                }
            }

            foreach (MTypeMeta mtm in HasLinksTo.Keys) {
                if (!IsInterestedIn.Contains(mtm)) {
                    if (mtm.IsInDynasty(this)) {
                        Connections[mtm] = -1;
                    }

                    if (mtm.DinastyRequiresType(this)) {
                        Connections[mtm] = -1;
                    }
                }
            }
        }

        private void buildDynasty(MTypeMeta typeMeta) {
            foreach (MTypeMeta mtm in typeMeta.divisibleTypes) {
                if (dynasty.Add(mtm)) {
                    mtm.AddSymbols(typeMeta.Alphabet);
                    buildDynasty(mtm);
                }
            }
        }

        internal bool IsInDynasty(MTypeMeta typeMeta) {
            if (this.HasDivision) {
                if (dynasty.Contains(typeMeta)) {
                    return true;
                }
            }

            return false;
        }

        internal bool DinastyRequiresType(MTypeMeta typeMeta) {
            if (this.HasDivision) {
                foreach (MTypeMeta mtm in dynasty) {
                    //if this is a desired connection
                    if (mtm.IsInterestedIn.Contains(typeMeta)) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void extractGuardAlphabet(IGuard g) {
            if (g == null) {
                return;
            }

            if (g is BasicGuard) {
                AddSymbols((g as BasicGuard).Multiset.Objects);
            } else if (g is NegatedGuard) {
                extractGuardAlphabet((g as NegatedGuard).Operand);
            } else if (g is CompoundGuard) {
                extractGuardAlphabet((g as CompoundGuard).Lhs);
                extractGuardAlphabet((g as CompoundGuard).Rhs);
            }
        }

        public Symbol Symbol(int id) {
            Symbol s = null;
            alphabet.TryGetValue(id, out s);
            return s;
        }

        public Symbol Symbol(string name) {
            Symbol s = null;
            symbols.TryGetValue(name, out s);
            return s;
        }

        public int SymbolId(string name) {
            return symbols[name].Id;
        }

        internal Symbol AddSymbol(string name) {
            Symbol s = null;
            if (!symbols.TryGetValue(name, out s)) {
                int id = nextSymbolId();
                s = new Symbol(name, id);
                symbols.Add(name, s);
                alphabet.Add(id, s);
            }

            return s;
        }

        private void AddSymbols(IEnumerable<string> symbolNames, string mtypeName) {
            MTypeMeta mtm = kpmm.GetTypeMetaByName(mtypeName, true);
            foreach (string symbol in symbolNames) {
                mtm.AddSymbol(symbol);
            }
        }

        private void AddSymbols(IEnumerable<string> symbolNames) {
            foreach (string symbol in symbolNames) {
                AddSymbol(symbol);
            }
        }

        private int nextInstanceId() {
            return instanceIdgen++;
        }

        private int nextRuleId() {
            return ruleIdgen++;
        }

        private int nextSymbolId() {
            return symbolIdgen++;
        }
    }
}
