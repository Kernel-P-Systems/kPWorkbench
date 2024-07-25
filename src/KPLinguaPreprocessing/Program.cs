namespace KPLinguaPreprocessing
{
    class Program
    {
        private static string source = @"C:\kPWorkbench\src\KPLinguaPreprocessing\Python\source\Source include.kplt";
        private static string destination = @"C:\kPWorkbench\src\KPLinguaPreprocessing\Python\results\Target2.kpl";

        static void Main(string[] args)
        {
            Parser parser = new Parser();
            parser.Execute(source, destination);
        }
    }
}
