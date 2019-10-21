using KpCore;
using KpLingua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSimulation {
    public class SimulationTextWriter {

        private TextWriter owt;

        public bool MuteConfiguration { get; set; }
        public bool MuteTargetSelection { get; set; }
        public bool MuteInstanceCreation { get; set; }
        public bool MuteRuleSelection { get; set; }

        public bool AutoClose { get; set; }
        public  KpSimulationParams SimulationParams { get; protected set; }

        public SimulationTextWriter(TextWriter writer) {
            owt = writer;
            AutoClose = true;
        }

        public void BindToSimulator(KpSimulator kps) {
            if (kps == null) {
                throw new ArgumentNullException("kps");
            }
            SimulationParams = kps.SimulationParams;

            if (kps.SimulationStarted == null) {
                kps.SimulationStarted = simulationStarted;
            } else {
                kps.SimulationStarted += simulationStarted;
            }

            if (kps.ReachedStep == null) {
                kps.ReachedStep = reachedStep;
            } else {
                kps.ReachedStep += reachedStep;
            }

            if (!MuteConfiguration) {
                if (kps.StepComplete == null) {
                    kps.StepComplete = stepComplete;
                } else {
                    kps.StepComplete += stepComplete;
                }
            }

            if (!MuteRuleSelection) {
                if (kps.RuleApplied == null) {
                    kps.RuleApplied = ruleApplied;
                } else {
                    kps.RuleApplied += ruleApplied;
                }
            }

            if (!MuteTargetSelection) {
                if (kps.TargetSelected == null) {
                    kps.TargetSelected = targetSelected;
                } else {
                    kps.TargetSelected += targetSelected;
                }
            }

            if (!MuteInstanceCreation) {
                if (kps.NewInstanceCreated == null) {
                    kps.NewInstanceCreated = newInstanceCreated;
                } else {
                    kps.NewInstanceCreated += newInstanceCreated;
                }
            }

            if (kps.SystemHalted == null) {
                kps.SystemHalted = systemHalted;
            } else {
                kps.SystemHalted += systemHalted;
            }

            if (kps.SimulationComplete == null) {
                kps.SimulationComplete = simulationComplete;
            } else {
                kps.SimulationComplete += simulationComplete;
            }

        }

        private void simulationStarted(KPsystem kp) {
            owt.WriteLine("Simulation started.");
            if (!MuteConfiguration) {
                writeInitialConfig(kp);
            }
        }

        private void systemHalted(int step) {
            owt.WriteLine("System halted at step {0} (no further rule could be applied, i.e. reached a deadlock).", step);
        }

        private void simulationComplete(int step) {
            owt.WriteLine("Simulation complete at step {0}.", step);
            if (AutoClose) {
                owt.Flush();
                owt.Close();
            }
        }

        private void reachedStep(int step) {
            owt.WriteLine();
            owt.WriteLine("STEP {0}", step);
            if (SimulationParams.RecordRuleSelection || SimulationParams.RecordTargetSelection) {
                owt.WriteLine();
            }
        }

        private void stepComplete(KpSimulationStep step) {
            owt.WriteLine();
            owt.WriteLine("Configuration #{0}", step.Step);
            owt.WriteLine("-------------------------------------------------");
            writeKPsystem(step.KPsystem);
            owt.WriteLine("-------------------------------------------------");
        }

        private void writeInitialConfig(KPsystem kp) {
            owt.WriteLine("Initial Configuration");
            owt.WriteLine("-------------------------------------------------");
            writeKPsystem(kp);
            owt.WriteLine("-------------------------------------------------");
        }

        private void writeKPsystem(KPsystem kp) {
            foreach (MType type in kp.Types) {
                owt.WriteLine("TYPE {0}", type.Name);
                foreach (MInstance instance in type.Instances) {
                    owt.Write("#{0}", instance.Id);
                    if (instance.HasName()) {
                        owt.Write(" {0}", instance.Name);
                    }
                    string status = null;
                    if(instance.IsCreated) {
                        status = "Created";
                    }
                    
                    if(instance.IsDissolved) {
                        status = status == null ? "Dissolved" : status + ", Dissolved";
                    } else if(instance.IsDivided) {
                        status = status == null ? "Divided" : status + ", Divided";
                    } else {
                        status = status == null ? "Active" : status + ", Active";
                    }

                    owt.Write(" [{0}]", status);

                    owt.Write(" {");
                    int i = 1, count = instance.Multiset.Count;
                    foreach (KeyValuePair<string, int> ms in instance.Multiset) {
                        if(ms.Value > 0) {
                            if(ms.Value == 1) {
                                owt.Write(ms.Key);
                            } else {
                                owt.Write("{0}{1}", ms.Value, ms.Key);
                            }
                            if (i++ < count) {
                                owt.Write(", ");
                            }   
                        }
                    }
                    owt.Write("}");

                    owt.Write(" Links to #: ");
                    i = 1;
                    count = instance.Connections.Count;
                    if (count == 0) {
                        owt.Write("none");
                    } else {
                        foreach (MInstance connection in instance.Connections) {
                            owt.Write("{0}", connection.Id);
                            if (i++ < count) {
                                owt.Write(", ");
                            }
                        }
                    }
                    owt.WriteLine();
                }
            }
        }

        private void ruleApplied(Rule r, MInstance instance) {
            owt.WriteLine("Rule #{0} * {1} * applied in instance #{2} {3}.", r.Id, r.ToKpl(), instance.Id, 
                instance.HasName() ? instance.Name : "");
        }

        private void targetSelected(MInstance target, MType type, Rule r) {
            owt.WriteLine("Selected instance #{0} as target of type {1} for rule #{2}.", target.Id, type.Name, r.Id);
        }

        private void newInstanceCreated(MInstance instance, MType type) {
            owt.WriteLine("Instance #{0} of type {1} created.", instance.Id, type.Name);
        }
    
    }
}
