using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing.SnpToKpsTranslators;

public class AntiSpikeSnpStrategy : ISnpVariantStrategy
{
    public void GenerateTypeDefinition(StringBuilder sb, string kplTypeId, string xmlTypeId,
        Dictionary<string, IEnumerable<XElement>> typeRulesMap,
        Dictionary<string, int> typeThresholdMap,
        List<(string Target, int Weight)> instanceTargets,
        Func<XElement, IEnumerable<(string Target, int Weight)>, bool, string> ruleGeneratorFunc)
    {
        sb.AppendLine($"type {kplTypeId} {{");

        // Annihilation block: Priority execution to cancel out 'a' and 'A'
        sb.AppendLine("    max {");
        sb.AppendLine("        >=1a & >=1A : 1a, 1A -> .");
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