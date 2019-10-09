using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Verification.Runtime
{
    public interface IModelCheckingExecutor
    {
        Task Verify(FileInfo kplModelFile, IEnumerable<IProperty> properties, FileInfo verificationModelFile, IModelCheckingProgressMonitor monitor);
    }
}
