using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class KpIdMap {

        private KPsystem kp;
        private Dictionary<int, MType> mtypeIds;
        private Dictionary<int, MInstance> instanceIds;
        private Dictionary<int, Rule> ruleIds;

        private int tId = 0;
        private int iId = 0;
        private int rId = 0;

        public KPsystem KPsystem { get { return kp; } private set { kp = value; } }

        public KpIdMap(KPsystem kps) {
            if (kps == null) {
                throw new ArgumentNullException("kps");
            }
            KPsystem = kps;

            mtypeIds = new Dictionary<int, MType>();
            instanceIds = new Dictionary<int, MInstance>();
            ruleIds = new Dictionary<int, Rule>();

            Generate();
        }

        protected void Generate() {
            
            foreach (MType mtype in kp.Types) {
                mtype.Id = tId++;
                mtypeIds.Add(mtype.Id, mtype);

                foreach (MInstance instance in mtype.Instances) {
                    instance.Id = iId++;
                    instanceIds.Add(instance.Id, instance);
                }

                ExecutionStrategy ex = mtype.ExecutionStrategy;
                while (ex != null) {
                    foreach (Rule r in ex.Rules) {
                        r.Id = rId++;
                        ruleIds.Add(r.Id, r);
                    }
                    ex = ex.Next;
                }
            }
        }

        public void RegisterNewInstance(MInstance instance) {
            instance.Id = iId++;
            instanceIds.Add(instance.Id, instance);
        }

        public Rule GetRuleForId(int id) {
            Rule r = null;
            ruleIds.TryGetValue(id, out r);

            return r;
        }

        public MType GetTypeForId(int id) {
            MType t = null;
            mtypeIds.TryGetValue(id, out t);

            return t;
        }

        public MInstance GetInstanceForId(int id) {
            MInstance m = null;
            instanceIds.TryGetValue(id, out m);

            return m;
        }
    }
}
