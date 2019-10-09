using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin.SpinVerificationModel {
    public class KpMeta {

        public bool HasLinkCreation { get; set; }
        public bool HasLinkDestruction { get; set; }
        public bool HasLinkRules { get { return HasLinkCreation || HasLinkDestruction; } }
        public bool HasDivision { get; set; }
        public bool HasDissolution { get; set; }

        public bool HasStructureChangingRules {
            get {
                return HasLinkRules || HasDissolution || HasDivision;
            }
        }

        public bool HasCommunication { get; set; }

        private HashSet<MInstance> allInstances;
        public HashSet<MInstance> AllInstance { get { return allInstances; } private set { allInstances = value; } }
        public int InstanceCount { get { return allInstances.Count; } }

        private Alphabet a;
        public Alphabet Alphabet { get { return a; } private set { a = value; } }

        private Dictionary<MInstance, MType> instanceTypes;

        private KpIdMap idMap;
        public KpIdMap IdMap { get { return idMap; } private set { idMap = value; } }

        private KPsystem kp;
        public KPsystem KPsystem {
            get { return kp; }
            set {
                if (value == null) {
                    throw new ArgumentNullException();
                } else if (value != kp) {
                    kp = value;
                    Build();
                }
            }
        }

        private int[] growthPerType;

        /// <summary>
        /// A KpMeta object assumes the KPsystem it wraps around is immutable
        /// </summary>
        /// <param name="kps"></param>
        public KpMeta(KPsystem kps) {
            //the assignment automatically triggers the building process
            KPsystem = kps;
        }

        private void Build() {
            a = new Alphabet();
            allInstances = new HashSet<MInstance>();
            instanceTypes = new Dictionary<MInstance, MType>();

            idMap = new KpIdMap(kp);

            growthPerType = new int[kp.Types.Count];
            foreach (MType mtype in kp.Types) {

                foreach (MInstance instance in mtype.Instances) {
                    registerSymbols(instance.Multiset);
                    allInstances.Add(instance);
                    instanceTypes.Add(instance, mtype);
                }

                ExecutionStrategy ex = mtype.ExecutionStrategy;
                while (ex != null) {
                    foreach (Rule r in ex.Rules) {
                        if (r is ConsumerRule) {
                            registerSymbols((r as ConsumerRule).Lhs);

                            if (r.Type == RuleType.MULTISET_REWRITING) {
                                registerSymbols((r as RewritingRule).Rhs);
                            } else if (r.Type == RuleType.REWRITE_COMMUNICATION) {
                                RewriteCommunicationRule rcr = r as RewriteCommunicationRule;
                                registerSymbols(rcr.Rhs);
                                foreach (TargetedMultiset tm in rcr.TargetRhs.Values) {
                                    registerSymbols(tm.Multiset);
                                }
                                HasCommunication = true;
                            } else if (r.Type == RuleType.LINK_CREATION) {
                                HasLinkCreation = true;
                            } else if (r.Type == RuleType.LINK_DESTRUCTION) {
                                HasLinkDestruction = true;
                            } else if (r.Type == RuleType.MEMBRANE_DIVISION) {
                                HasDivision = true;
                                foreach (InstanceBlueprint ib in (r as DivisionRule).Rhs) {
                                    registerSymbols(ib.Multiset);
                                    growthPerType[ib.Type.Id]++;
                                }
                            } else if (r.Type == RuleType.MEMBRANE_DISSOLUTION) {
                                HasDissolution = true;
                            }

                            if (r.IsGuarded) {
                                registerSymbols(r.Guard);
                            }
                        }
                    }
                    ex = ex.Next;
                }
            }
        }

        private void registerSymbols(Multiset ms) {
            foreach (string symbol in ms.Objects) {
                a.AddSymbolIfNotExists(symbol);
            }
        }

        private void registerSymbols(IGuard g) {
            if (g is BasicGuard) {
                registerSymbols((g as BasicGuard).Multiset);
            } else if (g is NegatedGuard) {
                registerSymbols((g as NegatedGuard).Operand);
            } else if (g is CompoundGuard) {
                registerSymbols((g as CompoundGuard).Lhs);
                registerSymbols((g as CompoundGuard).Rhs);
            }
        }

        public MType GetInstanceType(MInstance instance) {
            MType type = null;
            instanceTypes.TryGetValue(instance, out type);
            return type;
        }

        public int GetGrowthRate(MType type) {
            return growthPerType[type.Id];
        }
    }

    public class Alphabet {
        private Dictionary<int, string> symbolIds;
        private Dictionary<string, int> symbols;
        private int sid;

        public int Count {
            get {
                return symbols.Count;
            }
        }

        public Dictionary<string, int>.KeyCollection Symbols {
            get {
                return symbols.Keys;
            }
        }

        public Alphabet() {
            symbolIds = new Dictionary<int, string>();
            symbols = new Dictionary<string, int>();
            sid = 0;
        }

        public string this[int id] {
            get {
                return Symbol(id);
            }
        }

        public int this[string s] {
            get {
                return SymbolId(s);
            }
        }

        public string Symbol(int symbolId) {
            return symbolIds[symbolId];
        }

        public int SymbolId(string symbol) {
            return symbols[symbol];
        }

        public void AddSymbol(string symbol) {
            symbolIds.Add(sid, symbol);
            symbols.Add(symbol, sid++);
        }

        public void AddSymbolIfNotExists(string symbol) {
            if (!symbols.ContainsKey(symbol)) {
                AddSymbol(symbol);
            }
        }

        public bool RemoveSymbol(string symbol) {
            int sID = 0;
            if (symbols.TryGetValue(symbol, out sID)) {
                symbolIds.Remove(sID);
                symbols.Remove(symbol);
                return true;
            }

            return false;
        }

        public void Clear() {
            symbols.Clear();
            symbolIds.Clear();
            sid = 0;
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator() {
            return symbols.GetEnumerator();
        }
    }
}
