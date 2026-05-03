using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing;

public class SpikingNeuralParser
{
    public void Parse(string sourceFilePath, string destinationFilePath)
    {
        if (!File.Exists(sourceFilePath))
        {
            Console.WriteLine($"Error: File not found at {sourceFilePath}");
            return;
        }

        XDocument generatedXml = null;
        RawXmlToKpsGenerator kpsGenerator = new RawXmlToKpsGenerator();

        try
        {
            string extension = Path.GetExtension(sourceFilePath).ToLower();
            if (extension == ".snapse")
            {
                List<Neuron> neurons = ParseSnapseFile(sourceFilePath);
                //generatedXml = GenerateSNPkPML(neurons);
                generatedXml = GenerateRawSnpXml(neurons);
                generatedXml.Save(destinationFilePath);

                Console.WriteLine($"Successfully parsed Snapse file and saved XML to: {destinationFilePath}");
            }
            else if (extension == ".pli")
            {
                generatedXml = ParsePLingua(sourceFilePath);
                generatedXml.Save(destinationFilePath);
                Console.WriteLine($"Successfully parsed P-Lingua file and saved XML to: {destinationFilePath}");
            }
            else if (extension == ".txt" || extension == ".up")
            {
                List<Neuron> neurons = ParseUPSimulatorFile(sourceFilePath);
                //generatedXml = GenerateSNPkPML(neurons);
                generatedXml = GenerateRawSnpXml(neurons);
                generatedXml.Save(destinationFilePath);
                Console.WriteLine($"Successfully parsed UPSimulator file and saved XML to: {destinationFilePath}");
            }
            else if (extension == ".xml")
            {
                kpsGenerator.Generate(sourceFilePath, destinationFilePath);
                return;
            }
            else
            {
                Console.WriteLine(
                    $"Error: Unsupported file extension '{extension}'. Expected .snapse, .pli, or .txt");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred during parsing: {ex.Message}");
        }
        kpsGenerator.TransformXmlToKps(Path.GetFullPath(@"C:\PhD\Target.kpl"), generatedXml);
    }

    private List<Neuron> ParseSnapseFile(string filePath)
    {
        string fileContent = File.ReadAllText(filePath);
        var parsedNeurons = new Dictionary<string, Neuron>();

        var declaredNeurons = new HashSet<string>();
        var headerRegex = new Regex(@"neurons\s*=\s*\[([^\]]+)\]", RegexOptions.IgnoreCase);
        var headerMatch = headerRegex.Match(fileContent);

        if (headerMatch.Success)
        {
            var names = headerMatch.Groups[1].Value.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));

            foreach (var name in names)
            {
                declaredNeurons.Add(name);
            }
        }

        var neuronBlockRegex = new Regex(@"([A-Za-z0-9_]+)\s*\{((?:[^{}]|\{[^{}]*\})*)\}");
        var matches = neuronBlockRegex.Matches(fileContent);

        foreach (Match match in matches)
        {
            string neuronId = match.Groups[1].Value;
            string blockContent = match.Groups[2].Value;

            if (neuronId.ToLower() == "neurons") continue;

            var neuron = new Neuron { Id = neuronId };

            // Parse Spikes
            var spikesMatch = Regex.Match(blockContent, @"spikes\s*=\s*(-?\d+)");
            if (spikesMatch.Success) neuron.Spikes = int.Parse(spikesMatch.Groups[1].Value);

            // Parse Delay
            var delayMatch = Regex.Match(blockContent, @"delay\s*=\s*(-?\d+)");
            if (delayMatch.Success) neuron.Delay = int.Parse(delayMatch.Groups[1].Value);

            // Parse StoredGive
            var storedGiveMatch = Regex.Match(blockContent, @"storedGive\s*=\s*(-?\d+)");
            if (storedGiveMatch.Success) neuron.StoredGive = int.Parse(storedGiveMatch.Groups[1].Value);

            // Parse StoredConsume
            var storedConsumeMatch = Regex.Match(blockContent, @"storedConsume\s*=\s*(-?\d+)");
            if (storedConsumeMatch.Success) neuron.StoredConsume = int.Parse(storedConsumeMatch.Groups[1].Value);

            // Parse OutputNeuron
            var outputMatch = Regex.Match(blockContent, @"outputNeuron\s*=\s*([a-zA-Z]+)", RegexOptions.IgnoreCase);
            if (outputMatch.Success)
            {
                string boolValue = outputMatch.Groups[1].Value.Trim().ToLower();
                neuron.IsOutput = (boolValue == "true");
            }

            // Parse Rules
            var rulesMatch = Regex.Match(blockContent, @"rules\s*=\s*\{([^}]*)\}");
            if (rulesMatch.Success)
            {
                string rulesString = rulesMatch.Groups[1].Value;
                var individualRules = Regex.Matches(rulesString, @"\[[^\]]+\]");
                foreach (Match rule in individualRules)
                {
                    neuron.Rules.Add(rule.Value);
                }
            }

            // Parse OutSynapses
            var synapsesMatch = Regex.Match(blockContent, @"outsynapses\s*=\s*\[([^\]]*)\]");
            if (synapsesMatch.Success)
            {
                string synapsesString = synapsesMatch.Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(synapsesString))
                {
                    neuron.OutSynapses = synapsesString.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }

            // Store the parsed block in a dictionary for merging
            parsedNeurons[neuronId] = neuron;
        }

