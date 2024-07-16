# Generated from KplIterator.g4 by ANTLR 4.10.1
from antlr4 import *
if __name__ is not None and "." in __name__:
    from .KplIteratorParser import KplIteratorParser
else:
    from KplIteratorParser import KplIteratorParser

# This class defines a complete generic visitor for a parse tree produced by KplIteratorParser.

class KplIteratorVisitor(ParseTreeVisitor):

    # Visit a parse tree produced by KplIteratorParser#parameters.
    def visitParameters(self, ctx:KplIteratorParser.ParametersContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#parameter.
    def visitParameter(self, ctx:KplIteratorParser.ParameterContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#iterators.
    def visitIterators(self, ctx:KplIteratorParser.IteratorsContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#iterator.
    def visitIterator(self, ctx:KplIteratorParser.IteratorContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#arOpExp.
    def visitArOpExp(self, ctx:KplIteratorParser.ArOpExpContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#identifier.
    def visitIdentifier(self, ctx:KplIteratorParser.IdentifierContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#loOpExp.
    def visitLoOpExp(self, ctx:KplIteratorParser.LoOpExpContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#logicalExpression.
    def visitLogicalExpression(self, ctx:KplIteratorParser.LogicalExpressionContext):
        return self.visitChildren(ctx)


    # Visit a parse tree produced by KplIteratorParser#logicalOperatorName.
    def visitLogicalOperatorName(self, ctx:KplIteratorParser.LogicalOperatorNameContext):
        return self.visitChildren(ctx)



del KplIteratorParser