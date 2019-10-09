using KpExperiment.Verification;
using KpUi.Simulation.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUi.Runtime
{
    public class SimulatorManager
    {
        private static Lazy<SimulatorManager> _instance = new Lazy<SimulatorManager>(() => new SimulatorManager());

        public static SimulatorManager Instance
        {
            get { return _instance.Value; }
        }
        public async Task Verify(FileInfo kplModelFile, FileInfo outputFile, SimulatorSettings settings, SimulatorTarget target, ISimulatorProgressMonitor monitor)
        {
            var executor = GetExecutor(target);
            await executor.Verify(kplModelFile, outputFile, settings, monitor);
        }

        private ISimulatorExecutor GetExecutor(SimulatorTarget target)
        {
            var executor = default(ISimulatorExecutor);

            switch (target)
            {
                case SimulatorTarget.Kpw: executor = new KpwExecutor(); break;
                case SimulatorTarget.Flame: executor = new FlameExecutor(); break;
            }

            return executor;
        }
    }
}
