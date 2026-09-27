using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing.SnpToKpsTranslators;

public class DelayedSnpStrategy : ISnpVariantStrategy
{
    public void GenerateTypeDefinition(StringBuilder sb, string kplTypeId, string xmlTypeId,
        Dictionary<string, IEnumerable<XElement>> typeRulesMap,
        Dictionary<string, int> typeThresholdMap,
        List<(string Target, int Weight)> instanceTargets,
        Func<XElement, IEnumerable<(string Target, int Weight)>, bool, string> ruleGeneratorFunc)
    {
        if (!typeRulesMap.TryGetValue(xmlTypeId, out var rules) || !rules.Any())
        {
            sb.AppendLine($"type {kplTypeId} {{ }}");
            return;
        }

        var delayRules = rules.Where(r => int.Parse(r.Attribute("delay")?.Value ?? "0") > 0).ToList();
        var standardRules = rules.Where(r => int.Parse(r.Attribute("delay")?.Value ?? "0") == 0).ToList();
        bool typeHasDelays = delayRules.Any();

        sb.AppendLine($"type {kplTypeId} {{");

        sb.AppendLine("    choice {");
        int ruleIdx = 1;
        List<string> timerObjects = new List<string>();

        foreach (var rule in standardRules)
        {
            string generatedRule = ruleGeneratorFunc(rule, instanceTargets, false).Trim();
            if (typeHasDelays)
            {
                InjectGuardCondition(sb, generatedRule, "=1o");
            }
            else
            {
                sb.AppendLine($"        {generatedRule}");
            }
        }

        if (typeHasDelays)
        {
            foreach (var rule in delayRules)
            {
                int d = int.Parse(rule.Attribute("delay").Value);
                string timerObj = $"a_{ruleIdx}";
                timerObjects.Add(timerObj);

                string generatedRule = ruleGeneratorFunc(rule, instanceTargets, false).Trim();
                ExtractRuleComponents(generatedRule, out string originalGuard, out string lhs, out string rhs, out string loops);

                string openGuard = string.IsNullOrEmpty(originalGuard) ? "=1o" : $"{originalGuard} & =1o";

                string triggerLhs = $"{lhs}, 1o, 1f";
                string triggerRhs = $"1c, 1v_{ruleIdx}, {d}{timerObj}";
                sb.AppendLine($"        {openGuard} : {triggerLhs} -> {triggerRhs} . {loops}".TrimEnd());

                string fireRhs = string.IsNullOrWhiteSpace(rhs) ? "1f, 1o" : $"1f, 1o, {rhs}";
                sb.AppendLine($"        =1{timerObj} : 1c, 1v_{ruleIdx} -> {fireRhs} .");

                ruleIdx++;
            }
        }
        sb.AppendLine("    }");

        if (typeHasDelays)
        {
            string anyTimerCondition = string.Join(" | ", timerObjects.Select(t => $">=1{t}"));
            string exactTimerCondition = string.Join(" | ", timerObjects.Select(t => $"=1{t}"));

            sb.AppendLine("    max {");
            sb.AppendLine($"        {anyTimerCondition} : 1a -> .");
            sb.AppendLine($"        {exactTimerCondition} : 1b -> 1a .");
            sb.AppendLine("    }");

            sb.AppendLine("    choice {");
            foreach (var t in timerObjects)
            {
                sb.AppendLine($"        >=1{t} : 1{t} -> .");
            }
            sb.AppendLine("    }");

            sb.AppendLine("    max {");
            sb.AppendLine("        =1f : 1a -> 1b .");
            sb.AppendLine("    }");
        }

        sb.AppendLine("}");
        sb.AppendLine();
    }

    private void InjectGuardCondition(StringBuilder sb, string ruleText, string extraGuard)
    {
        string[] parts = ruleText.Split(new[] { ':' }, 2);
        if (parts.Length == 2)
        {
            string guard = parts[0].Trim();
            string newGuard = string.IsNullOrEmpty(guard) ? extraGuard : $"{guard} & {extraGuard}";
            sb.AppendLine($"        {newGuard} : {parts[1].Trim()}");
        }
    }

    private void ExtractRuleComponents(string ruleText, out string guard, out string lhs, out string rhs, out string loops)
    {
        string[] mainParts = ruleText.Split(new[] { "->" }, StringSplitOptions.None);

        string leftSide = mainParts[0];
        string[] guardAndLhs = leftSide.Split(new[] { ':' }, 2);
        guard = guardAndLhs[0].Trim();
        lhs = guardAndLhs.Length > 1 ? guardAndLhs[1].Trim() : guardAndLhs[0].Trim();

        string rightSide = mainParts[1];
        string[] rhsAndLoops = rightSide.Split(new[] { '.' }, 2);
        rhs = rhsAndLoops[0].Trim();
        loops = rhsAndLoops.Length > 1 && rhsAndLoops[1].Contains(":") ? rhsAndLoops[1].Trim() : "";
    }
}