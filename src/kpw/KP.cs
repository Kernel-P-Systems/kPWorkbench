using KpCore;
using KpExperiment;
using KpExperiment.Model;
using KpLingua;
using KpSpin;
using KpSpin.SpinVerificationModel;
using KpUtil;
using KpXML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kpw
{
    public class KP
    {
        public static KPsystem FromXML(string fileName)
        {
            return new KpSystemXMLReader(fileName).Read();
        }

        public static KpModel FromKpl(string fileName)
        {
            return new KpLinguaReader(fileName).Read();
            //return new KpModel(KPLinguaManager.Instance.Read(fileName));
        }

        public static Experiment FromKpx(string fileName)
        {
            return KpExperimentManager.Instance.Read(fileName);
        }

        public static void WriteKpLingua(KPsystem kp, TextWriter writer)
        {
            new KpLinguaWriter(writer).Write(kp);
        }

        public static void WriteKpLingua(KPsystem kp)
        {
            WriteKpLingua(kp, Console.Out);
        }

        public static void WriteJSON(KPsystem kp, TextWriter writer, bool formatted = false)
        {
            if (formatted)
            {
                new JsonWriter(writer).WriteFormatted(kp);
            }
            else
            {
                new JsonWriter(writer).Write(kp);
            }
        }

        public static void WriteJSON(KPsystem kp, bool formatted = false)
        {
            WriteJSON(kp, Console.Out, formatted);
        }

        public static void WritePromela(KPsystem kp, TextWriter writer)
        {
            WritePromela(kp, null, null, writer);
        }

        public static void WritePromela(KPsystem kp, Experiment kpx, PromelaTranslationParams tp, TextWriter writer)
        {
            var model = new KpTranslator(kp, kpx, tp).Translate();
            var promelaWriter = new PromelaWriter(writer);
            promelaWriter.Write(model);
        }

        public static void WriteVerificationPromelaModel(KpModel kpModel, Experiment kpExperiment, VerificationModelParams parameters, TextWriter writer)
        {
            new VerificationModelWriter(writer, parameters).Write(kpModel, kpExperiment);
        }
    }
}
