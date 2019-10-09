using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUi.Simulation.Runtime
{
    public interface ISimulatorExecutor
    {
        Task Verify(FileInfo kplModelFile, FileInfo outputFile, SimulatorSettings settings, ISimulatorProgressMonitor monitor);
    }
}
