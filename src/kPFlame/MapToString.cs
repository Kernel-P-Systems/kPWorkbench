using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace KpFLAME
{
    public class MapToString
    {

        private Dictionary<int, string> objectsId;
        private List<int> keys;

        public MapToString(string fileName)
        {
            objectsId = new Dictionary<int, string>();
            Read(fileName);
            keys = objectsId.Keys.ToList();
            keys.Reverse();
        }

        public void Map(string fileName)
        {
            string str;
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                using (StreamWriter writer = new StreamWriter(fileName + ".txt"))
                {
                    str = reader.ReadLine();
                    while (str != null)
                    {
                        if (str != null && str != "")
                        {
                            foreach (int i in keys)
                            {
                                string n = string.Format("#{0}", i);
                                string s = objectsId[i];
                                str = str.Replace(n, s);
                            }
                        }
                        writer.WriteLine(str);
                        str = reader.ReadLine();
                    }
                }
            }
        }

        private void Read(string fileName)
        {
            string str;
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                str = reader.ReadToEnd();
            }
            if (str != null && str != "")
            {
                string[] lines = str.Split(new string[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] s = line.Split(new string[] {"\t"}, StringSplitOptions.RemoveEmptyEntries);
                    if (s.Length == 2)
                    {
                        int n = int.Parse(s[0]);
                        objectsId.Add(n, s[1]);
                    }
                }
            }
        }
    }
}
