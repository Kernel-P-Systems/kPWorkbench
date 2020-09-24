using Antlr4.Runtime;
using KpCore;
using KpExperiment.Model;
using KpExperiment.Recognizer;
using KpLingua.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment
{
    public class KpExperimentManager
    {
        private static readonly Lazy<KpExperimentManager> _instance = new Lazy<KpExperimentManager>(() => new KpExperimentManager());

        private KpExperimentManager() { }

        public static KpExperimentManager Instance
        {
            get { return _instance.Value; }
        }

        public Experiment Read(string filename)
        {
            try
            {                
                var charStream = new AntlrFileStream(filename);

                var lexer = new KpExperimentLexer(charStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new KpExperimentParser(tokenStream);
                var modelBuilder = new KpExperimentModelBuilder();

                parser.BuildParseTree = true;
                var tree = parser.kPExpriment();

                var experiment = modelBuilder.GetExperiment(tree);

                return experiment;
            }
            catch (KpExperimentSemanticException semanticException)
            {
                Console.WriteLine(string.Format("Error: {0}; Line: {1}; Column: {2}", semanticException.Message, semanticException.Line, semanticException.Column));
                
                return null;
            }
        }
    }
}
