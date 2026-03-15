using KPLinguaPreprocessing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KPLinguaPreprocessing
{
    public class IndexationParser
    {
        private Regex iteratorRegex = new Regex(@"(?<rule>.*(\.|\{|\})\s*)(?<iterator>:.*)(?<n>\n)?");
        //private Regex iteratorContinuationRegex = new Regex(@"(?<rule>.*(\.|\{|\}).*(\.|\[|\])\s*)(?<iterator>:\s*)(?<n>\n)?$");
        private Regex commentRegex = new Regex(@"^\s*//");
        private Regex variableRegex = new Regex(@"\$(([a-z]+|\d+)(\+|-|\*)?([a-z]+|\d+)?)+\$");
        private Regex globalVariableRegex = new Regex(@"^\s*#define\s(?<content>.+)");
        private Regex includeRegex = new Regex(@"^\s*#include\s+""(?<fileName>.+)""");

        private Regex multisetIteratorRegexPattern = new Regex(@"@([^@]+)@");
        //private Regex multisetIteratorRegexPattern = new Regex(@"[a-zA-Z0-9_]+ \{@(?<multisetIterator>[^@]+)@\}(?:,\s*[a-zA-Z0-9_]+\s*)*(?:,\s*@(?<multisetIterator>[^@]+)@\s*)* \([a-zA-Z0-9_]+\)\s*\.", RegexOptions.IgnoreCase);
        //private Regex multisetIteratorRegexPattern = new Regex(@"[a-z]+ \{@(?<multisetIterator>.*?)@\}\s+\([a-z0-9]+\)(?:\s*\.\s*)*", RegexOptions.IgnoreCase);
        //new Regex(@"[a-z]+ \{@[a-z0-9]+_\$[a-z0-9]+\$:[0-9]+(<=|>=|<|>|==|!=)[a-z0-9]+(<=|>=|<|>|==|!=)bits@\}\s+\([a-z0-9]+\) \.", RegexOptions.IgnoreCase);
        private Regex iteratorPatternInMultisetRegex = new Regex(@"(?<rule>.*?(\$|\{|\})?\s*)(?<iterator>:\s*\S+)(?<n>\n)?");
        //private Regex iteratorPatternInLogicalExpressionRegex = new Regex(@"(?<rule>\(.*?\))\s*:\s*(?<iterator>:\S+)(?<n>\n)?");
        private Regex logicalExpressionPatternRegex = new Regex(@"@[\|&]([^@]+)@");
        //private Regex kpQueryRegexPattern = new Regex(@"^(?:(?<prefix>ltl|ctl|safety):\s*(?:(?<temporal>never|eventually|always|steady-state)\s+)?)?(?<logicalCondition>@(?:and|or|\+|\-)),\s*(?<logicalRule>[^:]+)\s*:\s*(?<firstIterator>\d+<?=?>?\w+<?=?>?\d+)@?\s*;?");
        private Regex kpQueryRegexPattern = new Regex(
            @"^(?:(?<prefix>ltl|ctl|safety):\s*(?:(?<temporal>never|eventually|always|steady-state|infinitely-often)\s+)?)?" +
            // anything up to first @...@ block (non-greedy)
            @"(?<beforeBlock>.*?)" +
            // one or more @OP, rule : iterator@ blocks (rule and iterator are liberal, stop at the next @)
            @"(?<blocks>(?:@(?:(?:and|or|\+|\-)),\s*.*?\s*:\s*[^@]+@)+)" +
            // remainder of the line after the @...@ blocks
            @"(?<afterBlock>.*?)\s*;?\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        string pattern = @"^(?:(?<prefix>ltl|ctl|safety):\s*(?:(?<temporal>never|eventually|always)\s+)?)?(?<logicalCondition>@(?:and|or|\+)),\s*(?<logicalRule>[^:]+)\s*:\s*(?<firstIterator>\d+<?=?>?\w+<?=?>?\d+)@?\s*;?";

        //private string multisetIteratorPattern = @"@(.+?)@";
        private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

        public List<string> ReadKpl(string filename)
        {
            return File.ReadAllLines(filename).ToList();
        }

        public void WriteKpl(List<string> lines, string filename)
        {
            File.WriteAllLines(filename, lines);
        }

        public string GetKpl(List<string> lines)
        {
            string tempDir = Path.GetTempPath();
            string tempFilePath = Path.Combine(tempDir, Guid.NewGuid() + ".kpl");
            File.WriteAllLines(tempFilePath, lines);
            return tempFilePath;
        }

        public string GetRules(string rules, List<Base> expressions)
        {
            string values = rules;
            for (int index = 0; index < expressions.Count; index++)
            {
                string value = expressions[index].Evaluate().ToString();
                values = values.Replace($"@{index}@", value);
            }
            return values;
        }

        public (List<Base> expressions, string rulesWithParameters) BuildRuleWithVariables(string rules, KplIteratorBuilder builder)
        {
            var matches = variableRegex.Matches(rules).Cast<Match>().ToList();
            List<string> expressionsText = new List<string>();
            string rulesWithParameters = rules;

            foreach (var match in matches)
            {
                expressionsText.Add(rules.Substring(match.Index + 1, match.Length - 2));
            }

            int index = matches.Count;
            foreach (var match in matches.AsEnumerable().Reverse())
            {
                index--;
                rulesWithParameters = $"{rulesWithParameters.Substring(0, match.Index)}@{index}@{rulesWithParameters.Substring(match.Index + match.Length)}";
            }

            List<Base> expressions = new List<Base>();
            foreach (var et in expressionsText)
            {
                Base expression = builder.BuildExpression(et);
                expressions.Add(expression);
            }

            return (expressions, rulesWithParameters);
        }

        private (IParsingComponent rule, string rulesWithParameters) BuildRulesComponents(string rules, KplIteratorBuilder builder)
        {
            Rule ruleComponent = new Rule();
            string rulesText = rules;
            string rulesWithParameters = "";
            int index = 0;

            while (rulesText.Contains(Token.OpenVariable))
            {
                rulesWithParameters += rulesText.Substring(0, rulesText.IndexOf(Token.OpenVariable));
                rulesWithParameters += $"@{index}@";
                rulesText = rulesText.Substring(rulesText.IndexOf(Token.OpenVariable) + 1);
                if (rulesText.Contains(Token.CloseVariable))
                {
                    string expressionText = rulesText.Substring(0, rulesText.IndexOf(Token.CloseVariable));
                    rulesText = rulesText.Substring(rulesText.IndexOf(Token.CloseVariable) + 1);
                    ruleComponent.Add(new Expression(expressionText, builder));
                }
                index++;
            }

            if (rulesText.Length > 0)
            {
                rulesWithParameters += rulesText;
            }

            return (ruleComponent, rulesWithParameters);
        }

        public (List<Base> expressions, string rulesWithParameters) BuildRuleWithVariablesUsingBaseList(string rules, KplIteratorBuilder builder)
        {
            List<Base> expressions = new List<Base>();
            string rulesText = rules;
            string rulesWithParameters = "";
            int index = 0;

            while (rulesText.Contains(Token.OpenVariable))
            {
                rulesWithParameters += rulesText.Substring(0, rulesText.IndexOf(Token.OpenVariable));
                rulesWithParameters += $"@{index}@";
                rulesText = rulesText.Substring(rulesText.IndexOf(Token.OpenVariable) + 1);
                if (rulesText.Contains(Token.CloseVariable))
                {
                    string expressionText = rulesText.Substring(0, rulesText.IndexOf(Token.CloseVariable));
                    rulesText = rulesText.Substring(rulesText.IndexOf(Token.CloseVariable) + 1);
                    Base expression = builder.BuildExpression(expressionText);
                    expressions.Add(expression);
                }
                index++;
            }

            if (rulesText.Length > 0)
            {
                rulesWithParameters += rulesText;
            }

            return (expressions, rulesWithParameters);
        }

        private string BuildIterator(string rules, string iteratorText, string variableSeparator, bool addParentheses = false)
        {
            iteratorText = Regex.Replace(iteratorText, @"^\s*:\s*", "");

            var builder = new KplIteratorBuilder(variables);
            (List<Iterator> iterators, Base restrictions, Dictionary<string, Variable> vars) = builder.BuildIterators(iteratorText);
            (IParsingComponent rule, string rulesWithParameters) = BuildRulesComponents(rules, builder);

            var executeIterator = new ExecuteIterator(iterators, restrictions);
            List<string> content = new List<string>();

            executeIterator.InitAll();
            bool isValid = true;

            while (isValid)
            {
                if (executeIterator.IsValid())
                {
                    string newRules = rule.Process(rulesWithParameters);
                    if (addParentheses && content.Any())
                    {
                        newRules = $"{newRules})";
                    }
                    content.Add(newRules);
                }
                isValid = executeIterator.HasNext();
                if (isValid)
                {
                    executeIterator.Next();
                }
            }

            return string.Join(variableSeparator, content);
        }

        public void Execute(string sourceFileName, string destinationFileName)
        {
            var lines = ReadKpl(sourceFileName);
            var newLines = Execute(lines, Path.GetDirectoryName(sourceFileName));
            WriteKpl(newLines, destinationFileName);
        }

        public string Execute(string sourceFileName)
        {
            var lines = ReadKpl(sourceFileName);
            var newLines = Execute(lines, Path.GetDirectoryName(sourceFileName));
            return GetKpl(newLines);
        }

        private List<string> Execute(List<string> lines, string filePath)
        {
            List<string> newLines = new List<string>();
            int indexLines = 0;
            int length = lines.Count;

            while (indexLines < length)
            {
                string line = lines[indexLines];
                try
                {
                    if (!commentRegex.IsMatch(line))
                    {
                        var logicalExpressionIterator = logicalExpressionPatternRegex.Match(line);
                        var multisetIterator = multisetIteratorRegexPattern.Match(line);
                        var iterator = iteratorRegex.Match(line);
                        var kpQueryIterator = kpQueryRegexPattern.Match(line);
                        if (kpQueryIterator.Success)
                        {
                            newLines.Add(TryToBuildKpQueryIterator(line, kpQueryIterator));
                            Console.WriteLine($"Successfully applied indexation on this line: {line}");
                        }
                        else if (logicalExpressionIterator.Success)
                        {
                            newLines.AddRange(TryToBuildLogicalExpressionIterators(line));
                            Console.WriteLine($"Successfully applied indexation on this line: {line}");
                        }
                        else if (multisetIterator.Success)
                        {
                            string newLine = TryToBuildMultisetIterators(line);
                            var newLineIterator = iteratorRegex.Match(newLine);
                            if (newLineIterator.Success)
                            {
                                newLine = TryToBuildIterator(newLineIterator, Environment.NewLine);
                            }

                            newLines.Add(newLine);
                            Console.WriteLine($"Successfully applied indexation on this line: {line}");
                        }
                        else if (iterator.Success)
                        {
                            string newLine = TryToBuildIterator(iterator, Environment.NewLine);
                            newLines.Add(newLine);
                            Console.WriteLine($"Successfully applied indexation on this line: {line}");
                        }
                        else
                        {
                            var globalVariable = globalVariableRegex.Match(line);
                            if (globalVariable.Success)
                            {
                                ProcessGlobalVariable(globalVariable);
                            }
                            else
                            {
                                var include = includeRegex.Match(line);
                                if (include.Success)
                                {
                                    ProcessInclude(include, newLines, filePath);
                                }
                                else
                                {
                                    newLines.Add(line);
                                }
                            }
                        }
                    }
                    else
                    {
                        newLines.Add(line);
                    }

                    indexLines++;
                }
                catch (Exception exception)
                {
                    throw new Exception($"Cannot process the following line: {line} because: {exception}");
                }
            }

            return newLines;
        }

        private void ProcessGlobalVariable(Match globalVariable)
        {
            var groups = globalVariable.Groups;
            string content = groups["content"].Value;
            var builder = new KplIteratorBuilder(variables);
            var parameters = builder.BuildParameters(content);
            foreach (var parameter in parameters)
            {
                if (!variables.ContainsKey(parameter.Name))
                {
                    variables[parameter.Name] = parameter;
                }
            }
        }

        private void ProcessInclude(Match include, List<string> newLines, string filePath)
        {
            var groups = include.Groups;
            string fileName = groups["fileName"].Value;
            var includeLines = ReadKpl(Path.Combine(filePath, fileName));
            var includeNewLines = Execute(includeLines, filePath);
            newLines.AddRange(includeNewLines);
        }

        private List<string> TryToBuildLogicalExpressionIterators(string logicalExpression)
        {
            List<string> lines = new List<string>();
            string pattern = @"(?<logicalCondition>@[&|]),\s*(?<logicalRule>[^:]+)\s*:\s*(?<firstIterator>\d+<=\w+<=\d+@?)\s*(?:\|\s*(?<secondaryRule>[^:]+)\s*:\s*(?<rewritingRule>[\w$]+ -> [\w$]+)\s*:\s*(?<rewritingIterator>\d+<=\w+<=\d+))?(?:\s*:\s*(?<finalRewriting>[\w$]+ -> [\w$]+))?";
            Match match = Regex.Match(logicalExpression, pattern);
            string logicalExpressionPattern = @"@.*?@";
            var groups = match.Groups;
            string variableSeparator = $" {groups[1].Value.Replace("@", "")} ";
            var logicalExpressionRule = groups["logicalRule"].Value;
            var logicalExpressionIterator = groups["firstIterator"].Value.Replace("@", "");
            string rewritingRule;
            string rewritingRuleIterator;
            if (string.IsNullOrEmpty(groups["finalRewriting"].Value) ||
                string.IsNullOrEmpty(groups["finalIterator"].Value))
            {
                rewritingRule = groups["rewritingRule"].Value;
                rewritingRuleIterator = groups["rewritingIterator"].Value;
            }
            else
            {
                rewritingRule = groups["finalRewriting"].Value;
                rewritingRuleIterator = groups["finalIterator"].Value;
            }

            string newLine = BuildIterator(logicalExpressionRule, logicalExpressionIterator, variableSeparator);
            Match matchLocalExpression = Regex.Match(logicalExpression, logicalExpressionPattern);
            if (matchLocalExpression.Success)
            {
                string resultedLine = logicalExpression.Replace(matchLocalExpression.Value, newLine);
                logicalExpression = resultedLine.Replace("@", string.Empty);
            }

            if (string.IsNullOrEmpty(rewritingRuleIterator))
            {
                lines.Add(logicalExpression);
            }
            else
            {
                string[] rewritingRules = BuildIterator(rewritingRule, rewritingRuleIterator, Environment.NewLine).Split(Environment.NewLine);
                foreach (var rule in rewritingRules)
                {
                    lines.Add(NormalizeLine(logicalExpression.Replace(rewritingRule, rule).Replace(rewritingRuleIterator, "")));
                }
            }

            return lines;
        }

        private string NormalizeLine(string line)
        {
            string pattern = @"(.*)\s*:\s*(\.)$";
            return Regex.Replace(line, pattern, "$1$2");
        }

        private string TryToBuildKpQueryIterator(string line, Match match)
        {
            //string pattern = @"^(?:(?<prefix>ltl|ctl|safety):\s*)?(?<logicalCondition>@(?:and|or)),\s*(?<logicalRule>[^:]+)\s*:\s*(?<firstIterator>\d+<=\w+<=\d+)@?";
            //Match match = Regex.Match(line, pattern);
            string expressionPattern = @"@.*?@";
            var groups = match.Groups;
            string variableSeparator = $" {groups["logicalCondition"].Value.Replace("@", "")} ";
            var logicalExpressionRule = groups["logicalRule"].Value;
            var logicalExpressionIterator = groups["firstIterator"].Value.Replace("@", "");

            string resultedLine = line;

            // Find all @...@ blocks
            var matches = Regex.Matches(line, expressionPattern);

            foreach (Match exprMatch in matches)
            {
                string expr = exprMatch.Value; // e.g. @and, c1.b$i$ = 0 : 1<=i<=3@

                // Parse inner components again (operator, rule, iterator)
                var inner = Regex.Match(expr,
                    @"@(?<logicalCondition>(?:and|or|\+|\-)),\s*(?<logicalRule>[^:]+)\s*:\s*(?<firstIterator>\d+<?=?>?\w+<?=?>?\d+)@?");

                if (!inner.Success)
                    continue;

                string separator = $" {inner.Groups["logicalCondition"].Value} ";
                string rule = inner.Groups["logicalRule"].Value;
                string iterator = inner.Groups["firstIterator"].Value;

                string expanded = BuildIterator(rule, iterator, separator, true);
                expanded = RemoveLastParentheses(expanded);

                int unmatchedClosings = expanded.Count(c => c == ')');
                string parenthesesLine = new string('(', unmatchedClosings) + expanded;

                // Replace this block in the current line
                resultedLine = resultedLine.Replace(expr, parenthesesLine);
            }

            return resultedLine;

            //Match matchLocalExpression = Regex.Match(logicalExpression, logicalExpressionPattern);
            //if (matchLocalExpression.Success)
            //{
            //    string resultedLine = logicalExpression.Replace(matchLocalExpression.Value, newLine);
            //    logicalExpression = resultedLine.Replace("@", string.Empty);
            //}

            //if (string.IsNullOrEmpty(rewritingRuleIterator))
            //{
            //    lines.Add(logicalExpression);
            //}
            //else
            //{
            //    string[] rewritingRules = BuildIterator(rewritingRule, rewritingRuleIterator, Environment.NewLine).Split(Environment.NewLine);
            //    foreach (var rule in rewritingRules)
            //    {
            //        lines.Add(NormalizeLine(logicalExpression.Replace(rewritingRule, rule).Replace(rewritingRuleIterator, "")));
            //    }
            //}

            //return lines;
        }

        private string RemoveLastParentheses(string newLine)
        {
            int index = newLine.LastIndexOf(')');
            if (index != -1)
            {
                newLine = newLine.Remove(index, 1);
            }
            return newLine;
        }

        private string TryToBuildMultisetIterators(string multisetIterator)
        {
            string pattern = "@(.*?)@";
            MatchCollection matches = Regex.Matches(multisetIterator, pattern);
            foreach (Match match in matches)
            {
                string oldValue = match.Groups[1].Value;
                string normalizedOldValue = new string(oldValue.Where(c => !char.IsWhiteSpace(c)).ToArray()); ;
                Match iterator = iteratorPatternInMultisetRegex.Match(normalizedOldValue);
                if (iterator.Success)
                {
                    string newLine = TryToBuildIterator(iterator, ", ");
                    string resultedLine = multisetIterator.Replace(oldValue, newLine);
                    multisetIterator = resultedLine;
                }
                else
                {
                    Console.WriteLine("The multiset does not have an iterator or a rule in it");
                }
            }

            return multisetIterator.Replace("@", string.Empty);
        }

        private string TryToBuildIterator(Match iterator, string variableSeparator)
        {
            var groups = iterator.Groups;
            string rules = groups["rule"].Value;
            string iteratorText = groups["iterator"].Value;
            if (groups["n"].Success)
            {
                rules += groups["n"].Value;
            }
            return BuildIterator(rules, iteratorText, variableSeparator);
        }
    }
}
