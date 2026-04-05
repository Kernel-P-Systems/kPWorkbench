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
        if (!File.Exists(xmlFilePath))
        {
            Console.WriteLine($"Error: XML file not found at {xmlFilePath}");
            return;
        }

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

        bool isPlingua = root.Attribute("type")?.Value == "plingua";
        bool requiresLimitMacro = false;

        // 1. Gather Instances and map them to their types
        var instances = root.Element("neurons")?.Elements("neuron");
        var instanceTypeMap = new Dictionary<string, string>();

        if (instances != null)
        {
            foreach (var inst in instances)
            {
                string id = inst.Attribute("id").Value;
                string type = inst.Attribute("type")?.Value ?? $"t_{SanitizeId(id)}";
                instanceTypeMap[id] = type;
            }
        }

        // 2. Build Connectivity Maps from Synapses
        var synapses = root.Element("synapses")?.Elements("synapse");
        var typeTargetTypes = new Dictionary<string, HashSet<string>>(); // For P-Lingua
        var instanceTargets = new Dictionary<string, List<string>>();    // For Snapse

        if (synapses != null)
        {
            foreach (var syn in synapses)
            {
                string sourceId = syn.Attribute("source").Value;
                string targetId = syn.Attribute("target").Value;

                // Map for Snapse (Instance -> Instance)
                if (!instanceTargets.ContainsKey(sourceId)) instanceTargets[sourceId] = new List<string>();
                instanceTargets[sourceId].Add(SanitizeId(targetId));

                // Map for P-Lingua (Type -> Type)
                if (instanceTypeMap.TryGetValue(sourceId, out string sourceType) &&
                    instanceTypeMap.TryGetValue(targetId, out string targetType))
                {
                    if (!typeTargetTypes.ContainsKey(sourceType)) typeTargetTypes[sourceType] = new HashSet<string>();
                    typeTargetTypes[sourceType].Add(targetType);
                }
            }
        }

        // 3. GENERATE COMPARTMENT TYPES & RULES
        var neuronTypesNode = root.Element("neuronTypes");

        if (isPlingua && neuronTypesNode != null)
        {
            // --- P-LINGUA MODE (Typed AST) ---
            foreach (var nType in neuronTypesNode.Elements("neuronType"))
            {
                string typeId = nType.Attribute("id").Value;
                var rules = nType.Element("rules")?.Elements("rule");

                if (rules == null || !rules.Any())
                {
                    sb.AppendLine($"type {typeId} {{}}");
                    sb.AppendLine();
                    continue;
                }

                sb.AppendLine($"type {typeId} {{");
                sb.AppendLine("    choice {");

                var targetTypes = typeTargetTypes.ContainsKey(typeId) ? typeTargetTypes[typeId].ToList() : new List<string>();

                foreach (var rule in rules)
                {
                    sb.AppendLine(GenerateRuleString(rule, targetTypes, true, ref requiresLimitMacro));
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }
        else if (instances != null)
        {
            // --- SNAPSE MODE (Flat AST) ---
            foreach (var inst in instances)
            {
                string id = inst.Attribute("id").Value;
                string typeId = $"t_{SanitizeId(id)}";
                var rules = inst.Element("rules")?.Elements("rule");

                if (rules == null || !rules.Any())
                {
                    sb.AppendLine($"type {typeId} {{}}");
                    sb.AppendLine();
                    continue;
                }

                sb.AppendLine($"type {typeId} {{");
                sb.AppendLine("    choice {");

                var targets = instanceTargets.ContainsKey(id) ? instanceTargets[id] : new List<string>();

                foreach (var rule in rules)
                {
                    sb.AppendLine(GenerateRuleString(rule, targets, false, ref requiresLimitMacro));
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");
                sb.AppendLine();
            }
        }

        // 4. GENERATE COMPARTMENT INSTANCES
        if (instances != null)
        {
            foreach (var inst in instances)
            {
                string id = SanitizeId(inst.Attribute("id").Value);
                string typeId = inst.Attribute("type")?.Value ?? $"t_{id}";

                // Safely parse initial spikes to prevent crashes on empty strings
                int.TryParse(inst.Attribute("initialSpikes")?.Value, out int initSpikes);
                string multiset = initSpikes > 0 ? $"{initSpikes}a" : "";
                string loop = inst.Attribute("loop")?.Value;

                string compText = $"{id} {{{multiset}}} ({typeId}) .";
                if (!string.IsNullOrEmpty(loop)) compText += $" : {loop}";

                sb.AppendLine(compText);
            }
            sb.AppendLine();
        }

        // 5. GENERATE LINKS
        if (synapses != null)
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

        // 6. MACROS
        if (requiresLimitMacro)
        {
            sb.Insert(0, "#define limit = 1024\n\n");
        }

        File.WriteAllText(destinationKpltPath, sb.ToString());
        Console.WriteLine($"Successfully generated kPWorkbench file at: {destinationKpltPath}");
    }

    // Helper: Formats a single rule and maps its targets
    private string GenerateRuleString(XElement rule, List<string> targets, bool isPLinguaTargeting, ref bool requiresLimitMacro)
    {
        string regex = rule.Attribute("regex")?.Value ?? "";
        int consumed = int.Parse(rule.Attribute("consumed")?.Value ?? "0");
        int produced = int.Parse(rule.Attribute("produced")?.Value ?? "0");
        string ruleType = rule.Attribute("type")?.Value ?? "spiking";
        string loop = rule.Attribute("loop")?.Value;

        string guard = ParseRegexToGuard(regex, consumed, out bool usesLimit);
        if (usesLimit) requiresLimitMacro = true;

        string lhs = $"{consumed}a";
        string rhs = "";

        // Map multiple targets dynamically
        if (ruleType == "spiking" && produced > 0 && targets.Any())
        {
            // P-Lingua routes to types "(t_2)", Snapse routes to components "(t_c2)"
            var targetStrings = targets.Select(t => isPLinguaTargeting ? $"{produced}a ({t})" : $"{produced}a (t_{t})");
            rhs = string.Join(", ", targetStrings);
        }

        string ruleText = $"        {guard} : {lhs} -> ";
        ruleText += string.IsNullOrEmpty(rhs) ? "." : $"{rhs} .";

        // Handle Loops
        if (usesLimit)
        {
            ruleText += string.IsNullOrEmpty(loop) ? " : 1<=j<=limit" : $" : {loop}, 1<=j<=limit";
        }
        else if (!string.IsNullOrEmpty(loop))
        {
            ruleText += $" : {loop}";
        }

        return ruleText;
    }

    // Helper: Converts SN P System Regex (e.g. a*2) into kPWorkbench Boolean Guards (=2a)
    // Helper: Converts SN P System Regex (e.g. a*2 or aaa) into kPWorkbench Boolean Guards (=2a)
    private string ParseRegexToGuard(string regex, int consumed, out bool usesLimit)
    {
        usesLimit = false;
        if (string.IsNullOrEmpty(regex)) return $"={consumed}a";

        // FIXED: Count repeating 'a's from Snapse natively (e.g., "aaa" becomes "=3a")
        if (Regex.IsMatch(regex, @"^a+$"))
        {
            return $"={regex.Length}a";
        }

        // Check for specific multipliers like a*2, a^2, a{2}
        var exactMatch = Regex.Match(regex, @"^a[\*\^\{](\d+)\}?$");
        if (exactMatch.Success) return $"={exactMatch.Groups[1].Value}a";

        // Check for pre-formatted strings like "3a"
        var intMatch = Regex.Match(regex, @"^(\d+)a$");
        if (intMatch.Success) return $"={intMatch.Groups[1].Value}a";

        // Check for unbounded positive closure (e.g., a+)
        if (regex == "a+") return ">=1a";
        if (regex.Contains("+") && !regex.Contains("(")) return $">={consumed}a";

        // Check for infinite multiplier (e.g., (a^2)+ or (a{2})+)
        var infMatch = Regex.Match(regex, @"^\(a[\^{](\d+)\}\)\+$");
        if (infMatch.Success)
        {
            usesLimit = true;
            return $"=${infMatch.Groups[1].Value}*j$a";
        }

        // Default fallback
        return $"={consumed}a";
    }

    private string SanitizeId(string id)
    {
        if (string.IsNullOrEmpty(id)) return id;

        // If the first character is a digit, prepend 'c' to make it a valid kPW identifier
        if (char.IsDigit(id[0]))
        {
            return "c" + id;
        }
        return id;
    }
}