using KpExperiment.Model.Verification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model
{
    public class Experiment
    {
        public IEnumerable<ILtlProperty> LtlProperties { get; set; }
        
        public IEnumerable<ICtlProperty> CtlProperties { get; set; }
    }
}