        var finalNeurons = new List<Neuron>();

        // Iterate over the array header to guarantee order and inclusion
        foreach (var declaredId in declaredNeurons)
        {
            if (parsedNeurons.TryGetValue(declaredId, out var parsedNeuron))
            {
                finalNeurons.Add(parsedNeuron);
                parsedNeurons.Remove(declaredId); // Mark as processed
            }
            else
            {
                finalNeurons.Add(new Neuron { Id = declaredId, IsOutput = true });
            }
        }

        // Add any remaining parsed blocks that were mysteriously missing from the header array
        finalNeurons.AddRange(parsedNeurons.Values);

        return finalNeurons;
    }

    private XDocument ParsePLingua(string file)
    {
        string fileContent = File.ReadAllText(file);
        var inputStream = new AntlrInputStream(fileContent);
        var lexer = new PLinguaSNPLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new PLinguaSNPParser(commonTokenStream);
        var tree = parser.program();
        //var visitor = new KpWorkbenchTranslatorVisitor();
        //var xmlVisitor = new PLinguaToXmlVisitor();
        var xmlVisitor = new PLinguaToRawXmlVisitor();
        xmlVisitor.Visit(tree);
        return xmlVisitor.GenerateXmlCode();
    }

    private List<Neuron> ParseUPSimulatorFile(string filePath)
    {
        var neurons = new List<Neuron>();
        string fileContent = File.ReadAllText(filePath);

        // Extract Membrane blocks (e.g., Membrane a { ... })
        var membraneRegex = new Regex(@"Membrane\s+([a-zA-Z0-9_]+)\s*\{([^}]*)\}");
        var matches = membraneRegex.Matches(fileContent);

        foreach (Match match in matches)
        {
            string membraneId = match.Groups[1].Value;
            string blockContent = match.Groups[2].Value;

            var neuron = new Neuron { Id = membraneId, IsOutput = false };

            // Parse Object (Spikes) e.g., "Object a^3;" or "Object a;"
            var objectMatch = Regex.Match(blockContent, @"Object\s+[a-zA-Z]+(?:\^(\d+))?;");
            if (objectMatch.Success)
            {
                string countStr = objectMatch.Groups[1].Value;
                neuron.Spikes = string.IsNullOrEmpty(countStr) ? 1 : int.Parse(countStr);
            }

            // Parse Tunnels (OutSynapses) e.g., "Tunnel b,c;"
            var tunnelMatch = Regex.Match(blockContent, @"Tunnel\s+([^;]+);");
            if (tunnelMatch.Success)
            {
                neuron.OutSynapses = tunnelMatch.Groups[1].Value.Split(',')
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
            }

            // Parse Rules e.g., "Rule r1= a+/ a -> ( a, go all);"
            var ruleMatches = Regex.Matches(blockContent,
                @"Rule\s+[a-zA-Z0-9_]+\s*=\s*([a-zA-Z0-9^+]+)\s*/\s*([a-zA-Z0-9^]+)\s*->\s*\(\s*([a-zA-Z]*)\s*,\s*go\s+([^)]+)\);");
            foreach (Match rm in ruleMatches)
            {
                string regexE = rm.Groups[1].Value.Trim();
                string consumed = rm.Groups[2].Value.Trim();
                string produced = rm.Groups[3].Value.Trim();
                if (string.IsNullOrEmpty(produced) || produced.ToLower() == "lambda")
                {
                    produced = "0";
                }
                string targets = rm.Groups[4].Value.Trim(); // Extracts "all", "b", "b | c"

                // Encode as a Snapse rule but append the targets using a pipe '|' for the XML generator to read
                neuron.Rules.Add($"[{regexE}/{consumed}->{produced};0|{targets}]");
            }

            neurons.Add(neuron);
        }

        return neurons;
    }

    private XDocument GenerateSNPkPML(List<Neuron> neurons)
    {
        var kPSystem = new XElement("kPSystem",
            new XElement("compartmentTypes"),
            new XElement("compartments"),
            new XElement("links")
        );

        var compartmentTypes = kPSystem.Element("compartmentTypes");
        var compartments = kPSystem.Element("compartments");
        var links = kPSystem.Element("links");

        foreach (var neuron in neurons)
        {
            // -- Build Compartment Type --
            var typeElement = new XElement("type", new XAttribute("id", $"t_{neuron.Id}"));

            // If it's not an output neuron and has rules, append the strategy block
            if (!neuron.IsOutput && neuron.Rules.Count > 0)
            {
                // Using 'choice' strategy as the default non-deterministic execution map for SN P systems
                var strategyElement = new XElement("strategy", new XAttribute("type", "choice"));

                int ruleIndex = 1;
                foreach (var ruleStr in neuron.Rules)
                {
                    strategyElement.Add(ParseRule(ruleStr, neuron.OutSynapses, ruleIndex++));
                }

                typeElement.Add(strategyElement);
            }

            compartmentTypes.Add(typeElement);

            // -- Build Compartment Instance --
            // Use KPL native multiset syntax (e.g., "8a" instead of "a^8")
            string initialMultiset = neuron.Spikes > 0 ? $"{neuron.Spikes}a" : "";
            var compartmentElement = new XElement("compartment",
                new XAttribute("id", neuron.Id),
                new XAttribute("type", $"t_{neuron.Id}"),
                new XAttribute("initialMultiset", initialMultiset)
            );
            compartments.Add(compartmentElement);

            // -- Build Links --
            foreach (var target in neuron.OutSynapses)
            {
                links.Add(new XElement("link",
                    new XAttribute("source", neuron.Id),
                    new XAttribute("target", target)
                ));
            }
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), kPSystem);
    }

    private XDocument GenerateRawSnpXml(List<Neuron> neurons)
    {
        // Find the designated output neuron (if any)
        string outputNeuronId = neurons.FirstOrDefault(n => n.IsOutput)?.Id ?? "out";

        var snpSystem = new XElement("snpSystem",
            new XAttribute("type", "standard"),
            new XAttribute("outputNeuron", outputNeuronId)
        );

        // 1. Define the Alphabet
        var alphabet = new XElement("alphabet",
            new XElement("spike", new XAttribute("symbol", "a"))
        );
        snpSystem.Add(alphabet);

        // 2. Define the Neurons and their Rules
        var neuronsElement = new XElement("neurons");
        var synapsesElement = new XElement("synapses");

        foreach (var neuron in neurons)
        {
            var neuronElement = new XElement("neuron",
                new XAttribute("id", neuron.Id),
                new XAttribute("initialSpikes", neuron.Spikes)
            );

            var rulesElement = new XElement("rules");
            int ruleIndex = 1;
            foreach (var ruleStr in neuron.Rules)
            {
                rulesElement.Add(ParseRawRule(ruleStr, ruleIndex++));
            }

            if (neuron.Rules.Count > 0)
            {
                neuronElement.Add(rulesElement);
            }

            neuronsElement.Add(neuronElement);

            // 3. Extract Synapses
            foreach (var target in neuron.OutSynapses)
            {
                synapsesElement.Add(new XElement("synapse",
                    new XAttribute("source", neuron.Id),
                    new XAttribute("target", target)
                ));
            }
        }

        snpSystem.Add(neuronsElement);
        snpSystem.Add(synapsesElement);

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), snpSystem);
    }

    private XElement ParseRawRule(string rule, int ruleId)
    {
        rule = rule.Trim('[', ']');
        string[] parts = rule.Split('|');
        string ruleCore = parts[0];

        string specificTargets = "";
        if (parts.Length > 1 && parts[1] != "all")
        {
            var targetMatches = Regex.Matches(parts[1], @"[a-zA-Z0-9_]+");
            var targetList = targetMatches.Cast<Match>().Select(m => m.Value).ToList();
            specificTargets = string.Join(",", targetList);
        }

        var match = Regex.Match(ruleCore, @"^([aA-Za-z0-9_+*^()]+)/([aA-Za-z0-9_^]+)->([aA-Za-z0-9_^]*|0|lambda);(\d+)$");

        if (!match.Success)
        {
            return new XElement("rule", new XComment($"Failed to parse rule format: {ruleCore}"));
        }

        string regexE = match.Groups[1].Value;
        string consumedStr = match.Groups[2].Value;
        string producedStr = match.Groups[3].Value;
        string delay = match.Groups[4].Value;

        // FIXED: Safely calculate spike amounts regardless of Snapse/UPSimulator syntax
        int consumedCount = ParseSpikeCount(consumedStr);
        int producedCount = ParseSpikeCount(producedStr);

        string ruleType = producedCount > 0 ? "spiking" : "forgetting";

        var ruleElement = new XElement("rule",
            new XAttribute("id", $"r{ruleId}"),
            new XAttribute("regex", regexE),
            new XAttribute("consumed", consumedCount),
            new XAttribute("produced", producedCount),
            new XAttribute("delay", delay),
            new XAttribute("type", ruleType)
        );

        if (!string.IsNullOrEmpty(specificTargets))
        {
            ruleElement.Add(new XAttribute("targets", specificTargets));
        }

        return ruleElement;
    }

    private XElement ParseRule(string rule, List<string> outSynapses, int ruleId)
    {
        rule = rule.Trim('[', ']');

        // Check if there are specific targets appended (used by UPSimulator rules)
        string[] parts = rule.Split('|');
        string ruleCore = parts[0];
        List<string> ruleTargets = outSynapses; // Default to standard tunnels

        if (parts.Length > 1)
        {
            string specificTargets = parts[1];
            if (specificTargets != "all")
            {
                // Extracts the membrane names, converting "b | c" or "b & c" into a clean list ["b", "c"]
                ruleTargets = Regex.Matches(specificTargets, @"[a-zA-Z0-9_]+")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToList();
            }
        }

        var match = Regex.Match(ruleCore, @"^([a]+|a\+)/([a]+)->([a]*|0);(\d+)$");

        if (!match.Success)
        {
            return new XElement("rule", new XComment($"Failed to parse rule format: {ruleCore}"));
        }

        string regexE = match.Groups[1].Value;
        string consumed = match.Groups[2].Value;
        string produced = match.Groups[3].Value;
        string delay = match.Groups[4].Value;

        int c = consumed.Length;
        int p = (produced == "0" || string.IsNullOrEmpty(produced)) ? 0 : produced.Length;

        string lhs = $"{c}a";

        string guard = "";
        if (regexE == "a+")
        {
            guard = ">=1a";
        }
        else if (Regex.IsMatch(regexE, @"^[a]+$"))
        {
            guard = $"={regexE.Length}a";
        }

        string rhs = "";
        if (p > 0)
        {
            var targets = ruleTargets.Select(target => $"{p}a (t_{target})");
            rhs = string.Join(", ", targets);
        }

        return new XElement("rule",
            new XAttribute("id", $"r{ruleId}"),
            new XAttribute("guard", guard),
            new XAttribute("lhs", lhs),
            new XAttribute("rhs", rhs),
            new XAttribute("delay", delay)
        );
    }

    private int ParseSpikeCount(string spikeStr)
    {
        if (string.IsNullOrEmpty(spikeStr) || spikeStr == "0" || spikeStr.ToLower() == "lambda")
            return 0;

        var match = Regex.Match(spikeStr, @"^a\^(\d+)$");
        if (match.Success)
            return int.Parse(match.Groups[1].Value);

        return spikeStr.Length;
    }

    public class Neuron
    {
        public string Id { get; set; }

        public int Spikes { get; set; }

        public List<string> Rules { get; set; } = new List<string>();

        public List<string> OutSynapses { get; set; } = new List<string>();

        public bool IsOutput { get; set; }

        public int Delay { get; set; }

        //storedGive is an integer that specifies the number of spikes a closed neuron produces when it fires (storedGive = 0 if the neuron
        // is open). storedConsume is an integer that specifies the number of spikes a closed neuron consumes when it fires (storedConsume = 0 if the neuron is open)
        public int StoredGive { get; set; }

        // storedConsume is an integer that specifies the number of spikes a closed neuron consumes when it fires (storedConsume = 0 if the neuron is open)
        public int StoredConsume { get; set; }
    }
}