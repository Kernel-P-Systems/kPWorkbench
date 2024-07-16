from enum import Enum


class ArithmeticOperations(Enum):
    Addition = '+'
    Subtraction = '-'
    Multiplication = '*'


class LogicalOperators(Enum):
    Greater = '>'
    GreaterOrEqual = '>='
    Less = '<'
    LessOrEqual = '<='
    Equality = '=='
    Inequality = '!='


class RelationalOperators(Enum):
    Or = '|'
    And = '&'


class Sign(Enum):
    LessOrEqual = '<='
    Less = '<'


class Base:
    def evaluate(self):
        pass


class Number(Base):

    def __init__(self, value):
        self.value = value

    def evaluate(self): return self.value

    def __repr__(self): return '{}'.format(self.value)


class Variable(Base):

    def __init__(self, name, value=0):
        self.name = name
        self.value = value

    def set_value(self, value): self.value = value

    def evaluate(self): return self.value

    def __repr__(self): return 'Variable: {} = {}'.format(self.name, self.value)


class ArithmeticExpression(Base):

    OPERATIONS = dict({
        ArithmeticOperations.Addition: lambda v1, v2: v1 + v2,
        ArithmeticOperations.Subtraction: lambda v1, v2: v1 - v2,
        ArithmeticOperations.Multiplication: lambda v1, v2: v1 * v2
    })

    def __init__(self, value1, operation, value2):
        self.value1 = value1
        self.operation = operation
        self.value2 = value2

    def evaluate(self):
        return ArithmeticExpression.OPERATIONS[self.operation](self.value1.evaluate(), self.value2.evaluate())

    def __repr__(self): return '{} {} {}'.format(self.value1, self.operation.value, self.value2)


class LogicalExpression(Base):

    EXPRESSIONS = dict({
        LogicalOperators.Greater: lambda v1, v2: v1 > v2,
        LogicalOperators.GreaterOrEqual: lambda v1, v2: v1 >= v2,
        LogicalOperators.Less: lambda v1, v2: v1 < v2,
        LogicalOperators.LessOrEqual: lambda v1, v2: v1 <= v2,
        LogicalOperators.Equality: lambda v1, v2: v1 == v2,
        LogicalOperators.Inequality: lambda v1, v2: v1 != v2
    })

    def __init__(self, value1, operation, value2):
        self.value1 = value1
        self.operation = operation
        self.value2 = value2

    def evaluate(self):
        return LogicalExpression.EXPRESSIONS[self.operation](self.value1.evaluate(), self.value2.evaluate())

    def __repr__(self): return '{} {} {}'.format(self.value1, self.operation.value, self.value2)


class RelationalExpression(Base):

    EXPRESSIONS = dict({
        RelationalOperators.Or: lambda v1, v2: v1 or v2,
        RelationalOperators.And: lambda v1, v2: v1 and v2
    })

    def __init__(self, value1, operation, value2):
        self.value1 = value1
        self.operation = operation
        self.value2 = value2

    def evaluate(self):
        return RelationalExpression.EXPRESSIONS[self.operation](self.value1.evaluate(), self.value2.evaluate())

    def __repr__(self): return '{} {} {}'.format(self.value1, self.operation.value, self.value2)


class RelationalExpressionTrue(Base):

    def evaluate(self):
        return True

    def __repr__(self): return 'True'


class Iterator(Base):

    def __init__(self, variable, minValue, minSign, maxValue, maxSign, increment=1):
        self.variable = variable
        self.minValue = minValue
        self.minSign = minSign
        self.maxValue = maxValue
        self.maxSign = maxSign
        self.increment = increment

    def init(self):
        if self.minSign == Sign.LessOrEqual:
            self.variable.set_value(self.minValue.evaluate())
        else:
            self.variable.set_value(self.minValue.evaluate() + 1)

    def hasNext(self):
        if self.maxSign == Sign.LessOrEqual:
            return self.variable.evaluate() + self.increment <= self.maxValue.evaluate()
        else:
            return self.variable.evaluate() + self.increment < self.maxValue.evaluate()

    def next(self):
        self.variable.set_value(self.variable.evaluate() + self.increment)

    def evaluate(self):
        return self.variable.evaluate()

    def __repr__(self):
        return '{} {} {} {} {}, {}'.format(self.minValue.evaluate(), self.minSign.value, self.variable,
                                       self.maxSign.value, self.maxValue.evaluate(), self.increment)
