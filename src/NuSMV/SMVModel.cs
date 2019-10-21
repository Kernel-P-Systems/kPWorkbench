using System;
using System.Collections.Generic;

namespace NuSMV
{
    public class SMVModel
    {
        public SMVModel()
        {
            Modules = new List<Module>();
        }

        public List<Module> Modules { get; set; }

        public override string ToString()
        {
            string result = "";
            foreach (var item in Modules)
            {
                result += item.ToString() + " \n";
            }
            return result;
        }

        public Module getModule(KpCore.MType type, KpCore.MInstance instance)
        {
            Module result = null;
            foreach (var module in this.Modules)
            {
                if (module.Type == type.Name && module.Instance.Name == instance.Name)
                {
                    result = module;
                    break;
                }
            }
            if (result == null)
                throw new Exception("Module :'" + instance.Name + "_" + type.Name + "' not found!");
            return result;
        }

        public MainModule MainModule { get; set; }
    }
}