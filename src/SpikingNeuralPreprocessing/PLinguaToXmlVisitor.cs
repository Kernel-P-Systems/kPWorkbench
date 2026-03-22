using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing;

internal class PLinguaToXmlVisitor : PLinguaSNPBaseVisitor<object>
{
    private HashSet<string> GlobalVariables = new HashSet<string>();
    private Dictionary<string, List<XElement>> TypeRules = new Dictionary<string, List<XElement>>();
    private List<XElement> Compartments = new List<XElement>();
    private List<XElement> Links = new List<XElement>();
    private Dictionary<string, string> InitialSpikes = new Dictionary<string, string>();

    // NEW: Maps specific instance IDs (e.g., "Cx_$i$") to their base types (e.g., "t_Cx")
    private Dictionary<string, string> InstanceToType = new Dictionary<string, string>();

    // ---------------------------------------------------------
    // 1. GLOBAL STRUCTURE & CONSTANTS
    // ---------------------------------------------------------
    public override object VisitFunctionDecl(PLinguaSNPParser.FunctionDeclContext context)
    {
        if (context.paramList() != null)
        {
            foreach (var param in context.paramList().ID())
            {
                GlobalVariables.Add(param.GetText());
            }
        }
        return base.VisitFunctionDecl(context);
    }

    // ---------------------------------------------------------
    // 2. COMPARTMENTS & MULTISETS
    // ---------------------------------------------------------
    public override object VisitMuStmt(PLinguaSNPParser.MuStmtContext context)
    {
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

        foreach (var neuronCtx in context.neuronList().neuronId())
        {
            string baseType = TranslateNeuronBaseType(neuronCtx);
            string instanceId = TranslateNeuronId(neuronCtx);

            // NEW: Auto-trigger macro expansion if the instance uses a variable but has no loop
            string compartmentLoop = loop;
            if (string.IsNullOrEmpty(compartmentLoop) && instanceId.Contains("$"))
            {
                compartmentLoop = GenerateDummyIterator(instanceId);
            }

            // Store the instance-to-type mapping for target routing later
            InstanceToType[instanceId] = baseType;

            if (!TypeRules.ContainsKey(baseType))
                TypeRules[baseType] = new List<XElement>();

            var compartment = new XElement("compartment",
                new XAttribute("id", instanceId),
                new XAttribute("type", baseType),
                new XAttribute("initialMultiset", "")
            );

            if (!string.IsNullOrEmpty(compartmentLoop))
                compartment.Add(new XAttribute("loop", compartmentLoop));

            Compartments.Add(compartment);
        }
        return null;
    }

    public override object VisitMsStmt(PLinguaSNPParser.MsStmtContext context)
    {
        string instanceId = TranslateNeuronId(context.neuronId());
        string multiset = TranslateMultiset(context.multiset());
        InitialSpikes[instanceId] = multiset;
        return null;
    }

    // ---------------------------------------------------------
    // 3. SYNAPSES (Links)
    // ---------------------------------------------------------
    public override object VisitMarcsStmt(PLinguaSNPParser.MarcsStmtContext context)
    {
        string sourceId = TranslateNeuronId(context.neuronId(0));
        string targetId = TranslateNeuronId(context.neuronId(1));
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

        // NEW: Auto-trigger macro expansion if the link uses a variable but has no loop
        if (string.IsNullOrEmpty(loop) && (sourceId.Contains("$") || targetId.Contains("$")))
        {
            loop = GenerateDummyIterator(sourceId + targetId);
        }

        var link = new XElement("link",
            new XAttribute("source", sourceId),
            new XAttribute("target", targetId)
        );

        if (!string.IsNullOrEmpty(loop))
            link.Add(new XAttribute("loop", loop));

        Links.Add(link);
        return null;
    }

