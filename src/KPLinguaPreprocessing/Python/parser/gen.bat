set java="C:\Program Files\Java\jdk-11.0.15\bin\java"


set c=%java% -cp ..\..\..\kpl\antlr-4.10.1-complete.jar org.antlr.v4.Tool

%c% -Dlanguage=Python3 KplIterator.g4 -visitor -no-listener -o grammar

