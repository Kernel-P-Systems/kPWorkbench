import re

from tools.kPLExtension.Iterator import ExecuteIterator
from tools.kPLExtension.Models import Variable
from tools.kPLExtension.Tokens import Tokens
from tools.kPLExtension.parser.KplIteratorBuilder import KplIteratorBuilder


class Parser:

    def __init__(self):
        self.iteratorRegex = re.compile(r'(?P<rule>.*(\.|\{|\})\s*)(?P<iterator>:.*)(?P<n>\n)?')
        self.iteratorContinuationRegex = re.compile(r'(?P<rule>.*(\.|\{|\})\s*)(?P<iterator>:\s*)(?P<n>\n)?$')
        self.commentRegex = re.compile(r'^\s*//')
        self.variableRegex = re.compile(r'\$(([a-z]+|\d+)(\+|-|\*)?([a-z]+|\d+)?)+\$')
        self.globalVariableRegex = re.compile(r'^\s*#define\s(?P<content>.+)')
        self.includeRegex = re.compile(r'^\s*#include\s+"(?P<fileName>.+)"')
        self.variables = dict()

    def readKpl(self, filename):
        with open(filename, 'r', encoding='UTF-8') as f:
            lines = f.readlines()
            f.close()
            return lines

    def writeKpl(self, lines, filename):
        with open(filename, 'w', encoding='UTF-8') as f:
            for line in lines:
                f.write(line)
            f.close()

    def getRules(self, rules, expressions):
        values = rules
        for index, expression in enumerate(expressions):
            value = expression.evaluate()
            values = values.replace("@{}@".format(index), str(value))
        return values

    def buildRuleWithVariables(self, rules, builder):
        matches = list(map(lambda m: m, self.variableRegex.finditer(rules)))
        expressionsText = []
        rulesWithParameters = rules

        for m in matches:
            expressionsText.append(rules[m.start() + 1:m.end() - 1])

        index = len(matches)
        for m in reversed(matches):
                index -= 1
                rulesWithParameters = '{}@{}@{}'.format(rulesWithParameters[:m.start()], index, rulesWithParameters[m.end():])

        expressions = []

        for et in expressionsText:
            expression = builder.buildExpression(et)
            expressions.append(expression)

        return expressions, rulesWithParameters

    def buildRuleWithVariables10(self, rules, builder):
        expressions = []
        rulesText = rules
        rulesWithParameters = ""

        index = 0

        while rulesText.find(Tokens.OPEN_VARIABLE) > -1:
            rulesWithParameters += rulesText[:rulesText.find(Tokens.OPEN_VARIABLE)]
            rulesWithParameters += '@{}@'.format(index)
            rulesText = rulesText[rulesText.find(Tokens.OPEN_VARIABLE) + 1:]
            if rulesText.find(Tokens.CLOSE_VARIABLE) > -1:
                expressionText = rulesText[:rulesText.find(Tokens.CLOSE_VARIABLE)]
                rulesText = rulesText[rulesText.find(Tokens.CLOSE_VARIABLE) + 1:]
                expression = builder.buildExpression(expressionText)
                expressions.append(expression)
            index += 1
        if len(rulesText) > 0:
            rulesWithParameters += rulesText

        return expressions, rulesWithParameters

    def buildIterator(self, rules, iteratorText):
        # print(rules)
        iteratorText = re.sub(r"^\s*:\s*", "", iteratorText)
        # print(self.variableRegex.findall(rules))
        # print(matches)

        builder = KplIteratorBuilder(self.variables)
        iterators, restrictions, variables = builder.buildIterators(iteratorText)
        expressions, rulesWithParameters = self.buildRuleWithVariables10(rules, builder)

        executeIterator = ExecuteIterator(iterators, restrictions)

        content = []

        executeIterator.initAll()

        isValid = True

        while isValid:
            if executeIterator.isValid():
                newReles = self.getRules(rulesWithParameters, expressions)
                content.append(newReles)
            isValid = executeIterator.hasNext()
            if isValid:
                executeIterator.next()

        return ''.join(content)

    def execute(self, sourceFileName, destinationFileName):
        lines = self.readKpl(sourceFileName)
        newLines = self._execute(lines)
        self.writeKpl(newLines, destinationFileName)

    def execute_lines(self, lines, destinationFileName):
        self._execute(lines)

    def _execute(self, lines):
        newLines = []

        indexLines = 0
        length = len(lines)
        while indexLines < length:
            line = lines[indexLines]
            if not self.commentRegex.match(line):
                # print(line)
                iterator = self.iteratorRegex.search(line)
                if iterator:
                    groupdict = iterator.groupdict()
                    rules = groupdict['rule']
                    iteratorText = groupdict['iterator']
                    if groupdict['n'] is not None:
                        rules += groupdict['n']
                    content = self.buildIterator(rules, iteratorText)
                    newLines.append(content)
                else:
                    globalVariable = self.globalVariableRegex.match(line)
                    if globalVariable:
                        groupdict = globalVariable.groupdict()
                        content = groupdict['content']
                        builder = KplIteratorBuilder(self.variables)
                        parameters = builder.buildParameters(content)
                        for parameter in parameters:
                            if parameter.name not in self.variables:
                                self.variables[parameter.name] = parameter
                    else:
                        include = self.includeRegex.match(line)
                        if include:
                            groupdict = include.groupdict()
                            fileNeme = groupdict['fileName']
                            includeLines = self.readKpl(fileNeme)
                            includeNewLines = self._execute(includeLines)
                            newLines += includeNewLines
                        else:
                            newLines.append(line)
            else:
                newLines.append(line)
            indexLines += 1
        return newLines


# Parser().execute(sourceFileName='Source.kplt', destinationFileName='../results/Target.kpl')