    // ---------------------------------------------------------
    // 4. RULE TRANSLATION
    // ---------------------------------------------------------
    public override object VisitRuleStmt(PLinguaSNPParser.RuleStmtContext context)
    {
        string lhs = TranslateMultiset(context.multiset(0));
        string rhs = context.GetChild(3).GetText() == "#" ? "" : TranslateMultiset(context.multiset(1));
        string baseType = TranslateNeuronBaseType(context.neuronId());
        string sourceId = TranslateNeuronId(context.neuronId());
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

        var rule = new XElement("rule",
            new XAttribute("lhs", lhs),
            new XAttribute("rhs", rhs),
            new XAttribute("sourceId", sourceId) // Temporarily save the source ID
        );

        if (context.guard() != null)
        {
            string rawGuard = context.guard().GetText().Trim('"');

            var regexMatch = Regex.Match(rawGuard, @"^\(a\{(\d+)\}\)\+$");
            if (regexMatch.Success)
            {
                string multiple = regexMatch.Groups[1].Value;
                rule.Add(new XAttribute("guard", $"=${multiple}*j$a"));
                loop = "1<=j<=limit";
                GlobalVariables.Add("limit");
            }
            else
            {
                string kpGuard = rawGuard.Contains("*") ? $"={rawGuard.Split('*')[1]}{rawGuard.Split('*')[0]}" : $">=1{rawGuard}";
                rule.Add(new XAttribute("guard", kpGuard));
            }
        }
        else
        {
            // FIXED: SN P Systems default to EXACT match if no guard is provided
            rule.Add(new XAttribute("guard", $"={lhs}"));
        }

        string kpGuardAttr = rule.Attribute("guard")?.Value ?? "";
        string ruleTextToCheck = $"{lhs} {rhs} {kpGuardAttr}";

        // NEW: Filter out redundant iterators (like 'i') that aren't used in the rule text
        loop = FilterRedundantIterators(loop, ruleTextToCheck);

        // Auto-trigger macro expansion if the rule uses a variable but has no loop left
        if (string.IsNullOrEmpty(loop) && (lhs.Contains("$") || rhs.Contains("$") || kpGuardAttr.Contains("$")))
        {
            loop = GenerateDummyIterator(ruleTextToCheck);
        }

        // Safely add the loop 
        if (!string.IsNullOrEmpty(loop)) rule.Add(new XAttribute("loop", loop));

        if (!TypeRules.ContainsKey(baseType))
        {
            TypeRules[baseType] = new List<XElement>();
        }
        TypeRules[baseType].Add(rule);

        return null;
    }

    public override object VisitTopoRuleStmt(PLinguaSNPParser.TopoRuleStmtContext context)
    {
        string sourceNeuron = TranslateNeuronBaseType(context.neuronId(0));
        string target1Type = TranslateNeuronBaseType(context.neuronId(1));
        string target2Type = TranslateNeuronBaseType(context.neuronId(2));
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";
        string spike = context.guard() != null ? context.guard().GetText().Trim('"') : "a";
        string normalizedSpike = (spike == "a") ? "1a" : spike; // Ensure valid kPW syntax

        string rhs = $"[{normalizedSpike}]({target1Type}) [{normalizedSpike}]({target2Type})";
        string ruleTextToCheck = $"{spike} {rhs}";

        // Filter out redundant iterators that aren't used in the rule text
        loop = FilterRedundantIterators(loop, ruleTextToCheck);

        // Auto-trigger macro expansion if needed
        if (string.IsNullOrEmpty(loop) && (spike.Contains("$") || rhs.Contains("$")))
        {
            loop = GenerateDummyIterator(ruleTextToCheck);
        }

        var rule = new XElement("rule",
            new XAttribute("type", "topology"),
            new XAttribute("lhs", spike),
            new XAttribute("rhs", rhs)
        );

        if (!string.IsNullOrEmpty(loop)) rule.Add(new XAttribute("loop", loop));

        if (!TypeRules.ContainsKey(sourceNeuron)) TypeRules[sourceNeuron] = new List<XElement>();
        TypeRules[sourceNeuron].Add(rule);

        return null;
    }

    // ---------------------------------------------------------
    // UTILITY TRANSLATORS
    // ---------------------------------------------------------
    private string TranslateMultiset(PLinguaSNPParser.MultisetContext context)
    {
        var terms = new List<string>();
        foreach (var spike in context.spikeTerm())
        {
            string obj = spike.ID().GetText();
            string count = spike.expr() != null ? TranslateExpr(spike.expr()) : "1";
            terms.Add($"{count}{obj}");
        }
        return string.Join(", ", terms);
    }

    private string TranslateNeuronId(PLinguaSNPParser.NeuronIdContext context)
    {
        if (context.ChildCount == 1) return context.GetText();

        var exprs = context.exprList().expr().Select(e => TranslateExpr(e));


        if (context.GetChild(0).GetText() == "{")
        {
            return $"c{string.Join("_", exprs)}";
        }

        return $"{context.ID().GetText()}_{string.Join("_", exprs)}";
    }

    private string TranslateNeuronBaseType(PLinguaSNPParser.NeuronIdContext context)
    {
        if (context.ChildCount == 1) return $"t_{context.GetText()}";
        if (context.GetChild(0).GetText() == "{") return "t_dynamic";
        return $"t_{context.ID().GetText()}";
    }

    private string VisitIndexLoop(PLinguaSNPParser.IndexLoopContext context)
    {
        var iterators = context.iterator().Select(i => i.GetText());
        return string.Join(", ", iterators);
    }

    private string TranslateExpr(PLinguaSNPParser.ExprContext context)
    {
        if (context is PLinguaSNPParser.IntExprContext)
        {
            return context.GetText();
        }

        string cleanMath = context.GetText().Replace("(", "").Replace(")", "");

        return $"${cleanMath}$";
    }

