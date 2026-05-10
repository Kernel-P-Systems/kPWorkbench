using SpikingNeuralPreprocessing.SnpToKpsTranslators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing;

internal class RawXmlToKpsGenerator
{
    public void Generate(string xmlFilePath, string destinationKpltPath)
    {
        if (!File.Exists(xmlFilePath)) return;
        try
        {
            XDocument xmlDoc = XDocument.Load(xmlFilePath);
            TransformXmlToKps(destinationKpltPath, xmlDoc);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating .kplt file: {ex.Message}");
        }
    }

    public void TransformXmlToKps(string destinationKpltPath, XDocument xmlDoc)
    {
        StringBuilder sb = new StringBuilder();
        var root = xmlDoc.Root;
        if (root == null || root.Name != "snpSystem") return;

        bool requiresLimitMacro = false;
        string systemType = root.Attribute("type")?.Value.ToLower() ?? "standard";

        ISnpVariantStrategy strategy = systemType switch
        {
            "weighted" => new WeightedSnpStrategy(),
            "antispike" => new AntiSpikeSnpStrategy(),
            _ => new StandardSnpStrategy()
        };

        var instances = root.Element("neurons")?.Elements("neuron");
        if (instances == null) return;

        var typeRulesMap = new Dictionary<string, IEnumerable<XElement>>();
        var typeThresholdMap = new Dictionary<string, int>();

        // 1A. Attempt to load from global <neuronTypes> (P-Lingua Style)
        var neuronTypesNode = root.Element("neuronTypes");
        if (neuronTypesNode != null)
        {
            foreach (var nType in neuronTypesNode.Elements("neuronType"))
            {
                string tId = nType.Attribute("id").Value;
                var rules = nType.Element("rules")?.Elements("rule");
                if (rules != null) typeRulesMap[tId] = rules;

                var thresholdAttr = nType.Attribute("threshold");
                if (thresholdAttr != null && int.TryParse(thresholdAttr.Value, out int explicitThreshold))
                    typeThresholdMap[tId] = explicitThreshold;
                else if (rules != null && rules.Any())
                    typeThresholdMap[tId] = rules.Select(r => int.Parse(r.Attribute("consumed")?.Value ?? "1")).Min();
                else
                    typeThresholdMap[tId] = 1;
            }
        }

        // 1B. Fallback: Load inline <rules> directly from instances (Snapse Style)
        foreach (var inst in instances)
        {
            string id = SanitizeId(inst.Attribute("id").Value);
            string typeAttr = inst.Attribute("type")?.Value;
            string tId = string.IsNullOrEmpty(typeAttr) ? $"t_{id}" : typeAttr;

            var rules = inst.Element("rules")?.Elements("rule");
            if (rules != null && rules.Any())
            {
                if (!typeRulesMap.ContainsKey(tId))
                {
                    typeRulesMap[tId] = rules;
                    typeThresholdMap[tId] = rules.Select(r => int.Parse(r.Attribute("consumed")?.Value ?? "1")).Min();
                }
            }
            else if (!typeRulesMap.ContainsKey(tId))
            {
                typeRulesMap[tId] = new List<XElement>();
                typeThresholdMap[tId] = 1;
            }
        }

        var synapses = root.Element("synapses")?.Elements("synapse");
        var instanceTargets = new Dictionary<string, List<(string Target, int Weight)>>();
        bool hasTopologicalLinks = false;

        // 2. EVALUATE ALL SYNAPSES
        if (synapses != null && synapses.Any())
        {
            hasTopologicalLinks = true;
            foreach (var syn in synapses)
            {
                string sourceId = syn.Attribute("source").Value;
                string targetId = syn.Attribute("target").Value;
                int weight = 1;
                var weightAttr = syn.Attribute("weight");
                if (weightAttr != null) int.TryParse(weightAttr.Value, out weight);
                string loop = syn.Attribute("loop")?.Value;

                EvaluateLoopAndAddTargets(sourceId, targetId, loop, weight, instanceTargets);
            }
        }

        // 3. GENERATE RULE TYPES
        var allTypes = new HashSet<string>(typeRulesMap.Keys);
        foreach (string kplTypeId in allTypes)
        {
            if (kplTypeId.Contains("$")) continue;

            string instanceId = kplTypeId.StartsWith("t_") ? kplTypeId.Substring(2) : kplTypeId;

            // --- THE FIX ---
            // We NO LONGER clear the targets if hasTopologicalLinks is true.
            // kPWorkbench needs BOTH the explicit rule targets AND the topological links.
            var targets = instanceTargets.ContainsKey(instanceId) ? instanceTargets[instanceId] : new List<(string Target, int Weight)>();

            strategy.GenerateTypeDefinition(
                sb, kplTypeId, kplTypeId, typeRulesMap, typeThresholdMap, targets,
                (rule, ruleTargets, isPlingua) => GenerateRuleString(rule, ruleTargets, isPlingua, ref requiresLimitMacro)
            );
        }

        // 4. GENERATE EXPLICIT COMPARTMENTS
        var explicitCompartments = new Dictionary<string, string>();
        foreach (var inst in instances)
        {
            string loop = inst.Attribute("loop")?.Value;
            string idTemplate = inst.Attribute("id").Value;
            string tId = inst.Attribute("type")?.Value ?? $"t_{SanitizeId(idTemplate)}";

            int.TryParse(inst.Attribute("initialSpikes")?.Value, out int initSpikes);
            string multiset = initSpikes > 0 ? $"{initSpikes}a" : "";

            if (string.IsNullOrEmpty(loop))
            {
                explicitCompartments[SanitizeId(idTemplate)] = $"{multiset}|{tId}";
            }
            else
            {
                var combos = EvaluateLoopToCombos(loop);
                foreach (var combo in combos)
                {
                    string concreteId = idTemplate;
                    foreach (var kvp in combo)
                        concreteId = concreteId.Replace($"${kvp.Key}$", kvp.Value).Replace($"{{{kvp.Key}}}", kvp.Value);

                    concreteId = SanitizeId(concreteId);
                    if (!explicitCompartments.ContainsKey(concreteId))
                        explicitCompartments[concreteId] = $"{multiset}|{tId}";
                }
            }
        }

        foreach (var kvp in explicitCompartments)
        {
            string[] parts = kvp.Value.Split('|');
            string multiset = parts[0];
            string tId = parts[1];
            sb.AppendLine($"{kvp.Key} {{{multiset}}} ({tId}) .");
        }
        sb.AppendLine();

        // 5. GENERATE LINKS
        if (hasTopologicalLinks)
        {
            foreach (var syn in synapses)
            {
                string source = SanitizeId(syn.Attribute("source").Value);
                string target = SanitizeId(syn.Attribute("target").Value);
                string loop = syn.Attribute("loop")?.Value;

                string linkText = $"{source} - {target} .";
                if (!string.IsNullOrEmpty(loop)) linkText += $" : {loop}";

                sb.AppendLine(linkText);
            }
        }

        if (requiresLimitMacro) sb.Insert(0, "#define limit = 1024\n\n");

        File.WriteAllText(destinationKpltPath, sb.ToString());
    }

