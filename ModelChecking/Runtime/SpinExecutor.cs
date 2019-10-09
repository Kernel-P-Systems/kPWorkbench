using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using KpLingua;
using KpSpin;
using KpSpin.SpinVerificationModel;
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
    public class SpinExecutor : IModelCheckingExecutor
    {
        public async Task Verify(FileInfo kplModelFile, IEnumerable<IProperty> properties, FileInfo verificationDirectory, IModelCheckingProgressMonitor monitor)
        {
            await Task.Run(() =>
            {
                try
                {
                    monitor.Start(4, string.Format("Verifying the {0} model using the SPIN modelchecker...", kplModelFile.Name));

                    monitor.LogProgress(0, "Generating the correspoding PROMELA model...");

                    var verificationModelFileName = string.Format("{0}\\{1}.pml", verificationDirectory.FullName, Path.GetFileNameWithoutExtension(kplModelFile.Name));
                    using (var fileWriter = new FileInfo(verificationModelFileName).CreateText())
                    {
                        //Old translator
                        /*
                        var translationParameters = PromelaTranslationParams.Default();
                        translationParameters.PrintRuleExecution = false;
                        translationParameters.PrintConfiguration = false;
                        translationParameters.PrintTargetSelection = false;
                        translationParameters.PrintLinks = false;
                         */

                        var translationParameters = VerificationModelParams.Default();

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
                            LtlProperties = properties.Where(p => p is ILtlProperty).Cast<ILtlProperty>().ToList(),
                        };

                        //Old translator
                        //KP.WritePromela(kpModel.KPsystem, experiment, translationParameters, fileWriter);

                        KP.WriteVerificationPromelaModel(kpModel, experiment, translationParameters, fileWriter);
                    }

                    monitor.LogProgress(1, "Translating the PROMELA model into its computationally equivalent verification model...");
                    GenerateModel(verificationDirectory, verificationModelFileName);

                    monitor.LogProgress(2, "Compiling the verification model...");
                    CompileModel(verificationDirectory, verificationModelFileName);

                    monitor.LogProgress(3, "Performing model checking...");
                    ExecuteModel(verificationDirectory, verificationModelFileName);

                    //monitor.LogProgress(4, "Generating the output trail file...");
                    //GenerateTrail(verificationDirectory, verificationModelFileName);

                    monitor.Done("Finished the verification process");
                }
                catch (Exception e)
                {
                    monitor.Terminate("Verification failed: " + e.Message);
                }
            });
        }

        private void GenerateModel(FileInfo verificationDirectory, string verificationModelFileName)
        {
            var p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = verificationDirectory.FullName,
                //FileName = "spin",
                FileName = "spin64",
                Arguments = string.Format("-a {0}", Path.GetFileName(verificationModelFileName)),
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                DumpErrors(p, verificationDirectory, verificationModelFileName);
                throw new Exception("Please check the output file for more details.");
            }
        }

        private void CompileModel(FileInfo verificationDirectory, string verificationModelFileName)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = verificationDirectory.FullName,
                FileName = "gcc",
                //Arguments = "-o pan -DVECTORSZ=10000000 -DBITSTATE pan.c",
                Arguments = "-DWIN64 -DVECTORSZ=99999999 pan.c -Fepan",
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                DumpErrors(p, verificationDirectory, verificationModelFileName);
                throw new Exception("Please check the output file for more details.");
            }
        }

        private void ExecuteModel(FileInfo verificationDirectory, string verificationModelFileName)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = verificationDirectory.FullName,
                FileName = string.Format("{0}\\a", verificationDirectory.FullName),
                Arguments = "-a -m999999",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();

            using (var outputStream = new FileInfo(string.Format("{0}\\{1}-spin-output.txt", verificationDirectory.FullName, Path.GetFileNameWithoutExtension(verificationModelFileName))).CreateText())
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
                DumpErrors(p, verificationDirectory, verificationModelFileName);
                throw new Exception("Please check the output file for more details.");
            }
        }

        private void GenerateTrail(FileInfo verificationDirectory, string verificationModelFileName)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = verificationDirectory.FullName,
                FileName = "spin",
                Arguments = string.Format("-t -u {0}", Path.GetFileName(verificationModelFileName)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();

            using (var outputStream = new FileInfo(string.Format("{0}\\{1}-spin-trail.txt", verificationDirectory.FullName, Path.GetFileNameWithoutExtension(verificationModelFileName))).CreateText())
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
        }

        private void DumpErrors(Process p, FileInfo verificationDirectory, string verificationModelFileName)
        {
            var resultsFile = string.Format("{0}\\{1}-spin-output.txt", verificationDirectory.FullName, Path.GetFileNameWithoutExtension(verificationModelFileName));

            using (var outputStream = new FileInfo(resultsFile).CreateText())
            {
                var errorLine = string.Empty;
                while (!p.StandardError.EndOfStream && (errorLine = p.StandardError.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(errorLine))
                    {
                        outputStream.WriteLine(errorLine);
                    }

                    outputStream.Flush();
                }
            }
        }
    }
}
