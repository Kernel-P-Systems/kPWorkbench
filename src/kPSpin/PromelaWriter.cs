using KpCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpSpin {
    public class PromelaWriter {

        private TextWriter owt;
        public TextWriter Channel { get { return owt; } set { owt = value; } }

        private string nl = System.Environment.NewLine;

        public PromelaWriter(TextWriter channel) {
            if (channel == null) {
                throw new ArgumentNullException("channel");
            }
            Channel = channel;
        }

        public void Write(KPsystem kp) {
            if (kp != null) {
                Write(new KpTranslator(kp).Translate());
            }
        }

        public void Write(PromelaModel model) {
            if (model != null) {

                foreach (ConstantDeclaration cd in model.Constants) {
                    owt.Write(cd.ToPromela());
                    owt.Write(nl);
                }

                owt.WriteLine();
                owt.WriteLine(model.MTypeDeclaration.ToPromela());
                owt.WriteLine();

                foreach (TypeDef td in model.TypeDefs) {
                    owt.Write(td.ToPromela(0));
                }

                owt.WriteLine();

                foreach (TypedDeclaration td in model.GlobalVariables) {
                    owt.WriteLine(td.ToPromela(0));
                }

                owt.WriteLine();

                if (model.PropertyTranslations.Count > 0)
                {
                    owt.WriteLine("/* LTL Properties */");

                    foreach (var property in model.PropertyTranslations)
                    {
                        owt.WriteLine(property);
                    }

                    owt.WriteLine();
                }

                foreach (Inline inlineDef in model.InlineDefinitions) {
                    owt.WriteLine(inlineDef.ToPromela(0));
                }

                owt.WriteLine();

                foreach (ProcessDef pd in model.Processes) {
                    owt.WriteLine(pd.ToPromela(0));
                }

                owt.Write(model.Init.ToPromela(0));
                owt.Flush();
                owt.Close();
            }
        }
    }
}
