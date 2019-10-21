using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;

namespace KpUi
{
    public class ModelChecker
    {
        public readonly static string Spin = "SPIN";
        public readonly static string NuSMV = "NuSMV";

        private readonly static Dictionary<PropertyType, List<string>> modelCheckers = new Dictionary<PropertyType, List<string>>()
        {
            { PropertyType.LTL, new List<string>(new string[] { Spin, NuSMV }) },
            { PropertyType.CTL, new List<string>(new string[] { NuSMV }) }
        };

        public static List<string> GetModelChecker(PropertyType property)
        {
            return modelCheckers[property];
        }
    }
}
