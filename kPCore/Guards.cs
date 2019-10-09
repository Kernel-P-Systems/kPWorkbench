using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public interface IGuard {
        bool IsSatisfiedBy(Multiset ms);
    }

    public class BasicGuard : IGuard {

        private Multiset ms;
        private RelationalOperator op;

        public Multiset Multiset {
            get { return ms; }
            set {
                if (value == null) {
                    throw new MultisetNullException("Multiset cannot be null in a guard");
                }
                ms = value;
            }
        }
        public RelationalOperator Operator { get { return op; } set { op = value; } }

        public BasicGuard(Multiset multiset, RelationalOperator op) {
            Multiset = multiset;
            Operator = op;
        }

        public virtual bool IsSatisfiedBy(Multiset multiset) {
            switch (op) {
                case RelationalOperator.EQUAL: {
                        foreach (KeyValuePair<string, int> kv in ms) {
                            if (kv.Value != multiset[kv.Key]) {
                                return false;
                            }
                        }
                        return true;
                    }
                case RelationalOperator.GEQ: {
                        foreach (KeyValuePair<string, int> kv in ms) {
                            if (kv.Value > multiset[kv.Key]) {
                                return false;
                            }
                        }
                        return true;
                    }
                case RelationalOperator.GT: {
                        foreach (KeyValuePair<string, int> kv in ms) {
                            if (kv.Value >= multiset[kv.Key]) {
                                return false;
                            }
                        }
                        return true;
                    }
                case RelationalOperator.LEQ: {
                        foreach (KeyValuePair<string, int> kv in ms) {
                            if (kv.Value < multiset[kv.Key]) {
                                return false;
                            }
                        }
                        return true;
                    }
                case RelationalOperator.LT: {
                        foreach (KeyValuePair<string, int> kv in ms) {
                            if (kv.Value <= multiset[kv.Key]) {
                                return false;
                            }
                        }
                        return true;
                    }
                case RelationalOperator.NOT_EQUAL: {
                        foreach (KeyValuePair<string, int> kv in ms) {
                            if (kv.Value == multiset[kv.Key]) {
                                return false;
                            }
                        }
                        return true;
                    }

            }
            return false;
        }
    }

    /// <summary>
    /// The unary operator "NOT" is implicitly considered for this type of guard.
    /// </summary>
    public class NegatedGuard : IGuard {
        private IGuard operand;
        public IGuard Operand { get { return operand; } 
            set { if (value == null) { throw new ArgumentNullException(); } operand = value; } }

        public NegatedGuard(IGuard guard) {
            operand = guard;
        }

        public bool IsSatisfiedBy(Multiset multiset) {
            return !operand.IsSatisfiedBy(multiset);
        }
    }

    public class CompoundGuard : IGuard {

        private IGuard lhs;
        private BinaryGuardOperator op;
        private IGuard rhs;

        public IGuard Lhs {
            get { return lhs; }
            set {
                if (value == null) {
                    throw new GuardOperandException();
                }
                lhs = value;
            }
        }
        public BinaryGuardOperator Operator { get { return op; } set { op = value; } }
        public IGuard Rhs {
            get { return rhs; }
            set {
                if (value == null) {
                    throw new GuardOperandException();
                }
                rhs = value;
            }
        }

        public CompoundGuard(BinaryGuardOperator op, IGuard operand1, IGuard operand2) {
            Lhs = operand1;
            Rhs = operand2;
            Operator = op;
        }

        public bool IsSatisfiedBy(Multiset multiset) {
            if (op == BinaryGuardOperator.AND) {
                return lhs.IsSatisfiedBy(multiset) && rhs.IsSatisfiedBy(multiset);
            } else if (op == BinaryGuardOperator.OR) {
                return lhs.IsSatisfiedBy(multiset) || rhs.IsSatisfiedBy(multiset);
            }

            return false;
        }
    }

    public enum UnaryGuardOperator {
        NOT
    }

    public enum BinaryGuardOperator {
        AND, OR
    }

    public class GuardOperandException : Exception {
        public GuardOperandException()
            : base("The value of an operand cannot be null") {
        }
    }
}
