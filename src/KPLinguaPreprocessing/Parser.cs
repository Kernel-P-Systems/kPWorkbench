﻿using KPLinguaPreprocessing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KPLinguaPreprocessing
{
    public class Parser
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
        private Regex iteratorPatternInMultisetRegex = new Regex(@"(?<rule>.*?(\$|\{|\})?\s*)(?<iterator>:\S+)(?<n>\n)?");
        private string multisetIteratorPattern = @"@(.+?)@";
        private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

        public List<string> ReadKpl(string filename)
        {
            return File.ReadAllLines(filename).ToList();
        }

        public void WriteKpl(List<string> lines, string filename)
        {
            File.WriteAllLines(filename, lines);
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

        public (IParsingComponent rule, string rulesWithParameters) BuildRulesComponents(string rules, KplIteratorBuilder builder)
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

        public string BuildIterator(string rules, string iteratorText, string variableSeparator)
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

        public void ExecuteLines(List<string> lines, string destinationFileName)
        {
            //Execute(lines);
        }

        private List<string> Execute(List<string> lines, string filePath)
        {
            List<string> newLines = new List<string>();
            int indexLines = 0;
            int length = lines.Count;

            while (indexLines < length)
            {
                string line = lines[indexLines];
                if (!commentRegex.IsMatch(line))
                {
                    var multisetIterator = multisetIteratorRegexPattern.Match(line);
                    var iterator = iteratorRegex.Match(line);
                    if (multisetIterator.Success)
                    {
                        string newLine = TryToBuildMultisetIterators(line);
                        var newLineIterator = iteratorRegex.Match(newLine);
                        if (newLineIterator.Success)
                        {
                            newLine = TryToBuildIterator(newLineIterator, Environment.NewLine);
                        }
                        newLines.Add(newLine);
                    }
                    else if (iterator.Success)
                    {
                        string newLine = TryToBuildIterator(iterator, Environment.NewLine);
                        newLines.Add(newLine);
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

        private string TryToBuildMultisetIterators(string multisetIterator)
        {
            string pattern = "@(.*?)@";
            MatchCollection matches = Regex.Matches(multisetIterator, pattern);
            foreach (Match match in matches)
            {
                string oldValue = match.Groups[1].Value;
                Match iterator = iteratorPatternInMultisetRegex.Match(oldValue);
                if (iterator.Success)
                {
                    string newLine = TryToBuildIterator(iterator, ",");
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
