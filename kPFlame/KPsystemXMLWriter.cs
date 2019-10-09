using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore;
using Antlr4.StringTemplate;

namespace KpFLAME
{
    public class KPsystemXMLWriter
    {
        private KPsystem kPsystem;

        private Model model;

        public AgentsInitialConfiguration agentsInitial;

        private ObjectsId objectsId;

        private Dictionary<string, int> typeId;

        public string MapObjectIds { get { return objectsId.ToString(); } }

        private MembraneId membraneId;

        private Membranes membranes;

        public KPsystemXMLWriter(KPsystem kp)
        {
            kPsystem = kp;
            model = new Model();
            agentsInitial = new AgentsInitialConfiguration();
            objectsId = new ObjectsId();
            typeId = new Dictionary<string, int>();
            membraneId = new MembraneId();
            membranes = new Membranes(objectsId);
            FlameRulesGenerator flameRulesGenerator = new FlameRulesGenerator(objectsId, membraneId);
            FlameCodeRulesGenerator flameCodeRulesGenerator = new FlameCodeRulesGenerator(objectsId, membraneId);
            foreach (MType mType in kPsystem.Types)
            {
                typeId.Add(mType.Name, typeId.Count);
                membranes.AddMembraneType(mType);
                model.AddAgent(new Agent(mType, typeId[mType.Name], flameCodeRulesGenerator));
                Console.WriteLine(mType.Name);
            }
            foreach (MType mType in kPsystem.Types)
            {
                AgentMembrane agentMembrane = new AgentMembrane();
                agentMembrane.Name = mType.Name;
                agentMembrane.Id = membraneId[mType.Name].ToString();
                List<int> list = flameCodeRulesGenerator.Rules(mType.ExecutionStrategy);
                foreach (MInstance mInstance in mType.Instances)
                {
                    agentMembrane.AddInstance(flameRulesGenerator.Multiset(mInstance.Multiset), mInstance, membranes, membraneId);
                }
                agentsInitial.AddMembrane(agentMembrane);
            }
        }

        public void SaveCFiles(string cFilesPath)
        {
            foreach (Agent agent in model.Agents)
            {
                string cFile = string.Format("{0}functions_{1}.c", cFilesPath, agent.Name);
                using (StreamWriter writer = new StreamWriter(cFile))
                {
                    writer.Write(SaveCFile(agent));
                }
            }
        }

        private string SaveCFile(Agent agent)
        {
            string modelTemplate = @"model\c\Functions.stg";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, modelTemplate);
            TemplateGroup templateGroup = new TemplateGroupFile(path);
            Template st = templateGroup.GetInstanceOf("Functions");
            st.Add("agent", agent);
            return st.Render();
        }

        public string ToXML()
        {
            string modelTemplate = @"model\xml\Model.stg";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, modelTemplate);
            TemplateGroup templateGroup = new TemplateGroupFile(path);
            Template st = templateGroup.GetInstanceOf("Model");
            st.Add("model", model);
            return st.Render();
        }

        public string ToAgentsInitialConfiguration()
        {
            string modelTemplate = @"model\ite\InitialConfiguration.stg";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, modelTemplate);
            TemplateGroup templateGroup = new TemplateGroupFile(path);
            Template st = templateGroup.GetInstanceOf("InitialConfiguration");
            st.Add("model", agentsInitial);
            return st.Render();
        }
    }
}
