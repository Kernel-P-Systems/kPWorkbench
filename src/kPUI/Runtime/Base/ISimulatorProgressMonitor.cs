using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUi.Simulation.Runtime
{
    public interface ISimulatorProgressMonitor
    {
        void Start(int taskCount, String description);

        void LogProgress(int currentTask, String description);

        void Done(String description);

        void Terminate(String description);
    }
}
