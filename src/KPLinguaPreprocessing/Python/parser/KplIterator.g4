grammar KplIterator;

parameters: parameter (',' parameter)* ;

parameter: Identifier ('=' Number)? ;

iterators : iterator (',' iterator)* (',' restrictions=relationalExpression)? ;

iterator : left=arithmeticExpression leftInq=('<='|'<') Identifier rightInq=('<='|'<') right=arithmeticExpression (',' increment=Number)?;

arithmeticExpression
        : left=arithmeticExpression op=Mul right=arithmeticExpression # arOpExp
        | left=arithmeticExpression op=(Add|Sub) right=arithmeticExpression # arOpExp
        | Number # arOpExp
        | identifierOp=identifier # arOpExp
        ;

identifier : Identifier ;

relationalExpression
        : left=relationalExpression op=And right=relationalExpression # loOpExp
        | left=relationalExpression op=Or right=relationalExpression # loOpExp
        | lo=logicalExpression # loOpExp
        ;

logicalExpression: left=arithmeticExpression lo=logicalOperator right=arithmeticExpression ;

logicalOperator
            : Gt # logicalOperatorName
            | Gte # logicalOperatorName
            | Lt # logicalOperatorName
            | Lte # logicalOperatorName
            | Eq # logicalOperatorName
            | Neq # logicalOperatorName
            ;

Add: '+';
Sub: '-';
Mul: '*';

Gt: '>';
Gte: '>=';
Lt: '<';
Lte: '<=';
Eq: '==';
Neq: '!=';

Or: '|';
And: '&';

Number : [0-9]+ ;

Identifier : [A-Za-z][A-Za-z0-9]* ;

WS: [ \n]+ -> skip;