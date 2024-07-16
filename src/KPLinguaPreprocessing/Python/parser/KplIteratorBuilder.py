from antlr4 import InputStream, CommonTokenStream

from tools.kPLExtension.Models import Variable, Number, ArithmeticExpression, ArithmeticOperations, Iterator, Sign, \
    LogicalOperators, LogicalExpression, RelationalExpression, RelationalOperators, RelationalExpressionTrue
from tools.kPLExtension.parser.grammar.KplIteratorLexer import KplIteratorLexer
from tools.kPLExtension.parser.grammar.KplIteratorParser import KplIteratorParser
from tools.kPLExtension.parser.grammar.KplIteratorVisitor import KplIteratorVisitor


class KplIteratorBuilder(KplIteratorVisitor):

    def __init__(self, variables):
        self.variables = variables

    def visitIterators(self, ctx:KplIteratorParser.IteratorsContext):
        iterators = list(map(lambda a: self.visit(a), ctx.children))
        iterators = list(filter(lambda a: a is not None, iterators))
        iterators = list(filter(lambda a: isinstance(a, Iterator), iterators))
        restrictions = RelationalExpressionTrue()
        if ctx.restrictions is not None:
            restrictions = self.visit(ctx.restrictions)
        return iterators, restrictions

    def visitIterator(self, ctx:KplIteratorParser.IteratorContext):
        leftInq = Sign(ctx.leftInq.text)
        rightInq = Sign(ctx.rightInq.text)
        e1 = self.visit(ctx.left)
        e2 = self.visit(ctx.right)
        identifier = ctx.Identifier().getText()
        increment = 1
        if ctx.increment is not None:
            increment = int(ctx.increment.text)
        if identifier in self.variables:
            variable = self.variables[identifier]
        else:
            variable = Variable(identifier)
            self.variables[identifier] = variable
        return Iterator(variable, e1, leftInq, e2, rightInq, increment)

    def visitArOpExp(self, ctx:KplIteratorParser.ArOpExpContext):
        if ctx.op is not None:
            o = ctx.op.text
            e1 = self.visit(ctx.left)
            e2 = self.visit(ctx.right)
            return ArithmeticExpression(e1, ArithmeticOperations(o), e2)
        elif ctx.Number() is not None:
            return Number(int(ctx.getText()))
        elif ctx.identifierOp is not None:
            identifier = self.visit(ctx.identifierOp)
            return identifier

    def visitLoOpExp(self, ctx:KplIteratorParser.LoOpExpContext):
        if ctx.op is not None:
            op = RelationalOperators(ctx.op.text)
            e1 = self.visit(ctx.left)
            e2 = self.visit(ctx.right)
            return RelationalExpression(e1, op, e2)
        else:
            return self.visit(ctx.lo)

    def visitLogicalExpression(self, ctx:KplIteratorParser.LogicalExpressionContext):
        ex1 = self.visit(ctx.left)
        op = self.visit(ctx.lo)
        ex2 = self.visit(ctx.right)
        return LogicalExpression(ex1, op, ex2)

    def visitLogicalOperatorName(self, ctx:KplIteratorParser.LogicalOperatorNameContext):
        return LogicalOperators(ctx.getText())

    def visitIdentifier(self, ctx:KplIteratorParser.ArOpExpContext):
        identifier = ctx.Identifier().getText()
        if identifier in self.variables:
            variable = self.variables[identifier]
        else:
            variable = Variable(identifier)
            self.variables[identifier] = variable
        return variable

    def visitParameters(self, ctx:KplIteratorParser.ParametersContext):
        variables = list(map(lambda a: self.visit(a), ctx.children))
        variables = list(filter(lambda a: a is not None, variables))
        return variables

    def visitParameter(self, ctx:KplIteratorParser.ParameterContext):
        identifier = ctx.Identifier().getText()
        number = int(ctx.Number().getText()) if ctx.Number() is not None else 0
        return Variable(identifier, number)

    def buildParameters(self, text):
        data = InputStream(text)
        lexer = KplIteratorLexer(data)
        stream = CommonTokenStream(lexer=lexer)
        parser = KplIteratorParser(stream)
        tree = parser.parameters()
        variables = self.visitParameters(tree)
        return variables

    def buildExpression(self, text):
        data = InputStream(text)
        lexer = KplIteratorLexer(data)
        stream = CommonTokenStream(lexer=lexer)
        parser = KplIteratorParser(stream)
        tree = parser.arithmeticExpression()
        s = self.visitArOpExp(tree)
        return s

    def buildLogicalExpression(self, text):
        data = InputStream(text)
        lexer = KplIteratorLexer(data)
        stream = CommonTokenStream(lexer=lexer)
        parser = KplIteratorParser(stream)
        tree = parser.relationalExpression()
        s = self.visitLoOpExp(tree)
        return s

    def buildIterators(self, text):
        data = InputStream(text)
        lexer = KplIteratorLexer(data)
        stream = CommonTokenStream(lexer=lexer)
        parser = KplIteratorParser(stream)
        tree = parser.iterators()
        iterators, restrictions = self.visitIterators(tree)
        return iterators, restrictions, self.variables


# builder = KplIteratorBuilder({})
# s = builder.buildParameters('i=100, o, oo=1')
# print(s)
# s = builder.buildExpression('i+100+10*10+10')
# s = builder.buildExpression('i')
# print(s)
# s = builder.buildLogicalExpression('10>100&10>100|10<1000')
# print(s, s.evaluate())
# iterators, restrictions, variables = builder.buildIterators('1<o<10, 10+o<a<100,20, 10<p<100,i<100')
# print(iterators)
# print(restrictions)
# print(variables)

