﻿using System;
using System.IO;

namespace KPLinguaPreprocessing
{
    class Program
    {
        private static string source = "Source.kplt";
        private static string source2 = "SatCellDivision.kplt";
        private static string factorizationProblem = "FactorizationProblem.kplt";
        private static string factorizationProblem2 = "FactorizationProblem2.kplt";
        private static string subsetSum = "subset_sum_index.kplt";
        private static string subsetSumSkPSystems = "skPSystems_subset_sum.kplt";
        private static string partitionProblem = "PartitionProblem.kplt";
        private static string addBits = "AddBits.kplt";
        private static string multisetIterator = "MultisetIterator.kplt";
        private static string destination = "Target2.kpl";

        static void Main(string[] args)
        {
            string workingDirectory = Environment.CurrentDirectory;
            string sourceFilePath = Path.Combine(workingDirectory, multisetIterator);
            string destinationFilePath = Path.Combine(workingDirectory, @"Python\results", destination);
            Parser parser = new Parser();
            parser.Execute(sourceFilePath, destinationFilePath);
        }
    }
}
