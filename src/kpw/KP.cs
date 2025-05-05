using KpCore;
using KpExperiment;
using KpExperiment.Model;
using KpLingua;
using KPLinguaPreprocessing;
using KpSpin;
using KpSpin.SpinVerificationModel;
using KpUtil;
using KpXML;
using System;
using System.IO;

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
            try
            {
                IndexationParser indexationParser = new IndexationParser();
                string afterIndexationFile = indexationParser.Execute(fileName);
                if (!string.IsNullOrEmpty(afterIndexationFile))
                {
                    return new KpLinguaReader(afterIndexationFile).Read();
                }
            }
            catch(Exception exception)
            {
                Console.WriteLine($"Cannot run the indexation on this file {exception}");
            }

            return new KpLinguaReader(fileName).Read();
            //return KPLinguaManager.Instance.Read(fileName);
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
