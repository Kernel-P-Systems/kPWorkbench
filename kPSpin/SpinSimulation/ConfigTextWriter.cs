using KpCore;
using KpLingua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin.Simulation {
    public class ConfigTextWriter {
        private TextWriter owt;
        public int Verbosity { get; set; }

        public ConfigTextWriter(TextWriter writer) {
            owt = writer;
            Verbosity = 1;
        }

        public void WriteConfig(KpSystemConfiguration config) {
            if (config.Step == 0) {
                owt.WriteLine("STEP " + config.Step + " (Initial configuration)");
            } else {
                owt.WriteLine();
                owt.WriteLine("STEP " + config.Step);
            }

            owt.WriteLine();
            KPsystem kp = config.KPsystem;

            foreach (MType mt in kp.Types) {
                if (Verbosity == 1) {
                    owt.WriteLine("Type " + mt.Name);
                } else {
                    owt.WriteLine("Type " + mt.Name + " (ID=" + mt.Id + ")");
                }
                foreach (MInstance mi in mt.Instances) {
                    if (Verbosity == 1) {
                        owt.Write("- Instance " + (mi.HasName() ? mi.Name : mi.Id.ToString()) + ": ");
                    } else {
                        owt.Write("- Instance " + (mi.HasName() ? mi.Name : "") + "ID=" + mi.Id + ":");
                    }

                    int count = mi.Multiset.Count;
                    if (count == 0) {
                        owt.Write("<empty>");
                    } else {
                        int i = 1;
                        foreach (KeyValuePair<string, int> kv in mi.Multiset) {
                            owt.Write(kv.Value + " " + kv.Key);
                            if (i++ < count) {
                                owt.Write(", ");
                            }
                        }
                    }

                    if (mi.IsCreated) {
                        owt.Write(" [created]");
                    }

                    if (mi.IsDissolved) {
                        owt.Write(" [dissolved]");
                    }

                    if (mi.IsDivided) {
                        owt.Write(" [divided]");
                    }

                    owt.WriteLine();
                }
            }

            owt.WriteLine("--------------");
        }

        public void WriteRuleApplication(KpRuleApplcation rapp) {
            owt.WriteLine("> Rule " + KpLinguaTranslator.TranslateRule(rapp.Rule, rapp.MType).Replace(" .", "") + " applied in Instance " +
                (rapp.Instance.HasName() ? rapp.Instance.Name : rapp.Instance.Id.ToString()) + " of type " + rapp.MType.Name);
        }

        public void WriteTargetSelection(KpTargetSelection kts) {
            owt.Write("? Selected ");
            int count = kts.Target.Count;
            int i = 1;
            MInstance mi = null;
            foreach (KeyValuePair<MInstance, MType> kv in kts.Target) {
                mi = kv.Key;
                if (Verbosity == 1) {
                    owt.Write("instance " + (mi.HasName() ? mi.Name : mi.Id.ToString()) + " (" + kv.Value.Name + ")");
                } else {
                    owt.Write("instance " + (mi.HasName() ? mi.Name : "") + "ID=" + mi.Id + " (" + kv.Value.Name + ")");
                }
                if (i++ < count) {
                    owt.Write(", ");
                }
            }
            owt.Write(" as targets for rule " + kts.Rule.Id + " in instance ");
            mi = kts.Instance;
            if (Verbosity == 1) {
                owt.Write(mi.HasName() ? mi.Name : mi.Id.ToString());
            } else {
                owt.Write((mi.HasName() ? mi.Name : "") + "ID=" + mi.Id);
            }
            owt.WriteLine(" of type " + kts.MType.Name);
        }
    }
}
