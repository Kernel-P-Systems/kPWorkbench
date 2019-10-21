using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore;

namespace KpFLAME
{
    public class AgentsInitialConfiguration
    {
        private List<AgentMembrane> membranes;

        public List<AgentMembrane> Membranes { get { return membranes; } set {}}

        public AgentsInitialConfiguration()
        {
            membranes = new List<AgentMembrane>();
        }

        public void AddMembrane(AgentMembrane membrane)
        {
            membranes.Add(membrane);
        }
    }

    public class AgentMembrane
    {
        private List<Instance> instances;

        public List<Instance> Instances { get { return instances; } private set{}}

        public int InstanceNumbers { get { return this.instances.Count; } }

        public string Name { get; set; }

        public string Id { get; set; }

        public AgentMembrane()
        {
            instances = new List<Instance>();
        }

        public void AddInstance(Instance instance)
        {
            instances.Add(instance);
        }

        public void AddInstance(string multiset, MInstance mInstance, Membranes membranes, MembraneId membraneId)
        {
            List<InstanceIds> connections = new List<InstanceIds>();
            Dictionary<int,InstanceIds> connectionsMap = new Dictionary<int, InstanceIds>();
            foreach (MInstance m in mInstance.Connections)
            {
                KeyValuePair<string, int> key = membranes[m];
                int id = membraneId[key.Key];
                if (connectionsMap.ContainsKey(id))
                    connectionsMap[id].Add(key.Value);
                else
                {
                    connections.Add(new InstanceIds(id));
                    connectionsMap.Add(id, connections[connections.Count - 1]);
                    connectionsMap[id].Add(key.Value);
                }
            }
            instances.Add(new Instance(this, membranes[mInstance].Value, multiset, connections));
        }
    }

    public class Instance
    {
        public AgentMembrane Membrane { get; private set; }

        public int Id { get; private set; }

        public string Multiset { get; private set; }

        public List<InstanceIds> Connections { get; private set; }

        public Instance(AgentMembrane agent, int id, string multiset, List<InstanceIds> connections)
        {
            Membrane = agent;
            this.Id = id;
            Multiset = multiset;
            this.Connections = connections;
        }
    }

    public class InstanceIds
    {
        public int Id { get; private set; }

        public List<int> Connections { get; private set; }

        public InstanceIds(int id)
        {
            this.Id = id;
            this.Connections = new List<int>();
        }

        public void Add(int connection)
        {
            Connections.Add(connection);
        }
    }
}
