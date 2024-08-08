using KPLinguaPreprocessing.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace KPLinguaPreprocessing
{
    public class Parser
    {
        private Regex iteratorRegex;
        private Regex iteratorContinuationRegex;
        private Regex commentRegex;
        private Regex variableRegex;
        private Regex globalVariableRegex;
        private Regex includeRegex;
        private Dictionary<string, Variable> variables;

        public Parser()
        {
            iteratorRegex = new Regex(@"(?<rule>.*(\.|\{|\})\s*)(?<iterator>:.*)(?<n>\n)?");
            iteratorContinuationRegex = new Regex(@"(?<rule>.*(\.|\{|\})\s*)(?<iterator>:\s*)(?<n>\n)?$");
            commentRegex = new Regex(@"^\s*//");
            variableRegex = new Regex(@"\$(([a-z]+|\d+)(\+|-|\*)?([a-z]+|\d+)?)+\$");
            globalVariableRegex = new Regex(@"^\s*#define\s(?<content>.+)");
            includeRegex = new Regex(@"^\s*#include\s+""(?<fileName>.+)""");
            variables = new Dictionary<string, Variable>();
        }

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

        public (List<Base> expressions, string rulesWithParameters) BuildRuleWithVariables10(string rules, KplIteratorBuilder builder)
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

        public string BuildIterator(string rules, string iteratorText)
        {
            iteratorText = Regex.Replace(iteratorText, @"^\s*:\s*", "");

            var builder = new KplIteratorBuilder(variables);
            (List<Iterator> iterators, Base restrictions, Dictionary<string, Variable> vars) = builder.BuildIterators(iteratorText);
            (List<Base> expressions, string rulesWithParameters) = BuildRuleWithVariables10(rules, builder);

            var executeIterator = new ExecuteIterator(iterators, restrictions);
            List<string> content = new List<string>();

            executeIterator.InitAll();
            bool isValid = true;

            while (isValid)
            {
                if (executeIterator.IsValid())
                {
                    string newRules = GetRules(rulesWithParameters, expressions);
                    content.Add(newRules + Environment.NewLine);
                }
                isValid = executeIterator.HasNext();
                if (isValid)
                {
                    executeIterator.Next();
                }
            }

            return string.Join("", content);
        }

        public void Execute(string sourceFileName, string destinationFileName)
        {
            var lines = ReadKpl(sourceFileName);
            var newLines = Execute(lines);
            WriteKpl(newLines, destinationFileName);
        }

        public void ExecuteLines(List<string> lines, string destinationFileName)
        {
            Execute(lines);
        }

        private List<string> Execute(List<string> lines)
        {
            List<string> newLines = new List<string>();
            int indexLines = 0;
            int length = lines.Count;

            while (indexLines < length)
            {
                string line = lines[indexLines];
                if (!commentRegex.IsMatch(line))
                {
                    var iterator = iteratorRegex.Match(line);
                    if (iterator.Success)
                    {
                        var groups = iterator.Groups;
                        string rules = groups["rule"].Value;
                        string iteratorText = groups["iterator"].Value;
                        if (groups["n"].Success)
                        {
                            rules += groups["n"].Value;
                        }
                        string content = BuildIterator(rules, iteratorText);
                        newLines.Add(content);
                    }
                    else
                    {
                        var globalVariable = globalVariableRegex.Match(line);
                        if (globalVariable.Success)
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
                        else
                        {
                            var include = includeRegex.Match(line);
                            if (include.Success)
                            {
                                var groups = include.Groups;
                                string fileName = groups["fileName"].Value;
                                var includeLines = ReadKpl(fileName);
                                var includeNewLines = Execute(includeLines);
                                newLines.AddRange(includeNewLines);
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
    }
}
