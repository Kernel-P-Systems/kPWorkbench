namespace NuSMV
{
    public class Next
    {
        private Case caseStatement = null;

        public Case CaseStatement
        {
            get
            {
                if (this.caseStatement == null)
                    this.caseStatement = new Case();
                return this.caseStatement;
            }
            set
            {
                this.caseStatement = value;
            }
        }

        public string Result { get; set; }

        public void addCaseLine(ICaseLine caseLine)
        {
            this.CaseStatement.CaseLines.Add(caseLine);
        }
        /// <summary>
        /// Insert caseline to the specified index.
        /// </summary>
        /// <param name="caseLine"></param>
        public void addCaseLine(int index, ICaseLine caseLine)
        {
            this.CaseStatement.CaseLines.Insert(index, caseLine);
        }
        /// <summary>
        /// return the case line with id of given rule
        /// </summary>
        /// <param name="index">Rule ID</param>
        /// <returns>Case line</returns>
        public ICaseLine getCaseLine(int index)
        {
            ICaseLine returnCaseLine = null;
            foreach (var caseLine in CaseStatement.CaseLines)
            {
                if (caseLine.Rule.ID == index)
                {
                    returnCaseLine = caseLine;
                    break;
                }
            }
            return returnCaseLine;
        }

        public override string ToString()
        {
            string result = "";
            if (caseStatement == null)
                result = Result;
            else
            {
                result = this.caseStatement.ToString();
            }
            return result;
        }
    }
}