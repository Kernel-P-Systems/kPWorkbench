using KpCore;
using KpExperiment.Model;
using KpSpin;
using KpSpin.Simulation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kpw {
    public class Spin {

        public static void Simulate(KPsystem kp) {
            Simulate(kp, Console.Out);
        }

        public static void Simulate(KPsystem kp, TextWriter writer) {
            Simulate(kp, writer, SpinSimulationParams.Default());
        }

        public static void Simulate(KPsystem kp, TextWriter writer, SpinSimulationParams ps) {
            string path = "";
            if (!String.IsNullOrEmpty(ps.AuxDirName)) {
                path = ps.AuxDirName;
                Directory.CreateDirectory(path);
                path += "\\";
            }
            path += ps.FileName;

            //ps.PrintRuleExecution = false;
            ps.LinksEnabled = true;
            ps.PrintTargetSelection = true;
            
            //translate to promela first
            string pml = path + ".pml";
            StreamWriter w = new StreamWriter(pml);
            KpTranslator translator = new KpTranslator(kp, ps.ToPromelaTranslationParams());
            PromelaModel model = translator.Translate();
            new PromelaWriter(w).Write(model);

            Process process = new Process();

            ProcessStartInfo psi = new ProcessStartInfo();
            process.StartInfo = psi;

            psi.Arguments = "-T " + pml;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.FileName = "spin";

            process.Start();

            ConfigTextWriter ctw = new ConfigTextWriter(writer);
            StreamReader so = process.StandardOutput;
            while (!so.EndOfStream) {
                string line = so.ReadLine();
                if (!String.IsNullOrEmpty(line)) {
                    if (line.StartsWith("#")) {
                        KpSystemConfiguration config = translator.TranslateConfig(line);
                        ctw.WriteConfig(config);
                    } else if (line.StartsWith("@")) {
                        writer.WriteLine();
                        KpRuleApplcation rApplication = translator.TranslateRuleAppliaction(line);
                        ctw.WriteRuleApplication(rApplication);
                    } else if (line.StartsWith("$")) {
                        writer.WriteLine();
                        KpTargetSelection targetSelection = translator.TranslateTargetSelection(line);
                        ctw.WriteTargetSelection(targetSelection);
                    } else {
                        Console.WriteLine(line);
                    }
                }
            }

            writer.Flush();
            writer.Close();
        }
    }
}
