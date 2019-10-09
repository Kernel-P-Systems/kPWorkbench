using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class KpMetaModel {

        public bool HasCommunication { get; private set; }
        public bool HasDivision { get; private set; }
        public bool HasDissolution { get; private set; }
        public bool HasLinkCreation { get; private set; }
        public bool HasLinkDestruction { get; private set; }
        public bool HasLinkOps { get { return HasLinkCreation || HasLinkDestruction; } }
        public bool AreLinksNecessary { get; internal set; }

        public int InstanceCount { get; private set; }
        public int RuleCount { get; private set; }
        public int MTypeCount { get; private set; }

        private Dictionary<int, MTypeMeta> tm;
        private Dictionary<MType, MTypeMeta> typeMeta;

        private Dictionary<MInstance, MInstanceMeta> instanceRegistry;
        public Dictionary<MInstance, MInstanceMeta> InstanceRegistry { get { return instanceRegistry; } }

        private Dictionary<Rule, RuleMeta> ruleRegistry;
        public Dictionary<Rule, RuleMeta> RuleRegistry { get { return ruleRegistry; } }

        private Dictionary<string, Symbol> symbols;
        private Dictionary<int, Symbol> alphabet;
        private int symbolCount;

        public int AlphabetSize { get { return alphabet.Count; } }
        public Symbol[] Alphabet { get { return symbols.Values.ToArray();  } }

        public IEnumerable<MTypeMeta> TypeMeta { get { return typeMeta.Values; } }

        private KPsystem src;
        public KPsystem KPsystem { get { return src; } private set { src = value; } }

        private int typeIdgen = 0;

        public KpMetaModel(KPsystem kp)
            : this(kp, true) {
        }

        public KpMetaModel(KPsystem kp, bool build) {
            KPsystem = kp;

            tm = new Dictionary<int, MTypeMeta>();
            typeMeta = new Dictionary<MType, MTypeMeta>();
            instanceRegistry = new Dictionary<MInstance, MInstanceMeta>();
            ruleRegistry = new Dictionary<Rule, RuleMeta>();
            alphabet = new Dictionary<int, Symbol>();
            symbols = new Dictionary<string, Symbol>();
            symbolCount = 0;

            AreLinksNecessary = false;

            if (build) {
                Build();
            }
        }

        public MTypeMeta GetTypeMetaById(int id) {
            return tm[id];
        }

        public MTypeMeta GetTypeMetaByName(string typeName, bool createIfNotExists = false) {
            MType mt = src[typeName];
            if (mt == null) {
                return null;
            }
            MTypeMeta mtm = null;
            if (typeMeta.TryGetValue(mt, out mtm)) {
                return mtm;
            } else {
                if (createIfNotExists) {
                    return create(mt);
                }
            }
            return null;
        }

        public MTypeMeta GetTypeMetaByType(MType type, bool createIfNotExists = false) {
            if (type == null) {
                return null;
            }
            MTypeMeta mtm = null;
            if (typeMeta.TryGetValue(type, out mtm)) {
                return mtm;
            } else {
                if (createIfNotExists) {
                    return create(type);
                }
            }
            return null;
        }

        public MType GetTypeByName(string typeName) {
            return src[typeName];
        }

        public void Build() {
            if (src == null) {
                throw new KPsystemNullException();
            }

            tm.Clear();
            typeMeta.Clear();
            instanceRegistry.Clear();

            MType[] mtypes = new MType[src.Types.Count];
            src.Types.CopyTo(mtypes, 0);

            foreach (MType mtype in mtypes) {
                create(mtype);
            }

            foreach (MTypeMeta mtypeMeta in tm.Values) {
                mtypeMeta.postBuild();
            }

            //extract system alphabet
            foreach (MTypeMeta mtm in tm.Values) {
                foreach (string item in mtm.Alphabet) {
                    if (!symbols.ContainsKey(item)) {
                        Symbol s = new Symbol(item, symbolCount);
                        symbols.Add(item, s);
                        alphabet.Add(symbolCount++, s);
                    }
                }
            }
        }

        public MInstanceMeta GetInstanceMeta(MInstance instance) {
            return instanceRegistry[instance];
        }

        public RuleMeta GetRuleMeta(Rule r) {
            return ruleRegistry[r];
        }

        public Symbol GetSymbolByName(string symbolName) {
            Symbol x = null;
            symbols.TryGetValue(symbolName, out x);
            return x;
        }

        private MTypeMeta create(MType mtype) {
            MTypeMeta mtm;

            if (typeMeta.TryGetValue(mtype, out mtm)) {
                return mtm;
            }

            int id = nextTypeId();
            mtm = new MTypeMeta(this, mtype, id, false);
            tm.Add(id, mtm);
            typeMeta.Add(mtype, mtm);
            mtm.build();

            HasCommunication = HasCommunication || mtm.HasCommunication;
            HasDissolution = HasDissolution || mtm.HasDissolution;
            HasDivision = HasDivision || mtm.HasDivision;
            HasLinkCreation = HasLinkCreation || mtm.HasLinkCreation;
            HasLinkDestruction = HasLinkDestruction || mtm.HasLinkDestruction;
            InstanceCount += mtm.Instances.Length;
            RuleCount += mtm.RuleSet.Length;
            MTypeCount++;

            return mtm;
        }

        private int nextTypeId() {
            return typeIdgen++;
        }
    }
}
