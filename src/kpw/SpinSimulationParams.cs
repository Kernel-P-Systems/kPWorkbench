using KpSpin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kpw {

    public enum SpinSimulationOutput {
        Text, Html, Latex
    }

    public class SpinSimulationParams {

        public int MaxSteps { get; set; }

        public bool LinksEnabled { get; set; }
        public bool DivisionEnabled { get; set; }
        public bool DissolutionEnabled { get; set; }

        public bool PrintRuleExecution { get; set; }
        public bool PrintConfiguration { get; set; }
        public bool PrintTargetSelection { get; set; }
        public bool PrintLinks { get; set; }

        public bool PrintAll {
            get {
                return PrintConfiguration && PrintRuleExecution && PrintTargetSelection && PrintLinks;
            }
            set {
                PrintConfiguration = true;
                PrintRuleExecution = true;
                PrintTargetSelection = true;
                PrintLinks = true;
            }
        }

        public string FileName { get; set; }
        public string AuxDirName { get; set; }

        public SpinSimulationOutput OutputFormat { get; set; }

        public SpinSimulationParams() {

        }

        public static SpinSimulationParams Default() {
            return new SpinSimulationParams() {
                MaxSteps = 20,
                LinksEnabled = true,
                DivisionEnabled = true, 
                DissolutionEnabled = true,
                PrintConfiguration = true,
                PrintRuleExecution = true,
                PrintTargetSelection = false,
                PrintLinks = false,
                FileName = "model",
                AuxDirName = ".aux",
                OutputFormat = SpinSimulationOutput.Text
            };
        }

        public PromelaTranslationParams ToPromelaTranslationParams() {
            PromelaTranslationParams p = PromelaTranslationParams.Default();
            p.MaxSteps = MaxSteps;
            p.DivisionEnabled = DivisionEnabled;
            p.DivisionEnabled = DissolutionEnabled;
            p.LinksEnabled = LinksEnabled;

            p.PrintLinks = PrintLinks;
            p.PrintConfiguration = PrintConfiguration;
            p.PrintTargetSelection = PrintTargetSelection;
            p.PrintRuleExecution = PrintRuleExecution;

            return p;
        }
    }
}
