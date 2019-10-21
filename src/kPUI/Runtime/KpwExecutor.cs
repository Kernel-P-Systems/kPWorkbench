using KpUi.Simulation.Runtime;
using kpw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpFLAME;
using System.Reflection;

namespace KpUi.Runtime
{
    public class KpwExecutor : ISimulatorExecutor
    {
        private SimulatorSettings settings;
        public async Task Verify(FileInfo kplModelFile, FileInfo outputFile, SimulatorSettings settings, ISimulatorProgressMonitor monitor)
        {
            this.settings = settings;
            await Task.Run(() =>
            {
                try
                {
                    monitor.Start(1, string.Format("Simulating the {0} model using the kPWorkbench simulator...", kplModelFile.Name));

                    monitor.LogProgress(0, "Performing simulation...");
                    Execute(kplModelFile, outputFile);

                    monitor.Done("Finished the simulation process");
                }
                catch (Exception e)
                {
                    monitor.Terminate("Simulation failed: " + e.Message);
                }
            });
        }

        private void Execute(FileInfo kplModelFile, FileInfo outputFile)
        {
            string s = settings.ToString();
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = outputFile.Directory.FullName,
                FileName = "kpw",
                Arguments = string.Format("-s {0} {1} -o {2}-simulation-result.txt", kplModelFile.FullName, s, Path.GetFileNameWithoutExtension(kplModelFile.Name)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit();
        }
    }
}
