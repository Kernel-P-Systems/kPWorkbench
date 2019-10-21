using System;
using System.IO;

namespace NuSMV
{
    internal class Writer
    {
        private static string fileName = "";
        private static StreamWriter file = null;

        public static string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public static void WriteLine(string line)
        {
            if (file == null)
            {
                CleanFile(fileName);
            }
            file = new StreamWriter(FileName, true);
            file.WriteLine(line);
            file.Close();
        }

        public static void Write(string line)
        {
            if (file == null)
            {
                CleanFile(fileName);
            }
            using (file = new StreamWriter(FileName, true))
            {
                file.Write(line);
            }
        }

        public static void WriteLine(string line, string fileName)
        {
            //write to console
            if (String.IsNullOrEmpty(fileName))
            {
                Console.WriteLine(line);
            }
            else
            {
                //write to a file
                if (!fileName.EndsWith(".smv"))
                {
                    fileName += ".smv";
                }
                if (file == null)
                {
                    CleanFile(fileName);
                }
                FileName = fileName;
                file = new StreamWriter(FileName, true);
                file.WriteLine(line);
                file.Close();
            }
        }

        public static void CleanFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }
}