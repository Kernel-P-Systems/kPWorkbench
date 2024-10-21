using System;

namespace KPLinguaPreprocessing
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: program.exe <sourceFilePath> <destinationFilePath>");
                return;
            }

            string sourceFilePath = args[0];
            string destinationFilePath = args[1];
            if (string.IsNullOrWhiteSpace(sourceFilePath) || string.IsNullOrWhiteSpace(destinationFilePath))
            {
                Console.WriteLine("Both source and destination file paths are required.");
                return;
            }

            Parser parser = new Parser();
            parser.Execute(sourceFilePath, destinationFilePath);

            Console.WriteLine("Execution completed.");
        }
    }
}
