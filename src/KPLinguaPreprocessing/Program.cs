using System;
using System.IO;

namespace KPLinguaPreprocessing
{
    class Program
    {
        private static string source = "Source.kplt";
        private static string source2 = "SatCellDivision.kplt";
        private static string factorizationProblem = "FactorizationProblem.kplt";
        private static string factorizationProblem2 = "FactorizationProblem2.kplt";
        private static string destination = "Target2.kpl";

        static void Main(string[] args)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string sourceFilePath = Path.Combine(workingDirectory, factorizationProblem);
            string destinationFilePath = Path.Combine(workingDirectory, @"Python\results", destination);
            Parser parser = new Parser();
            parser.Execute(sourceFilePath, destinationFilePath);
        }
    }
}
