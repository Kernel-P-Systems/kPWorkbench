using Antlr4.Runtime;
using KpCore;
using KpLingua.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpLingua
{
    public class KPLinguaManager
    {
        private static readonly Lazy<KPLinguaManager> _instance = new Lazy<KPLinguaManager>(() => new KPLinguaManager());

        private KPLinguaManager() { }

        public static KPLinguaManager Instance
        {
            get { return _instance.Value; }
        }

        public KPsystem Read(string filename)
        {
            try
            {
                var charStream = new AntlrFileStream(filename);

                var lexer = new KpLinguaLexer(charStream);
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new KpLinguaParser(tokenStream);
                var modelBuilder = new KpLinguaModelBuilder();

                parser.BuildParseTree = true;
                var tree = parser.kPsystem();

                var kPsystem = modelBuilder.GetKpSystem(tree);

                return kPsystem;
            }
            catch (KpLinguaSemanticException semanticException)
            {
                Console.WriteLine(string.Format("Error: {0}; Line: {1}; Column: {2}", semanticException.Message, semanticException.Line, semanticException.Column));
                
                return null;
            }
        }
    }
}
