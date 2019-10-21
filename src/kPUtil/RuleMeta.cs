using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class RuleMeta {

        private Rule rule;
        public Rule Rule { get { return rule; } private set { rule = value; } }

        private int id;
        public int Id { get { return id; } private set { id = value; } }

        private MTypeMeta mtm;
        public MTypeMeta MTypeMeta { get { return mtm; } private set { mtm = value; } }
        public MType MType { get { return mtm.MType; } }
        public KpMetaModel KpMetaModel { get { return mtm.KpMetaModel; } }
        public KPsystem KPsystem { get { return mtm.KPsystem; } }

        public RuleMeta(MTypeMeta mt, Rule rule, int id) {
            MTypeMeta = mtm;
            Rule = rule;
            Id = id;
        }
    }
}
