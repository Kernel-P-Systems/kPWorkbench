using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpLingua.Input
{
    public class KpLinguaSemanticException : Exception
    {
        private int _line;
        private int _column;

        public int Line
        {
            get { return _line; }
        }

        public int Column
        {
            get { return _column; }
        }

        public KpLinguaSemanticException(string message, int line, int column) : base(message)
        {
            _line = line;
            _column = column;
        }
    }
}
