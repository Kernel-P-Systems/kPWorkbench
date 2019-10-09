using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class Symbol {
        public int Id { get; set; }
        public string Name { get; set; }

        public Symbol(string name, int id) {
            Id = id;
            Name = name;
        }
    }
}
