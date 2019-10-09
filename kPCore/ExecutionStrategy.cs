using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public class ExecutionStrategy {

        private List<Rule> rules;
        public List<Rule> Rules { get { return rules; } }
        public StrategyOperator Operator { get; set; }

        private ExecutionStrategy next;
        public ExecutionStrategy Next { get { return next; } set { next = value; } }

        public ExecutionStrategy(params Rule[] rs)
            : this(StrategyOperator.SEQUENCE, rs) {
        }

        public ExecutionStrategy(StrategyOperator op, params Rule[] rs) {
            rules = new List<Rule>();
            if (rules != null) {
                rules.AddRange(rs);
            }
            Operator = op;
            next = null;
        }

        public ExecutionStrategy Append(ExecutionStrategy ex) {
            if (ex == null) {
                return this;
            }
            ExecutionStrategy es = this;
            while (es.Next != null) {
                es = es.Next;
            }
            es.Next = ex;

            return ex;
        }

        public bool IsEmpty() {
            if (rules.Count > 0) {
                return false;
            } else {
                ExecutionStrategy x = Next;
                while (x != null) {
                    if (x.Rules.Count != 0) {
                        return false;
                    }
                    x = x.Next;
                }
            }

            return true;
        }
    }

    public enum StrategyOperator {
        SEQUENCE, MAX, CHOICE, ARBITRARY
    }
}
