# Generated from KplIterator.g4 by ANTLR 4.10.1
# encoding: utf-8
from antlr4 import *
from io import StringIO
import sys
if sys.version_info[1] > 5:
	from typing import TextIO
else:
	from typing.io import TextIO

def serializedATN():
    return [
        4,1,16,97,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
        6,2,7,7,7,2,8,7,8,1,0,1,0,1,0,5,0,22,8,0,10,0,12,0,25,9,0,1,1,1,
        1,1,1,3,1,30,8,1,1,2,1,2,1,2,5,2,35,8,2,10,2,12,2,38,9,2,1,2,1,2,
        3,2,42,8,2,1,3,1,3,1,3,1,3,1,3,1,3,1,3,3,3,51,8,3,1,4,1,4,1,4,3,
        4,56,8,4,1,4,1,4,1,4,1,4,1,4,1,4,5,4,64,8,4,10,4,12,4,67,9,4,1,5,
        1,5,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,1,6,5,6,80,8,6,10,6,12,6,83,
        9,6,1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,8,1,8,1,8,3,8,95,8,8,1,8,0,2,8,
        12,9,0,2,4,6,8,10,12,14,16,0,2,1,0,8,9,1,0,3,4,102,0,18,1,0,0,0,
        2,26,1,0,0,0,4,31,1,0,0,0,6,43,1,0,0,0,8,55,1,0,0,0,10,68,1,0,0,
        0,12,70,1,0,0,0,14,84,1,0,0,0,16,94,1,0,0,0,18,23,3,2,1,0,19,20,
        5,1,0,0,20,22,3,2,1,0,21,19,1,0,0,0,22,25,1,0,0,0,23,21,1,0,0,0,
        23,24,1,0,0,0,24,1,1,0,0,0,25,23,1,0,0,0,26,29,5,15,0,0,27,28,5,
        2,0,0,28,30,5,14,0,0,29,27,1,0,0,0,29,30,1,0,0,0,30,3,1,0,0,0,31,
        36,3,6,3,0,32,33,5,1,0,0,33,35,3,6,3,0,34,32,1,0,0,0,35,38,1,0,0,
        0,36,34,1,0,0,0,36,37,1,0,0,0,37,41,1,0,0,0,38,36,1,0,0,0,39,40,
        5,1,0,0,40,42,3,12,6,0,41,39,1,0,0,0,41,42,1,0,0,0,42,5,1,0,0,0,
        43,44,3,8,4,0,44,45,7,0,0,0,45,46,5,15,0,0,46,47,7,0,0,0,47,50,3,
        8,4,0,48,49,5,1,0,0,49,51,5,14,0,0,50,48,1,0,0,0,50,51,1,0,0,0,51,
        7,1,0,0,0,52,53,6,4,-1,0,53,56,5,14,0,0,54,56,3,10,5,0,55,52,1,0,
        0,0,55,54,1,0,0,0,56,65,1,0,0,0,57,58,10,4,0,0,58,59,5,5,0,0,59,
        64,3,8,4,5,60,61,10,3,0,0,61,62,7,1,0,0,62,64,3,8,4,4,63,57,1,0,
        0,0,63,60,1,0,0,0,64,67,1,0,0,0,65,63,1,0,0,0,65,66,1,0,0,0,66,9,
        1,0,0,0,67,65,1,0,0,0,68,69,5,15,0,0,69,11,1,0,0,0,70,71,6,6,-1,
        0,71,72,3,14,7,0,72,81,1,0,0,0,73,74,10,3,0,0,74,75,5,13,0,0,75,
        80,3,12,6,4,76,77,10,2,0,0,77,78,5,12,0,0,78,80,3,12,6,3,79,73,1,
        0,0,0,79,76,1,0,0,0,80,83,1,0,0,0,81,79,1,0,0,0,81,82,1,0,0,0,82,
        13,1,0,0,0,83,81,1,0,0,0,84,85,3,8,4,0,85,86,3,16,8,0,86,87,3,8,
        4,0,87,15,1,0,0,0,88,95,5,6,0,0,89,95,5,7,0,0,90,95,5,8,0,0,91,95,
        5,9,0,0,92,95,5,10,0,0,93,95,5,11,0,0,94,88,1,0,0,0,94,89,1,0,0,
        0,94,90,1,0,0,0,94,91,1,0,0,0,94,92,1,0,0,0,94,93,1,0,0,0,95,17,
        1,0,0,0,11,23,29,36,41,50,55,63,65,79,81,94
    ]

