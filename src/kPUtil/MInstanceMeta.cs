using KpCore;
using KpUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class MInstanceMeta {

        private MInstance instance;
        public MInstance MInstance { get { return instance; } private set { instance = value; } }

        private int id;
        public int Id { get { return id; } private set { id = value; } }

        private MTypeMeta mtm;
        public MTypeMeta MTypeMeta { get { return mtm; } private set { mtm = value; } }
        public MType MType { get { return mtm.MType; } }
        public KpMetaModel KpMetaModel { get { return mtm.KpMetaModel; } }
        public KPsystem KPsystem { get { return mtm.KPsystem; } }

        public MInstanceMeta(MTypeMeta tm, MInstance instance, int id) {
            MTypeMeta = tm;
            MInstance = instance;
            Id = id;
        }

        public bool IsEmpty() {
            return instance.Multiset.IsEmpty();
        }
    }
}