    private string GenerateRuleString(XElement rule, IEnumerable<(string Target, int Weight)> targets, bool isPLinguaTargeting, ref bool requiresLimitMacro)
    {
        string regex = rule.Attribute("regex")?.Value ?? "";
        int consumed = int.Parse(rule.Attribute("consumed")?.Value ?? "0");
        int produced = int.Parse(rule.Attribute("produced")?.Value ?? "0");
        string ruleType = rule.Attribute("type")?.Value ?? "spiking";
        string loop = rule.Attribute("loop")?.Value;

        string producedSymbol = rule.Attribute("producedSymbol")?.Value ?? "a";
        char consumedSymbol = regex.FirstOrDefault(char.IsLetter);
        if (consumedSymbol == '\0') consumedSymbol = 'a';

        string guard = ParseRegexToGuard(regex, consumed, consumedSymbol, out bool usesLimit);
        if (usesLimit) requiresLimitMacro = true;

        string lhs = $"{consumed}{consumedSymbol}";
        string rhs = "";

        if (ruleType == "spiking" && produced > 0)
        {
            if (targets.Any())
            {
                var targetStrings = targets.Select(t => {
                    int actualProduced = produced * t.Weight;
                    string symbol = actualProduced < 0 ? "n" : producedSymbol;
                    return isPLinguaTargeting ? $"{Math.Abs(actualProduced)}{symbol} ({t.Target})" : $"{Math.Abs(actualProduced)}{symbol} (t_{t.Target})";
                });
                rhs = string.Join(", ", targetStrings);
            }
            else
            {
                rhs = $"{produced}{producedSymbol} (env)";
            }
        }

        string ruleText = $"        {guard} : {lhs} -> ";
        ruleText += string.IsNullOrEmpty(rhs) ? "." : $"{rhs} .";

        if (usesLimit) ruleText += string.IsNullOrEmpty(loop) ? " : 1<=j<=limit" : $" : {loop}, 1<=j<=limit";
        else if (!string.IsNullOrEmpty(loop)) ruleText += $" : {loop}";

        return ruleText;
    }

