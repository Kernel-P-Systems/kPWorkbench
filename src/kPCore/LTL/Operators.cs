
namespace KpCore.Ltl {
    public enum UnaryLtlOperator {
        EVENTUALLY,
        ALWAYS,
        NOT
    }

    public enum BinaryLtlOperator {
        AND,
        OR,
        UNTIL,
        WEAK_UNTIL,
        IMPLICATION,
        EQUIVALENCE
    }

    public enum ArithmeticOperator {
        PLUS, MINUS, TIMES, DIVIDED_BY, MODULO, EXPONENT
    }
}