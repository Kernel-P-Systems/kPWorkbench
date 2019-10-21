using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore;

namespace KpFLAME
{
    public class Membranes
    {
        private ObjectsId objectsId;

        private Dictionary<string, KeyValuePair<MType, int>> membranesType;

        private Dictionary<MInstance, KeyValuePair<string,int>> membranesInstance;

        public Membranes(ObjectsId objects)
        {
            objectsId = objects;
            membranesType = new Dictionary<string, KeyValuePair<MType, int>>();
            membranesInstance = new Dictionary<MInstance, KeyValuePair<string, int>>();
        }

        public KeyValuePair<string,int> this[MInstance instance]
        {
            get { return membranesInstance[instance]; }
        }

        public void AddMembraneType(MType mType)
        {
            if (!membranesType.ContainsKey(mType.Name))
            {
                membranesType.Add(mType.Name, new KeyValuePair<MType, int>(mType, membranesType.Count + 1));
                AddInstances(mType);
            }
        }

        private void AddInstances(MType mType)
        {
            int i = 0;
            foreach (MInstance instance in mType.Instances)
            {
                membranesInstance.Add(instance, new KeyValuePair<string, int>(mType.Name, i++));
                objectsId.Add(instance.Multiset);
            }
        }
    }
}
