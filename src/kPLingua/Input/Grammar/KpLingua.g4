/* The ANTLRv4 grammar for kPLingua language */

grammar KpLingua;

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

// Grammar rules for parsing the kPLingua language

kPsystem    
    :   statement* EOF
    ;

statement   
    :   typeDefinition | instantiation | link
    ;

typeDefinition
    :   'type' Identifier '{' ruleSet? '}'
    ;

ruleSet
    :	executionStrategy+
    ;
                    
executionStrategy
    :   sequenceExecutionStrategy | choiceExecutionStrategy | maxExecutionStrategy | arbitraryExecutionStrategy
    ;

sequenceExecutionStrategy
    :   rule+
    ;

choiceExecutionStrategy
    :   'choice' '{' rule* '}'
    ;

maxExecutionStrategy
    :   'max' '{' rule* '}'
    ;

arbitraryExecutionStrategy
    :   'arbitrary' '{' rule* '}'
    ;

rule
    :   (guard ':')? nonEmptyMultiset '->' ruleRightHandSide '.'
    ;

ruleRightHandSide
    :   emptyMultiset
    |   ruleMultiset (',' ruleMultiset)* 
    |   division+ 
    |   dissolution
    |   linkCreation
    |   linkDestruction
    ;

ruleMultiset
    :   nonEmptyMultiset | targetedMultiset
    ;
        
guard
    :   andGuardExpression ('|' andGuardExpression)*
    ;
	
andGuardExpression
    :	guardOperand ('&' guardOperand)*
    ;
	
guardOperand
    :	RelationalOperator? nonEmptyMultiset
    |	'(' guard ')'
    ;

emptyMultiset
    :   '{' '}'
    ;

multisetAtom
    :   Multiplicity? Identifier
    ;

nonEmptyMultiset
    :   multisetAtom (',' multisetAtom)*
    ;

typeReference
    :   '(' Identifier ')'
    ;

targetedMultiset
    :   (multisetAtom | '{' nonEmptyMultiset '}') typeReference
    ;

linkCreation
    :   '-' typeReference
    ;

linkDestruction
    :   '\\-' typeReference
    ;
        
dissolution
    :   '#'
    ;
        
division
    :   '[' nonEmptyMultiset? ']' typeReference?
    ;

instance
    :   Identifier? (emptyMultiset |  '{' nonEmptyMultiset '}') typeReference
    ;

instantiation
    :   instance (',' instance)* '.'
    ;

link
    :   linkOperand ('-' linkOperand)+ '.'
    ;

linkOperand
    :   instance | Identifier | linkWildcardOperand
    ;

linkWildcardOperand
    :   '*' typeReference
    ;
	
Identifier
    :   Letter Alphanumeric*
    ;

RelationalOperator
    :   '>=' | '<=' | '>' | '<' | '=' | '!'
    ;

Multiplicity
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
