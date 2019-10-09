using KpCore;
using KpUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpLingua {

    public class KpLinguaWriter {

        private TextWriter owt;
        public TextWriter Channel { get { return owt; } set { owt = value; } }

        private string nl = System.Environment.NewLine;

        public KpLinguaWriter(TextWriter channel) {
            if (channel == null) {
                throw new ArgumentNullException("channel");
            }
            Channel = channel;
        }

        public void Write(KPsystem kp) {
            if (!String.IsNullOrEmpty(kp.Name)) {
                owt.WriteLine("#Name: {0}", kp.Name);
                owt.WriteLine("#Description: {0}", kp.Description);
                owt.WriteLine();
            }

            foreach (MType mtype in kp.Types) {
                owt.WriteLine("@" + mtype.Name + ":");
            }
        }

        private void WriteTuple(MTuple t) {
            owt.Write(t.Multiplicity + t.Obj);
        }

        private void WriteMultiset(Multiset m) {
            if (m == null) {
                return;
            }
            owt.Write("{ ");
            foreach (KeyValuePair<string, int> mv in m) {
                owt.Write(mv.Value);
                owt.Write(mv.Key);
            }
            owt.Write(" }");
        }

        private void WriteInstance(MInstance instance) {
            owt.WriteLine();
        }

        private void WriteRule(Rule rule) {

        }
    }
}
