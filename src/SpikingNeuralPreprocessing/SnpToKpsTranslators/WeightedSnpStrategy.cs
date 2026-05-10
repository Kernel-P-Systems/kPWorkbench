using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing.SnpToKpsTranslators;

public class WeightedSnpStrategy : ISnpVariantStrategy
{
    public void GenerateTypeDefinition(StringBuilder sb, string kplTypeId, string xmlTypeId,
        Dictionary<string, IEnumerable<XElement>> typeRulesMap,
        Dictionary<string, int> typeThresholdMap,
        List<(string Target, int Weight)> instanceTargets,
        Func<XElement, IEnumerable<(string Target, int Weight)>, bool, string> ruleGeneratorFunc)
    {
        int threshold = typeThresholdMap.ContainsKey(xmlTypeId) ? typeThresholdMap[xmlTypeId] : 1;

        sb.AppendLine($"type {kplTypeId} {{");

        sb.AppendLine("    max {");
        sb.AppendLine("        >=1a & >=1n : 1a, 1n -> .");
        sb.AppendLine("    }");

        // Threshold Clearing block
        sb.AppendLine("    max {");
        sb.AppendLine($"        <{threshold}a : 1a -> .");
        sb.AppendLine("        >=1n : 1n -> .");
        sb.AppendLine("    }");

        if (typeRulesMap.TryGetValue(xmlTypeId, out var rules) && rules.Any())
        {
            sb.AppendLine("    choice {");
            foreach (var rule in rules)
            {
                sb.AppendLine(ruleGeneratorFunc(rule, instanceTargets, false));
            }
            sb.AppendLine("    }");
        }
        sb.AppendLine("}");
        sb.AppendLine();
    }
}