/* The ANTLRv4 grammar for .kpx files */

grammar KpExperiment;

@parser::members
{
        protected const int EOF = Eof;
}

@lexer::members
{
	protected const int EOF = Eof;
	protected const int HIDDEN = Hidden;
}

tokens {  	
}

// Grammar rules for parsing the Kernel P systems experiments

kPExpriment    
    :   (ltlProperty | ctlProperty)+ EOF
    ;

ltlProperty
    :   'ltl' ':' ltlExpression ';'
    ;

ctlProperty
    :   'ctl' ':' ctlExpression ';'
    ;

ltlExpression   
    :   unaryExpression 
    |   binaryExpression
    ;

ctlExpression   
    :   unaryExpression 
    |   binaryExpression
    ;

unaryExpression
    :   notLtlExpression
	|	eventuallyExpression
    |   alwaysExpression
	|	nextExpression
	|	neverExpression
	|	infinitelyOftenExpression
	|	steadyStateExpression 
    ;

binaryExpression
    :   untilExpression
    |   weakUntilExpression
	|	followedByExpression
	|	precededByExpression
    ;

notLtlExpression
    :   'not' ltlExpression
    ;

alwaysExpression
    :   'always' equivalenceExpression
    ;

eventuallyExpression
    :   'eventually' equivalenceExpression
    ;

nextExpression
	:   'next' equivalenceExpression
    ;

neverExpression
	:   'never' equivalenceExpression
    ;

infinitelyOftenExpression
	:   'infinitely-often' equivalenceExpression
    ;

steadyStateExpression
	:   'steady-state' equivalenceExpression
    ;

untilExpression
    :	equivalenceExpression 'until' equivalenceExpression
    ;

weakUntilExpression
    :	equivalenceExpression 'weak-until' equivalenceExpression
    ;

followedByExpression
    :	equivalenceExpression 'followed-by' equivalenceExpression
    ;

precededByExpression
    :	equivalenceExpression 'preceded-by' equivalenceExpression
    ;

equivalenceExpression
    :	implicationExpression ('equivalent' implicationExpression)?
    ;

implicationExpression
    :	orExpression ('implies' orExpression)?
    ;

orExpression
    :   andExpression ('or' andExpression)?
    ;

andExpression
    :	ltlExpressionOperand ('and' ltlExpressionOperand)?
    ;

ltlExpressionOperand
    :	'(' ltlExpression ')'
	|	'(' equivalenceExpression ')'
    |   relationalExpression
    |   notExpression
    ;

notExpression
    :   'not' '(' equivalenceExpression ')'
    ;
	
relationalExpression
    :   arithmeticExpression RelationalOperator arithmeticExpression
    ;

arithmeticExpression
    :   arithmeticAddition
    |   arithmeticMultiplication
    |   arithmeticOperand
    ;

arithmeticAddition
    :   arithmeticOperand '+' arithmeticOperand
    ;

arithmeticMultiplication
    :   arithmeticOperand '*' arithmeticOperand
    ;

arithmeticOperand
    :   objectMultiplicity
    |   NumericLiteral
    |   '(' arithmeticExpression ')'
    ;

objectMultiplicity
    :   Identifier '.' Identifier
    ;

Identifier
    :   Letter Alphanumeric*
    ;

RelationalOperator
    :   '>=' | '<=' | '>' | '<' | '=' | '!='
    ;

NumericLiteral
    :   '0' | '1'..'9' '0'..'9'*
    ;

fragment Alphanumeric
    :   Letter | ('0'..'9') | ('_')
    ;	

fragment Letter
    :   '\u0024' 
    |   '\u0041'..'\u005a'
    |   '\u005f' 
    |   '\u0061'..'\u007a'
    |   '\u00c0'..'\u00d6'
    |   '\u00d8'..'\u00f6' 
    |   '\u00f8'..'\u00ff' 
    |   '\u0100'..'\u1fff' 
    |   '\u3040'..'\u318f' 
    |   '\u3300'..'\u337f' 
    |   '\u3400'..'\u3d2d' 
    |   '\u4e00'..'\u9fff' 
    |   '\uf900'..'\ufaff'
    ;
		
Ws
    : [ \r\t\u000C\n]+ -> channel(HIDDEN)
    ;
	
Comment
    : '/*' .*? '*/' -> channel(HIDDEN)
    ;

LineComment
    : '//' ~[\r\n]* -> channel(HIDDEN)
    ;