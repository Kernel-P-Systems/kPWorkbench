using System.Collections.Generic;

namespace NuSMV
{
    public class Case
    {
        public Case()
        {
            CaseLines = new List<ICaseLine>();
        }

        public List<ICaseLine> CaseLines { get; set; }

        public override string ToString()
        {
            string result = "case \n";
            if (CaseLines.Count < 1)
            {
                return "";
            }
            foreach (var item in CaseLines)
            {
                result += "\t" + item.ToString();
            }
            result += "esac";
            return result;
        }
    }

    public class UnionCases
    {
        private List<Case> unionCase = null;

        public List<Case> UnionCase
        {
            get
            {
                if (unionCase == null)
                {
                    unionCase = new List<Case>();
                    return unionCase;
                }
                else return unionCase;
            }
            set
            {
                this.unionCase = value;
            }
        }

        public UnionCases()
        {
            UnionCase = new List<Case>();
        }

        public void addCase(Case newCase)
        {
            UnionCase.Add(newCase);
        }

        public override string ToString()
        {
            string result = "";
            int count = 1;
            foreach (var item in UnionCase)
            {
                result += item.ToString();
                //if last item don't add 'union' key
                if (count != UnionCase.Count)
                {
                    result += " union ";
                }
                count++;
            }
            return result;
        }
    }
}