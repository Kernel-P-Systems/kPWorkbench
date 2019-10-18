using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using KpLingua;
using NuSMV;
using KpSpin;
using KpUtil;
using kpw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Verification.Runtime
{
    public class NuSmvExecutor : IModelCheckingExecutor
    {
        public async Task Verify(FileInfo kplModelFile, IEnumerable<IProperty> properties, FileInfo verificationDirectory, IModelCheckingProgressMonitor monitor)
        {
            await Task.Run(() =>
            {
                try
                {
                    monitor.Start(2, string.Format("Verifying the {0} model using the NuSMV modelchecker...", kplModelFile.Name));

                    monitor.LogProgress(0, "Generating the correspoding NuSMV model...");
                    KpModel kpModel = null;
                    try
                    {
                        kpModel = KP.FromKpl(kplModelFile.FullName);
                    }
                    catch (KplParseException kplException)
                    {
                        throw new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplModelFile.Name, kplException.Message));
                    }

                    var experiment = new Experiment
                    {
                        LtlProperties = properties.Where(p => !(p is ICtlProperty)).Cast<ILtlProperty>().ToList(),
                        CtlProperties = properties.Where(p => p is ICtlProperty).Cast<ICtlProperty>().ToList(),
                    };

                    var verificationModelFileName = string.Format("{0}\\{1}.smv", verificationDirectory.FullName, Path.GetFileNameWithoutExtension(kplModelFile.Name));
                    TranslateSMV.Translate(kpModel, experiment, verificationModelFileName);

                    monitor.LogProgress(1, "Performing model checking...");
                    ExecuteModel(verificationDirectory, verificationModelFileName);

                    monitor.Done("Finished the verification process");
                }
                catch (Exception e)
                {
                    monitor.Terminate("Verification failed: " + e.Message);
                }
            });
        }

        private void ExecuteModel(FileInfo verificationDirectory, string verificationModelFileName)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = verificationDirectory.FullName,
                FileName = AppSettings.Instance.NuSmvPath,
                Arguments = verificationModelFileName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();

            using (var outputStream = new FileInfo(string.Format("{0}\\{1}-nusmv-output.txt", verificationDirectory.FullName, Path.GetFileNameWithoutExtension(verificationModelFileName))).CreateText())
            {
                var outputLine = string.Empty;
                var errorLine = string.Empty;
                while ((!p.StandardOutput.EndOfStream || !p.StandardError.EndOfStream) && (((outputLine = p.StandardOutput.ReadLine()) != null || (errorLine = p.StandardError.ReadLine()) != null)))
                {
                    if (!string.IsNullOrEmpty(outputLine))
                    {
                        outputStream.WriteLine(outputLine);
                    }

                    if (!string.IsNullOrEmpty(errorLine))
                    {
                        outputStream.WriteLine(errorLine);
                    }

                    outputStream.Flush();
                }
            }

            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                throw new Exception("Please check the output file for more details.");
            }
        }
    }
}