class KplIteratorParser ( Parser ):

    grammarFileName = "KplIterator.g4"

    atn = ATNDeserializer().deserialize(serializedATN())

    decisionsToDFA = [ DFA(ds, i) for i, ds in enumerate(atn.decisionToState) ]

    sharedContextCache = PredictionContextCache()

    literalNames = [ "<INVALID>", "','", "'='", "'+'", "'-'", "'*'", "'>'", 
                     "'>='", "'<'", "'<='", "'=='", "'!='", "'|'", "'&'" ]

    symbolicNames = [ "<INVALID>", "<INVALID>", "<INVALID>", "Add", "Sub", 
                      "Mul", "Gt", "Gte", "Lt", "Lte", "Eq", "Neq", "Or", 
                      "And", "Number", "Identifier", "WS" ]

    RULE_parameters = 0
    RULE_parameter = 1
    RULE_iterators = 2
    RULE_iterator = 3
    RULE_arithmeticExpression = 4
    RULE_identifier = 5
    RULE_relationalExpression = 6
    RULE_logicalExpression = 7
    RULE_logicalOperator = 8

    ruleNames =  [ "parameters", "parameter", "iterators", "iterator", "arithmeticExpression", 
                   "identifier", "relationalExpression", "logicalExpression", 
                   "logicalOperator" ]

    EOF = Token.EOF
    T__0=1
    T__1=2
    Add=3
    Sub=4
    Mul=5
    Gt=6
    Gte=7
    Lt=8
    Lte=9
    Eq=10
    Neq=11
    Or=12
    And=13
    Number=14
    Identifier=15
    WS=16

    def __init__(self, input:TokenStream, output:TextIO = sys.stdout):
        super().__init__(input, output)
        self.checkVersion("4.10.1")
        self._interp = ParserATNSimulator(self, self.atn, self.decisionsToDFA, self.sharedContextCache)
        self._predicates = None




    class ParametersContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser

        def parameter(self, i:int=None):
            if i is None:
                return self.getTypedRuleContexts(KplIteratorParser.ParameterContext)
            else:
                return self.getTypedRuleContext(KplIteratorParser.ParameterContext,i)


        def getRuleIndex(self):
            return KplIteratorParser.RULE_parameters

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitParameters" ):
                return visitor.visitParameters(self)
            else:
                return visitor.visitChildren(self)




    def parameters(self):

        localctx = KplIteratorParser.ParametersContext(self, self._ctx, self.state)
        self.enterRule(localctx, 0, self.RULE_parameters)
        self._la = 0 # Token type
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 18
            self.parameter()
            self.state = 23
            self._errHandler.sync(self)
            _la = self._input.LA(1)
            while _la==KplIteratorParser.T__0:
                self.state = 19
                self.match(KplIteratorParser.T__0)
                self.state = 20
                self.parameter()
                self.state = 25
                self._errHandler.sync(self)
                _la = self._input.LA(1)

        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx


    class ParameterContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser

        def Identifier(self):
            return self.getToken(KplIteratorParser.Identifier, 0)

        def Number(self):
            return self.getToken(KplIteratorParser.Number, 0)

        def getRuleIndex(self):
            return KplIteratorParser.RULE_parameter

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitParameter" ):
                return visitor.visitParameter(self)
            else:
                return visitor.visitChildren(self)




    def parameter(self):

        localctx = KplIteratorParser.ParameterContext(self, self._ctx, self.state)
        self.enterRule(localctx, 2, self.RULE_parameter)
        self._la = 0 # Token type
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 26
            self.match(KplIteratorParser.Identifier)
            self.state = 29
            self._errHandler.sync(self)
            _la = self._input.LA(1)
            if _la==KplIteratorParser.T__1:
                self.state = 27
                self.match(KplIteratorParser.T__1)
                self.state = 28
                self.match(KplIteratorParser.Number)


        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx


    class IteratorsContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser
            self.restrictions = None # RelationalExpressionContext

        def iterator(self, i:int=None):
            if i is None:
                return self.getTypedRuleContexts(KplIteratorParser.IteratorContext)
            else:
                return self.getTypedRuleContext(KplIteratorParser.IteratorContext,i)


        def relationalExpression(self):
            return self.getTypedRuleContext(KplIteratorParser.RelationalExpressionContext,0)


        def getRuleIndex(self):
            return KplIteratorParser.RULE_iterators

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitIterators" ):
                return visitor.visitIterators(self)
            else:
                return visitor.visitChildren(self)




    def iterators(self):

        localctx = KplIteratorParser.IteratorsContext(self, self._ctx, self.state)
        self.enterRule(localctx, 4, self.RULE_iterators)
        self._la = 0 # Token type
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 31
            self.iterator()
            self.state = 36
            self._errHandler.sync(self)
            _alt = self._interp.adaptivePredict(self._input,2,self._ctx)
            while _alt!=2 and _alt!=ATN.INVALID_ALT_NUMBER:
                if _alt==1:
                    self.state = 32
                    self.match(KplIteratorParser.T__0)
                    self.state = 33
                    self.iterator() 
                self.state = 38
                self._errHandler.sync(self)
                _alt = self._interp.adaptivePredict(self._input,2,self._ctx)

            self.state = 41
            self._errHandler.sync(self)
            _la = self._input.LA(1)
            if _la==KplIteratorParser.T__0:
                self.state = 39
                self.match(KplIteratorParser.T__0)
                self.state = 40
                localctx.restrictions = self.relationalExpression(0)


        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx


    class IteratorContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser
            self.left = None # ArithmeticExpressionContext
            self.leftInq = None # Token
            self.rightInq = None # Token
            self.right = None # ArithmeticExpressionContext
            self.increment = None # Token

        def Identifier(self):
            return self.getToken(KplIteratorParser.Identifier, 0)

        def arithmeticExpression(self, i:int=None):
            if i is None:
                return self.getTypedRuleContexts(KplIteratorParser.ArithmeticExpressionContext)
            else:
                return self.getTypedRuleContext(KplIteratorParser.ArithmeticExpressionContext,i)


        def Lte(self, i:int=None):
            if i is None:
                return self.getTokens(KplIteratorParser.Lte)
            else:
                return self.getToken(KplIteratorParser.Lte, i)

        def Lt(self, i:int=None):
            if i is None:
                return self.getTokens(KplIteratorParser.Lt)
            else:
                return self.getToken(KplIteratorParser.Lt, i)

        def Number(self):
            return self.getToken(KplIteratorParser.Number, 0)

        def getRuleIndex(self):
            return KplIteratorParser.RULE_iterator

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitIterator" ):
                return visitor.visitIterator(self)
            else:
                return visitor.visitChildren(self)




    def iterator(self):

        localctx = KplIteratorParser.IteratorContext(self, self._ctx, self.state)
        self.enterRule(localctx, 6, self.RULE_iterator)
        self._la = 0 # Token type
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 43
            localctx.left = self.arithmeticExpression(0)
            self.state = 44
            localctx.leftInq = self._input.LT(1)
            _la = self._input.LA(1)
            if not(_la==KplIteratorParser.Lt or _la==KplIteratorParser.Lte):
                localctx.leftInq = self._errHandler.recoverInline(self)
            else:
                self._errHandler.reportMatch(self)
                self.consume()
            self.state = 45
            self.match(KplIteratorParser.Identifier)
            self.state = 46
            localctx.rightInq = self._input.LT(1)
            _la = self._input.LA(1)
            if not(_la==KplIteratorParser.Lt or _la==KplIteratorParser.Lte):
                localctx.rightInq = self._errHandler.recoverInline(self)
            else:
                self._errHandler.reportMatch(self)
                self.consume()
            self.state = 47
            localctx.right = self.arithmeticExpression(0)
            self.state = 50
            self._errHandler.sync(self)
            la_ = self._interp.adaptivePredict(self._input,4,self._ctx)
            if la_ == 1:
                self.state = 48
                self.match(KplIteratorParser.T__0)
                self.state = 49
                localctx.increment = self.match(KplIteratorParser.Number)


        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx


    class ArithmeticExpressionContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser


        def getRuleIndex(self):
            return KplIteratorParser.RULE_arithmeticExpression

     
        def copyFrom(self, ctx:ParserRuleContext):
            super().copyFrom(ctx)


    class ArOpExpContext(ArithmeticExpressionContext):

        def __init__(self, parser, ctx:ParserRuleContext): # actually a KplIteratorParser.ArithmeticExpressionContext
            super().__init__(parser)
            self.left = None # ArithmeticExpressionContext
            self.identifierOp = None # IdentifierContext
            self.op = None # Token
            self.right = None # ArithmeticExpressionContext
            self.copyFrom(ctx)

        def Number(self):
            return self.getToken(KplIteratorParser.Number, 0)
        def identifier(self):
            return self.getTypedRuleContext(KplIteratorParser.IdentifierContext,0)

        def arithmeticExpression(self, i:int=None):
            if i is None:
                return self.getTypedRuleContexts(KplIteratorParser.ArithmeticExpressionContext)
            else:
                return self.getTypedRuleContext(KplIteratorParser.ArithmeticExpressionContext,i)

        def Mul(self):
            return self.getToken(KplIteratorParser.Mul, 0)
        def Add(self):
            return self.getToken(KplIteratorParser.Add, 0)
        def Sub(self):
            return self.getToken(KplIteratorParser.Sub, 0)

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitArOpExp" ):
                return visitor.visitArOpExp(self)
            else:
                return visitor.visitChildren(self)



    def arithmeticExpression(self, _p:int=0):
        _parentctx = self._ctx
        _parentState = self.state
        localctx = KplIteratorParser.ArithmeticExpressionContext(self, self._ctx, _parentState)
        _prevctx = localctx
        _startState = 8
        self.enterRecursionRule(localctx, 8, self.RULE_arithmeticExpression, _p)
        self._la = 0 # Token type
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 55
            self._errHandler.sync(self)
            token = self._input.LA(1)
            if token in [KplIteratorParser.Number]:
                localctx = KplIteratorParser.ArOpExpContext(self, localctx)
                self._ctx = localctx
                _prevctx = localctx

                self.state = 53
                self.match(KplIteratorParser.Number)
                pass
            elif token in [KplIteratorParser.Identifier]:
                localctx = KplIteratorParser.ArOpExpContext(self, localctx)
                self._ctx = localctx
                _prevctx = localctx
                self.state = 54
                localctx.identifierOp = self.identifier()
                pass
            else:
                raise NoViableAltException(self)

            self._ctx.stop = self._input.LT(-1)
            self.state = 65
            self._errHandler.sync(self)
            _alt = self._interp.adaptivePredict(self._input,7,self._ctx)
            while _alt!=2 and _alt!=ATN.INVALID_ALT_NUMBER:
                if _alt==1:
                    if self._parseListeners is not None:
                        self.triggerExitRuleEvent()
                    _prevctx = localctx
                    self.state = 63
                    self._errHandler.sync(self)
                    la_ = self._interp.adaptivePredict(self._input,6,self._ctx)
                    if la_ == 1:
                        localctx = KplIteratorParser.ArOpExpContext(self, KplIteratorParser.ArithmeticExpressionContext(self, _parentctx, _parentState))
                        localctx.left = _prevctx
                        self.pushNewRecursionContext(localctx, _startState, self.RULE_arithmeticExpression)
                        self.state = 57
                        if not self.precpred(self._ctx, 4):
                            from antlr4.error.Errors import FailedPredicateException
                            raise FailedPredicateException(self, "self.precpred(self._ctx, 4)")
                        self.state = 58
                        localctx.op = self.match(KplIteratorParser.Mul)
                        self.state = 59
                        localctx.right = self.arithmeticExpression(5)
                        pass

                    elif la_ == 2:
                        localctx = KplIteratorParser.ArOpExpContext(self, KplIteratorParser.ArithmeticExpressionContext(self, _parentctx, _parentState))
                        localctx.left = _prevctx
                        self.pushNewRecursionContext(localctx, _startState, self.RULE_arithmeticExpression)
                        self.state = 60
                        if not self.precpred(self._ctx, 3):
                            from antlr4.error.Errors import FailedPredicateException
                            raise FailedPredicateException(self, "self.precpred(self._ctx, 3)")
                        self.state = 61
                        localctx.op = self._input.LT(1)
                        _la = self._input.LA(1)
                        if not(_la==KplIteratorParser.Add or _la==KplIteratorParser.Sub):
                            localctx.op = self._errHandler.recoverInline(self)
                        else:
                            self._errHandler.reportMatch(self)
                            self.consume()
                        self.state = 62
                        localctx.right = self.arithmeticExpression(4)
                        pass

             
                self.state = 67
                self._errHandler.sync(self)
                _alt = self._interp.adaptivePredict(self._input,7,self._ctx)

        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.unrollRecursionContexts(_parentctx)
        return localctx


    class IdentifierContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser

        def Identifier(self):
            return self.getToken(KplIteratorParser.Identifier, 0)

        def getRuleIndex(self):
            return KplIteratorParser.RULE_identifier

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitIdentifier" ):
                return visitor.visitIdentifier(self)
            else:
                return visitor.visitChildren(self)




    def identifier(self):

        localctx = KplIteratorParser.IdentifierContext(self, self._ctx, self.state)
        self.enterRule(localctx, 10, self.RULE_identifier)
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 68
            self.match(KplIteratorParser.Identifier)
        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx


    class RelationalExpressionContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser


        def getRuleIndex(self):
            return KplIteratorParser.RULE_relationalExpression

     
        def copyFrom(self, ctx:ParserRuleContext):
            super().copyFrom(ctx)


    class LoOpExpContext(RelationalExpressionContext):

        def __init__(self, parser, ctx:ParserRuleContext): # actually a KplIteratorParser.RelationalExpressionContext
            super().__init__(parser)
            self.left = None # RelationalExpressionContext
            self.lo = None # LogicalExpressionContext
            self.op = None # Token
            self.right = None # RelationalExpressionContext
            self.copyFrom(ctx)

        def logicalExpression(self):
            return self.getTypedRuleContext(KplIteratorParser.LogicalExpressionContext,0)

        def relationalExpression(self, i:int=None):
            if i is None:
                return self.getTypedRuleContexts(KplIteratorParser.RelationalExpressionContext)
            else:
                return self.getTypedRuleContext(KplIteratorParser.RelationalExpressionContext,i)

        def And(self):
            return self.getToken(KplIteratorParser.And, 0)
        def Or(self):
            return self.getToken(KplIteratorParser.Or, 0)

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitLoOpExp" ):
                return visitor.visitLoOpExp(self)
            else:
                return visitor.visitChildren(self)



    def relationalExpression(self, _p:int=0):
        _parentctx = self._ctx
        _parentState = self.state
        localctx = KplIteratorParser.RelationalExpressionContext(self, self._ctx, _parentState)
        _prevctx = localctx
        _startState = 12
        self.enterRecursionRule(localctx, 12, self.RULE_relationalExpression, _p)
        try:
            self.enterOuterAlt(localctx, 1)
            localctx = KplIteratorParser.LoOpExpContext(self, localctx)
            self._ctx = localctx
            _prevctx = localctx

            self.state = 71
            localctx.lo = self.logicalExpression()
            self._ctx.stop = self._input.LT(-1)
            self.state = 81
            self._errHandler.sync(self)
            _alt = self._interp.adaptivePredict(self._input,9,self._ctx)
            while _alt!=2 and _alt!=ATN.INVALID_ALT_NUMBER:
                if _alt==1:
                    if self._parseListeners is not None:
                        self.triggerExitRuleEvent()
                    _prevctx = localctx
                    self.state = 79
                    self._errHandler.sync(self)
                    la_ = self._interp.adaptivePredict(self._input,8,self._ctx)
                    if la_ == 1:
                        localctx = KplIteratorParser.LoOpExpContext(self, KplIteratorParser.RelationalExpressionContext(self, _parentctx, _parentState))
                        localctx.left = _prevctx
                        self.pushNewRecursionContext(localctx, _startState, self.RULE_relationalExpression)
                        self.state = 73
                        if not self.precpred(self._ctx, 3):
                            from antlr4.error.Errors import FailedPredicateException
                            raise FailedPredicateException(self, "self.precpred(self._ctx, 3)")
                        self.state = 74
                        localctx.op = self.match(KplIteratorParser.And)
                        self.state = 75
                        localctx.right = self.relationalExpression(4)
                        pass

                    elif la_ == 2:
                        localctx = KplIteratorParser.LoOpExpContext(self, KplIteratorParser.RelationalExpressionContext(self, _parentctx, _parentState))
                        localctx.left = _prevctx
                        self.pushNewRecursionContext(localctx, _startState, self.RULE_relationalExpression)
                        self.state = 76
                        if not self.precpred(self._ctx, 2):
                            from antlr4.error.Errors import FailedPredicateException
                            raise FailedPredicateException(self, "self.precpred(self._ctx, 2)")
                        self.state = 77
                        localctx.op = self.match(KplIteratorParser.Or)
                        self.state = 78
                        localctx.right = self.relationalExpression(3)
                        pass

             
                self.state = 83
                self._errHandler.sync(self)
                _alt = self._interp.adaptivePredict(self._input,9,self._ctx)

        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.unrollRecursionContexts(_parentctx)
        return localctx


    class LogicalExpressionContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser
            self.left = None # ArithmeticExpressionContext
            self.lo = None # LogicalOperatorContext
            self.right = None # ArithmeticExpressionContext

        def arithmeticExpression(self, i:int=None):
            if i is None:
                return self.getTypedRuleContexts(KplIteratorParser.ArithmeticExpressionContext)
            else:
                return self.getTypedRuleContext(KplIteratorParser.ArithmeticExpressionContext,i)


        def logicalOperator(self):
            return self.getTypedRuleContext(KplIteratorParser.LogicalOperatorContext,0)


        def getRuleIndex(self):
            return KplIteratorParser.RULE_logicalExpression

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitLogicalExpression" ):
                return visitor.visitLogicalExpression(self)
            else:
                return visitor.visitChildren(self)




    def logicalExpression(self):

        localctx = KplIteratorParser.LogicalExpressionContext(self, self._ctx, self.state)
        self.enterRule(localctx, 14, self.RULE_logicalExpression)
        try:
            self.enterOuterAlt(localctx, 1)
            self.state = 84
            localctx.left = self.arithmeticExpression(0)
            self.state = 85
            localctx.lo = self.logicalOperator()
            self.state = 86
            localctx.right = self.arithmeticExpression(0)
        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx


    class LogicalOperatorContext(ParserRuleContext):
        __slots__ = 'parser'

        def __init__(self, parser, parent:ParserRuleContext=None, invokingState:int=-1):
            super().__init__(parent, invokingState)
            self.parser = parser


        def getRuleIndex(self):
            return KplIteratorParser.RULE_logicalOperator

     
        def copyFrom(self, ctx:ParserRuleContext):
            super().copyFrom(ctx)



    class LogicalOperatorNameContext(LogicalOperatorContext):

        def __init__(self, parser, ctx:ParserRuleContext): # actually a KplIteratorParser.LogicalOperatorContext
            super().__init__(parser)
            self.copyFrom(ctx)

        def Gt(self):
            return self.getToken(KplIteratorParser.Gt, 0)
        def Gte(self):
            return self.getToken(KplIteratorParser.Gte, 0)
        def Lt(self):
            return self.getToken(KplIteratorParser.Lt, 0)
        def Lte(self):
            return self.getToken(KplIteratorParser.Lte, 0)
        def Eq(self):
            return self.getToken(KplIteratorParser.Eq, 0)
        def Neq(self):
            return self.getToken(KplIteratorParser.Neq, 0)

        def accept(self, visitor:ParseTreeVisitor):
            if hasattr( visitor, "visitLogicalOperatorName" ):
                return visitor.visitLogicalOperatorName(self)
            else:
                return visitor.visitChildren(self)



    def logicalOperator(self):

        localctx = KplIteratorParser.LogicalOperatorContext(self, self._ctx, self.state)
        self.enterRule(localctx, 16, self.RULE_logicalOperator)
        try:
            self.state = 94
            self._errHandler.sync(self)
            token = self._input.LA(1)
            if token in [KplIteratorParser.Gt]:
                localctx = KplIteratorParser.LogicalOperatorNameContext(self, localctx)
                self.enterOuterAlt(localctx, 1)
                self.state = 88
                self.match(KplIteratorParser.Gt)
                pass
            elif token in [KplIteratorParser.Gte]:
                localctx = KplIteratorParser.LogicalOperatorNameContext(self, localctx)
                self.enterOuterAlt(localctx, 2)
                self.state = 89
                self.match(KplIteratorParser.Gte)
                pass
            elif token in [KplIteratorParser.Lt]:
                localctx = KplIteratorParser.LogicalOperatorNameContext(self, localctx)
                self.enterOuterAlt(localctx, 3)
                self.state = 90
                self.match(KplIteratorParser.Lt)
                pass
            elif token in [KplIteratorParser.Lte]:
                localctx = KplIteratorParser.LogicalOperatorNameContext(self, localctx)
                self.enterOuterAlt(localctx, 4)
                self.state = 91
                self.match(KplIteratorParser.Lte)
                pass
            elif token in [KplIteratorParser.Eq]:
                localctx = KplIteratorParser.LogicalOperatorNameContext(self, localctx)
                self.enterOuterAlt(localctx, 5)
                self.state = 92
                self.match(KplIteratorParser.Eq)
                pass
            elif token in [KplIteratorParser.Neq]:
                localctx = KplIteratorParser.LogicalOperatorNameContext(self, localctx)
                self.enterOuterAlt(localctx, 6)
                self.state = 93
                self.match(KplIteratorParser.Neq)
                pass
            else:
                raise NoViableAltException(self)

        except RecognitionException as re:
            localctx.exception = re
            self._errHandler.reportError(self, re)
            self._errHandler.recover(self, re)
        finally:
            self.exitRule()
        return localctx



    def sempred(self, localctx:RuleContext, ruleIndex:int, predIndex:int):
        if self._predicates == None:
            self._predicates = dict()
        self._predicates[4] = self.arithmeticExpression_sempred
        self._predicates[6] = self.relationalExpression_sempred
        pred = self._predicates.get(ruleIndex, None)
        if pred is None:
            raise Exception("No predicate with index:" + str(ruleIndex))
        else:
            return pred(localctx, predIndex)

    def arithmeticExpression_sempred(self, localctx:ArithmeticExpressionContext, predIndex:int):
            if predIndex == 0:
                return self.precpred(self._ctx, 4)
         

            if predIndex == 1:
                return self.precpred(self._ctx, 3)
         

    def relationalExpression_sempred(self, localctx:RelationalExpressionContext, predIndex:int):
            if predIndex == 2:
                return self.precpred(self._ctx, 3)
         

            if predIndex == 3:
                return self.precpred(self._ctx, 2)
         




