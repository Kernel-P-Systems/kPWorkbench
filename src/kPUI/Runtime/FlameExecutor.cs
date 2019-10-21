using KpUi.Simulation.Runtime;
using kpw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpLingua.Input;
using KpFLAME;
using KpCore;
using KpLingua;
using KpUtil;

namespace KpUi.Runtime
{
    public class FlameExecutor : ISimulatorExecutor
    {
        private readonly string modelGeneratedFile = "model_generated.xml";
        private readonly string executableFileName = "main.exe";
        private SimulatorSettings settings;
        private ISimulatorProgressMonitor monitor;
        public async Task Verify(FileInfo kplModelFile, FileInfo outputFile, SimulatorSettings settings, ISimulatorProgressMonitor monitor)
        {
            this.settings = settings;
            this.monitor = monitor;
            await Task.Run(() =>
            {
                try
                {
                    monitor.Start(5, string.Format("Simulates the {0} model using the FLAME simulator...", kplModelFile.Name));

                    monitor.LogProgress(0, "Translate in the correspoding FLAME model...");
                    FileInfo tempFolder;
                    TranslateModel(kplModelFile, outputFile, out tempFolder);

                    monitor.LogProgress(2, "Generating the FLAME model...");
                    GenerateModel(new FileInfo(tempFolder + modelGeneratedFile));

                    monitor.LogProgress(3, "Compiling the FLAME model...");
                    CompileModel(tempFolder);

                    Clean(tempFolder);

                    monitor.LogProgress(4, "Simulate the FLAME model...");
                    Simulate(outputFile);


                    monitor.Done("Finished the simulation process");
                }
                catch (Exception e)
                {
                    monitor.Terminate("Simulation failed: " + e.Message);
                }
            });
        }

        private void TranslateModel(FileInfo kplFileName, FileInfo outputPathName, out FileInfo tempFolder)
        {
            KpModel kpModel = null;
            try
            {
                kpModel = KP.FromKpl(kplFileName.FullName);
            }
            catch (KplParseException kplException)
            {
                new Exception(string.Format("Failed to parse input file {0}. Reason: {1}", kplFileName, kplException.Message));
            }

            KPsystemXMLWriter kPsystemXML = new KPsystemXMLWriter(kpModel.KPsystem);
            try
            {
                Directory.CreateDirectory(outputPathName + "ite");
            }
            catch (KplParseException kplException)
            {
                new Exception(string.Format("Failed to parse create directory {0}. Reason: {1}", kplFileName, kplException.Message));
            }
            using (StreamWriter writer = new StreamWriter(outputPathName + modelGeneratedFile))
            {
                writer.Write(kPsystemXML.ToXML());
            }
            kPsystemXML.SaveCFiles(outputPathName.FullName);
            int i = 0;
            bool b;
            do
            {
                tempFolder = new FileInfo(i == 0 ? outputPathName + "tmp" : outputPathName + "tmp" + i);
                i++;
                b = Directory.Exists(tempFolder.FullName);
            } while (b);
            try
            {
                Directory.CreateDirectory(tempFolder.FullName);
                tempFolder = new FileInfo(tempFolder + @"\");
            }
            catch (KplParseException kplException)
            {
                new Exception(string.Format("Failed to parse create directory {0}. Reason: {1}", kplFileName, kplException.Message));
            }
            using (StreamWriter writer = new StreamWriter(tempFolder + modelGeneratedFile))
            {
                writer.Write(kPsystemXML.ToXML());
            }
            kPsystemXML.SaveCFiles(tempFolder.FullName);
            using (StreamWriter writer = new StreamWriter(outputPathName + @"ite\0.xml"))
            {
                writer.Write(kPsystemXML.ToAgentsInitialConfiguration());
            }
            using (StreamWriter writer = new StreamWriter(outputPathName + @"ite\map.txt"))
            {
                writer.Write(kPsystemXML.MapObjectIds);
            }
            monitor.LogProgress(2, "KP model translated FLAME model successfully.");
        }

        private void GenerateModel(FileInfo fileName)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = AppSettings.Instance.FlameXparserPath,
                FileName = AppSettings.Instance.FlameXparserPath + AppSettings.Instance.FlameXparserName,
                Arguments = fileName + " -s -f",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit();
        }

        private void CompileModel(FileInfo fileName)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = fileName.FullName,
                FileName = "make",
                Arguments = string.Format("LIBMBOARD_DIR={0}", AppSettings.Instance.FlameLibmboardPath),
            };

            p.StartInfo = startInfo;
            p.Start();
            p.WaitForExit();
        }

        private void Clean(FileInfo fileName)
        {
            DirectoryInfo parent = Directory.GetParent(Directory.GetParent(fileName.FullName).FullName);
            if (File.Exists(parent.FullName + @"\" + executableFileName))
                File.Delete(parent.FullName + @"\" + executableFileName);
            Directory.Move(fileName.FullName + executableFileName, parent.FullName + @"\" + executableFileName);
            Directory.Delete(Directory.GetParent(fileName.FullName).FullName, true);
        }

        private void Simulate(FileInfo file)
        {
            Process p = new Process();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = file.Directory.FullName,
                FileName = file.FullName + executableFileName,
                Arguments = string.Format("{0} ite/0.xml", settings.Steps),
            };

            p.StartInfo = startInfo;
            p.Start();
            using (StreamWriter writer = new StreamWriter(file.FullName + "out.txt"))
            {
                writer.Write(p.StandardOutput.ReadToEnd());
            }
            p.WaitForExit();
        }
    }
}
