using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore;

namespace KpFLAME
{
    public class Id
    {
        protected Dictionary<string, int> iDs;

        public Id()
        {
            iDs = new Dictionary<string, int>();
        }

        public void Add(string name)
        {
            if (!iDs.ContainsKey(name))
                iDs.Add(name, iDs.Count);
        }

        public int this[string name]
        {
            get
            {
                if (iDs.ContainsKey(name))
                    return iDs[name];
                else
                {
                    Add(name);
                    return iDs[name];
                }
            }

            set
            {
                Add(name);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string,int> keyValuePair in iDs)
            {
                sb.AppendFormat("{0}\t{1}\r\n", keyValuePair.Value, keyValuePair.Key);
            }
            return sb.ToString();
        }
    }

    public class ObjectsId : Id
    {
        public ObjectsId():base(){}

        public void Add(Multiset m)
        {
            foreach (string o in m.Objects)
                Add(o);
        }
    }

    public class MembraneId : Id
    {
        public MembraneId():base(){}
    }
}
