using KpExperiment.Model.Verification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Verification.Translation.Base
{
    public interface ILtlPropertyTranslator : IVisitor<string>
    {
        ModelCheckingTarget Target { get; }
    }
}
