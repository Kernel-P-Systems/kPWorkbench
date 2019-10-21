using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Verification.Runtime
{
    public interface IModelCheckingProgressMonitor
    {
        void Start(int taskCount, String description);

        void LogProgress(int currentTask, String description);

        void Done(String description);

        void Terminate(String description);
    }
}