    private string ParseRegexToGuard(string regex, int consumed, char symbol, out bool usesLimit)
    {
        usesLimit = false;
        if (string.IsNullOrEmpty(regex)) return $"={consumed}{symbol}";
        if (Regex.IsMatch(regex, $@"^{symbol}+$")) return $"={regex.Length}{symbol}";
        var exactMatch = Regex.Match(regex, $@"^{symbol}[\*\^\{{](\d+)\}}?$");
        if (exactMatch.Success) return $"={exactMatch.Groups[1].Value}{symbol}";
        var intMatch = Regex.Match(regex, $@"^(\d+){symbol}$");
        if (intMatch.Success) return $"={intMatch.Groups[1].Value}{symbol}";
        if (regex == $"{symbol}+") return $">=1{symbol}";
        if (regex.Contains("+") && !regex.Contains("(")) return $">={consumed}{symbol}";
        var infMatch = Regex.Match(regex, $@"^\({symbol}[\^{{](\d+)\}}\)\+$");
        if (infMatch.Success)
        {
            usesLimit = true;
            return $"=${infMatch.Groups[1].Value}*j${symbol}";
        }
        return $"={consumed}{symbol}";
    }

    private string SanitizeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return id;
        if (char.IsDigit(id[0])) return "c" + id;
        return id;
    }

    // --- ITERATOR EVALUATION LOGIC ---
    private void EvaluateLoopAndAddTargets(string source, string target, string loop, int weight, Dictionary<string, List<(string, int)>> instanceTargets)
    {
        if (string.IsNullOrEmpty(loop))
        {
            string s = SanitizeId(source.Replace("$", "").Replace("{", "").Replace("}", ""));
            string t = SanitizeId(target.Replace("$", "").Replace("{", "").Replace("}", ""));
            if (!instanceTargets.ContainsKey(s)) instanceTargets[s] = new List<(string, int)>();
            instanceTargets[s].Add((t, weight));
            return;
        }

        var combos = EvaluateLoopToCombos(loop);
        foreach (var combo in combos)
        {
            string concreteSource = source;
            string concreteTarget = target;
            foreach (var kvp in combo)
            {
                concreteSource = concreteSource.Replace($"${kvp.Key}$", kvp.Value).Replace($"{{{kvp.Key}}}", kvp.Value);
                concreteTarget = concreteTarget.Replace($"${kvp.Key}$", kvp.Value).Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            concreteSource = SanitizeId(concreteSource);
            concreteTarget = SanitizeId(concreteTarget);

            if (!instanceTargets.ContainsKey(concreteSource)) instanceTargets[concreteSource] = new List<(string, int)>();
            instanceTargets[concreteSource].Add((concreteTarget, weight));
        }
    }

    private List<Dictionary<string, string>> EvaluateLoopToCombos(string loop)
    {
        var iterators = new List<(string Var, int Min, int Max)>();
        var parts = loop.Split(',');
        foreach (var p in parts)
        {
            var m = Regex.Match(p, @"(-?\d+)\s*<=\s*([a-zA-Z0-9_]+)\s*<=\s*(-?\d+)");
            if (m.Success) iterators.Add((m.Groups[2].Value, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[3].Value)));
        }

        var combinations = new List<Dictionary<string, string>>();
        GenerateCombinations(iterators, 0, new Dictionary<string, string>(), combinations);
        return combinations;
    }

    private void GenerateCombinations(List<(string Var, int Min, int Max)> iterators, int index, Dictionary<string, string> current, List<Dictionary<string, string>> results)
    {
        if (index >= iterators.Count)
        {
            results.Add(new Dictionary<string, string>(current));
            return;
        }
        var iter = iterators[index];
        for (int i = iter.Min; i <= iter.Max; i++)
        {
            current[iter.Var] = i.ToString();
            GenerateCombinations(iterators, index + 1, current, results);
        }
    }
}