    // ---------------------------------------------------------
    // 5. XML GENERATION
    // ---------------------------------------------------------
    public XDocument GenerateXmlCode()
    {
        var kPSystem = new XElement("kPSystem");

        // 1. Constants
        if (GlobalVariables.Count > 0)
        {
            var constantsNode = new XElement("constants");
            foreach (var v in GlobalVariables)
            {
                constantsNode.Add(new XElement("constant",
                    new XAttribute("name", v),
                    new XAttribute("value", "10") // Default mock value for compilation
                ));
            }
            kPSystem.Add(constantsNode);
        }

        // 2. Compartment Types & Rules
        var typesNode = new XElement("compartmentTypes");
        foreach (var type in TypeRules)
        {
            var typeElement = new XElement("type", new XAttribute("id", type.Key));
            var strategyElement = new XElement("strategy", new XAttribute("type", "choice"));

            int ruleId = 1;
            foreach (var rule in type.Value)
            {
                // NEW: Dynamically map synaptic targets to the RHS of the rule
                if (rule.Attribute("sourceId") != null)
                {
                    string sourceId = rule.Attribute("sourceId").Value;
                    string rhs = rule.Attribute("rhs").Value;

                    if (!string.IsNullOrWhiteSpace(rhs) && rhs != "lambda" && !rhs.Contains("("))
                    {
                        var outgoingLinks = Links.Where(l => l.Attribute("source").Value == sourceId).ToList();
                        if (outgoingLinks.Any())
                        {
                            var targetStrings = new List<string>();
                            foreach (var link in outgoingLinks)
                            {
                                string targetId = link.Attribute("target").Value;

                                if (InstanceToType.TryGetValue(targetId, out string targetType))
                                {
                                    targetStrings.Add($"{rhs} ({targetType})");
                                }
                                else
                                {
                                    string fallbackType = targetId.StartsWith("id_") ? "t_dynamic" : $"t_{targetId.Split('_')[0]}";
                                    targetStrings.Add($"{rhs} ({fallbackType})");
                                }
                            }
                            rule.SetAttributeValue("rhs", string.Join(", ", targetStrings));
                        }
                    }

                    // Clean up the temporary attribute so it doesn't end up in the XML
                    rule.Attribute("sourceId").Remove();
                }

                rule.Add(new XAttribute("id", $"r{ruleId++}"));
                strategyElement.Add(rule);
            }

            typeElement.Add(strategyElement);
            typesNode.Add(typeElement);
        }
        kPSystem.Add(typesNode);

        // 3. Map Initial Spikes into Compartments
        foreach (var comp in Compartments)
        {
            string id = comp.Attribute("id").Value;
            if (InitialSpikes.ContainsKey(id))
            {
                comp.SetAttributeValue("initialMultiset", InitialSpikes[id]);
                InitialSpikes.Remove(id);
            }
        }

        foreach (var spike in InitialSpikes)
        {
            string inferredType = "t_" + spike.Key.Split('_')[0];
            Compartments.Add(new XElement("compartment",
                new XAttribute("id", spike.Key),
                new XAttribute("type", inferredType),
                new XAttribute("initialMultiset", spike.Value)
            ));
        }

        kPSystem.Add(new XElement("compartments", Compartments));
        kPSystem.Add(new XElement("links", Links));

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), kPSystem);
    }

    private string GenerateDummyIterator(string content)
    {
        if (!content.Contains("$")) return "";

        // Matches the first alphabetic variable name inside the $ $ tags
        var match = Regex.Match(content, @"\$[^$]*?([a-zA-Z_][a-zA-Z0-9_]*)[^$]*?\$");
        if (match.Success)
        {
            string varName = match.Groups[1].Value;
            return $"{varName}<={varName}<={varName}";
        }

        return "1<=macro_trigger<=1"; // Fallback if no specific variable is found
    }

    private string FilterRedundantIterators(string loop, string ruleTextToCheck)
    {
        if (string.IsNullOrEmpty(loop)) return loop;

        var validIterators = new List<string>();
        foreach (string iterator in loop.Split(','))
        {
            // Extract the variable name (e.g., "i" from "1<=i<=n" or "k" from "1<=k<=n")
            var match = Regex.Match(iterator, @"[<]=?\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*[<]=?");
            if (match.Success)
            {
                string varName = match.Groups[1].Value;

                // Keep if the variable name is actually used in the rule text
                // \b ensures we match the exact word (e.g., 'i'), and not parts of other words (like 'in')
                if (Regex.IsMatch(ruleTextToCheck, $@"\b{varName}\b"))
                {
                    validIterators.Add(iterator.Trim());
                }
            }
            else
            {
                validIterators.Add(iterator.Trim());
            }
        }
        return string.Join(", ", validIterators);
    }
}