using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing;

internal class PLinguaToRawXmlVisitor : PLinguaSNPBaseVisitor<object>
{
    private HashSet<string> GlobalVariables = new HashSet<string>();
    private Dictionary<string, List<XElement>> TypeRules = new Dictionary<string, List<XElement>>();
    private List<XElement> Compartments = new List<XElement>();
    private List<XElement> Links = new List<XElement>();
    private Dictionary<string, string> InitialSpikes = new Dictionary<string, string>();

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
    // 2. COMPARTMENTS (Neurons) & MULTISETS
    // ---------------------------------------------------------
    public override object VisitMuStmt(PLinguaSNPParser.MuStmtContext context)
    {
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

        foreach (var neuronCtx in context.neuronList().neuronId())
        {
            string baseType = TranslateNeuronBaseType(neuronCtx);
            string instanceId = TranslateNeuronId(neuronCtx);

            // Auto-trigger macro expansion if the instance uses a variable but has no loop
            string compartmentLoop = loop;
            if (string.IsNullOrEmpty(compartmentLoop) && instanceId.Contains("$"))
            {
                compartmentLoop = GenerateDummyIterator(instanceId);
            }

            if (!TypeRules.ContainsKey(baseType))
                TypeRules[baseType] = new List<XElement>();

            var compartment = new XElement("neuron",
                new XAttribute("id", instanceId),
                new XAttribute("type", baseType),
                new XAttribute("initialSpikes", "")
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

        // Extract just the spike count/expression for the Raw XML
        InitialSpikes[instanceId] = ExtractSpikeExpression(multiset);
        return null;
    }

    // ---------------------------------------------------------
    // 3. SYNAPSES
    // ---------------------------------------------------------
    public override object VisitMarcsStmt(PLinguaSNPParser.MarcsStmtContext context)
    {
        string sourceId = TranslateNeuronId(context.neuronId(0));
        string targetId = TranslateNeuronId(context.neuronId(1));
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

        if (string.IsNullOrEmpty(loop) && (sourceId.Contains("$") || targetId.Contains("$")))
        {
            loop = GenerateDummyIterator(sourceId + targetId);
        }

        var link = new XElement("synapse",
            new XAttribute("source", sourceId),
            new XAttribute("target", targetId)
        );

        if (!string.IsNullOrEmpty(loop))
            link.Add(new XAttribute("loop", loop));

        Links.Add(link);
        return null;
    }

    // ---------------------------------------------------------
    // 4. RULE TRANSLATION (Pure AST mapping)
    // ---------------------------------------------------------
    public override object VisitRuleStmt(PLinguaSNPParser.RuleStmtContext context)
    {
        string lhsMultiset = TranslateMultiset(context.multiset(0));
        string rhsMultiset = context.GetChild(3).GetText() == "#" ? "" : TranslateMultiset(context.multiset(1));
        string baseType = TranslateNeuronBaseType(context.neuronId());
        string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

        string consumed = ExtractSpikeExpression(lhsMultiset);
        string produced = ExtractSpikeExpression(rhsMultiset);
        string ruleType = produced == "0" ? "forgetting" : "spiking";

        string rawGuard = context.guard() != null ? context.guard().GetText().Trim('"') : "";
        string regex = string.IsNullOrEmpty(rawGuard) ? lhsMultiset : rawGuard;

        var rule = new XElement("rule",
            new XAttribute("regex", regex),
            new XAttribute("consumed", consumed),
            new XAttribute("produced", produced),
            new XAttribute("delay", "0"),
            new XAttribute("type", ruleType)
        );

        // Auto-trigger macro expansion if the rule uses a variable but has no loop left
        string ruleTextToCheck = $"{lhsMultiset} {rhsMultiset} {regex}";
        loop = FilterRedundantIterators(loop, ruleTextToCheck);

        if (string.IsNullOrEmpty(loop) && ruleTextToCheck.Contains("$"))
        {
            loop = GenerateDummyIterator(ruleTextToCheck);
        }

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

        string rawGuard = context.guard() != null ? context.guard().GetText().Trim('"') : "1a";
        string consumed = ExtractSpikeExpression(rawGuard);

        var rule = new XElement("rule",
            new XAttribute("regex", rawGuard),
            new XAttribute("consumed", consumed),
            new XAttribute("produced", "topology"),
            new XAttribute("target1", target1Type),
            new XAttribute("target2", target2Type),
            new XAttribute("type", "division")
        );

        string ruleTextToCheck = $"{rawGuard} {target1Type} {target2Type}";
        loop = FilterRedundantIterators(loop, ruleTextToCheck);

        if (string.IsNullOrEmpty(loop) && ruleTextToCheck.Contains("$"))
        {
            loop = GenerateDummyIterator(ruleTextToCheck);
        }

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

    private string ExtractSpikeExpression(string multiset)
    {
        if (string.IsNullOrEmpty(multiset)) return "0";
        var match = Regex.Match(multiset.Trim(), @"^(.*)a$");
        if (match.Success)
        {
            string val = match.Groups[1].Value.Trim();
            return string.IsNullOrEmpty(val) ? "1" : val;
        }
        return multiset;
    }

    private string TranslateNeuronId(PLinguaSNPParser.NeuronIdContext context)
    {
        string id;
        if (context.ChildCount == 1)
        {
            id = context.GetText();
        }
        else
        {
            var exprs = context.exprList().expr().Select(e => TranslateExpr(e));
            if (context.GetChild(0).GetText() == "{") id = $"c{string.Join("_", exprs)}";
            else id = $"{context.ID().GetText()}_{string.Join("_", exprs)}";
        }

        // Force the 'c' prefix for safety if it starts with a number (e.g., '0' -> 'c0')
        if (!string.IsNullOrEmpty(id) && char.IsDigit(id[0])) return "c" + id;

        return id;
    }

    private string TranslateNeuronBaseType(PLinguaSNPParser.NeuronIdContext context)
    {
        return $"t_{TranslateNeuronId(context)}";
    }

    private string VisitIndexLoop(PLinguaSNPParser.IndexLoopContext context)
    {
        var iterators = context.iterator().Select(i => i.GetText());
        return string.Join(", ", iterators);
    }

    private string TranslateExpr(PLinguaSNPParser.ExprContext context)
    {
        if (context is PLinguaSNPParser.IntExprContext) return context.GetText();
        string cleanMath = context.GetText().Replace("(", "").Replace(")", "");
        return $"${cleanMath}$";
    }

    // ---------------------------------------------------------
    // 5. XML GENERATION (Raw AST Format)
    // ---------------------------------------------------------
    public XDocument GenerateXmlCode()
    {
        var snpSystem = new XElement("snpSystem", new XAttribute("type", "plingua"));

        // 1. Constants
        if (GlobalVariables.Count > 0)
        {
            var constantsNode = new XElement("constants");
            foreach (var v in GlobalVariables)
            {
                constantsNode.Add(new XElement("constant", new XAttribute("name", v)));
            }
            snpSystem.Add(constantsNode);
        }

        // 2. Alphabet
        snpSystem.Add(new XElement("alphabet", new XElement("spike", new XAttribute("symbol", "a"))));

        // 3. Neuron Types (Rules)
        var typesNode = new XElement("neuronTypes");
        foreach (var type in TypeRules)
        {
            var typeElement = new XElement("neuronType", new XAttribute("id", type.Key));
            var rulesElement = new XElement("rules");

            int ruleId = 1;
            foreach (var rule in type.Value)
            {
                rule.Add(new XAttribute("id", $"r{ruleId++}"));
                rulesElement.Add(rule);
            }

            typeElement.Add(rulesElement);
            typesNode.Add(typeElement);
        }
        snpSystem.Add(typesNode);

        // 4. Compartment Instances (Neurons)
        foreach (var comp in Compartments)
        {
            string id = comp.Attribute("id").Value;
            if (InitialSpikes.ContainsKey(id))
            {
                comp.SetAttributeValue("initialSpikes", InitialSpikes[id]);
                InitialSpikes.Remove(id);
            }
        }

        // Add any inferred neurons from raw initial spike mappings
        foreach (var spike in InitialSpikes)
        {
            string inferredType = "t_" + spike.Key.Split('_')[0];
            Compartments.Add(new XElement("neuron",
                new XAttribute("id", spike.Key),
                new XAttribute("type", inferredType),
                new XAttribute("initialSpikes", spike.Value)
            ));
        }

        snpSystem.Add(new XElement("neurons", Compartments));
        snpSystem.Add(new XElement("synapses", Links));

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), snpSystem);
    }

    // ---------------------------------------------------------
    // AST HELPERS
    // ---------------------------------------------------------
    private string GenerateDummyIterator(string content)
    {
        if (!content.Contains("$")) return "";
        var match = Regex.Match(content, @"\$[^$]*?([a-zA-Z_][a-zA-Z0-9_]*)[^$]*?\$");
        if (match.Success)
        {
            string varName = match.Groups[1].Value;
            return $"{varName}<={varName}<={varName}";
        }
        return "1<=macro_trigger<=1";
    }

    private string FilterRedundantIterators(string loop, string ruleTextToCheck)
    {
        if (string.IsNullOrEmpty(loop)) return loop;
        var validIterators = new List<string>();
        foreach (string iterator in loop.Split(','))
        {
            var match = Regex.Match(iterator, @"[<]=?\s*([a-zA-Z_][a-zA-Z0-9_]*)\s*[<]=?");
            if (match.Success)
            {
                string varName = match.Groups[1].Value;
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