using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpikingNeuralPreprocessing
{
    public class KpWorkbenchTranslatorVisitor : PLinguaSNPBaseVisitor<object>
    {
        private HashSet<string> GlobalVariables = new HashSet<string>();
        private Dictionary<string, List<string>> TypeRules = new Dictionary<string, List<string>>();
        private List<string> CompartmentInstances = new List<string>();
        private List<string> Links = new List<string>();
        private Dictionary<string, string> InitialSpikes = new Dictionary<string, string>();


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

        public override object VisitMuStmt(PLinguaSNPParser.MuStmtContext context)
        {

            string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

            foreach (var neuronCtx in context.neuronList().neuronId())
            {
                string baseType = TranslateNeuronBaseType(neuronCtx);
                string instanceId = TranslateNeuronId(neuronCtx);

                // Ensure the type exists in our dictionary
                if (!TypeRules.ContainsKey(baseType))
                {
                    TypeRules[baseType] = new List<string>();
                }

                // Register the compartment instance
                CompartmentInstances.Add($"{instanceId} {{}} ({baseType}){loop}");
            }
            return null;
        }

        public override object VisitMsStmt(PLinguaSNPParser.MsStmtContext context)
        {
            // Maps P-Lingua: @ms(d{0}) = a*6; 
            // We store this to inject it into the compartment instance later
            string instanceId = TranslateNeuronId(context.neuronId());
            string multiset = TranslateMultiset(context.multiset());

            InitialSpikes[instanceId] = multiset;
            return null;
        }


        public override object VisitMarcsStmt(PLinguaSNPParser.MarcsStmtContext context)
        {
            string sourceId = TranslateNeuronId(context.neuronId(0));
            string targetId = TranslateNeuronId(context.neuronId(1));
            string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

            Links.Add($"{sourceId} - {targetId}{loop}");
            return null;
        }

        public override object VisitRuleStmt(PLinguaSNPParser.RuleStmtContext context)
        {
            string lhs = TranslateMultiset(context.multiset(0));

            string rhs = context.GetChild(3).GetText() == "#" ? "lambda" : TranslateMultiset(context.multiset(1));

            string baseType = TranslateNeuronBaseType(context.neuronId());
            string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

            // Note: In kPWorkbench, you generally specify the target type in the RHS for broadcasting (e.g., 5a (t_out)). 
            // Because P-Lingua rules don't contain the target (it relies on @marcs), we output a standard rule. 
            // You may need to adapt this depending on how you route objects over links.
            string ruleText = $"{lhs} -> {rhs}{loop} .";

            if (context.guard() != null)
            {
                string rawGuard = context.guard().GetText().Trim('"');
                string kpGuard = rawGuard.Contains("*") ? $"={rawGuard.Split('*')[1]}{rawGuard.Split('*')[0]}" : $"=1{rawGuard}";
                ruleText = $"{kpGuard} : {ruleText}";
            }

            if (!TypeRules.ContainsKey(baseType)) TypeRules[baseType] = new List<string>();
            TypeRules[baseType].Add(ruleText);

            return null;
        }

        public override object VisitTopoRuleStmt(PLinguaSNPParser.TopoRuleStmtContext context)
        {
            // Matches: []'0 --> []'t{1} || []'f{1} "a";
            // Translates to kPWorkbench division: a -> [][t_t$1$] [][t_f$1$]

            string sourceNeuron = TranslateNeuronBaseType(context.neuronId(0));
            string target1Type = TranslateNeuronBaseType(context.neuronId(1));
            string target2Type = TranslateNeuronBaseType(context.neuronId(2));

            string loop = context.indexLoop() != null ? VisitIndexLoop(context.indexLoop()) : "";

            // Extract the triggering spike (guard)
            string spike = context.guard() != null ? context.guard().GetText().Trim('"') : "a";

            // kPWorkbench membrane division format: lhs -> [rhs1][rhs2]
            string ruleText = $"{spike} -> []({target1Type}) []({target2Type}){loop} .";

            if (!TypeRules.ContainsKey(sourceNeuron)) TypeRules[sourceNeuron] = new List<string>();

            // Note: We flag this with a comment as division/budding might require specific kP strategy tuning
            TypeRules[sourceNeuron].Add($"/* Topology Rule */ {ruleText}");

            return null;
        }

        private string TranslateMultiset(PLinguaSNPParser.MultisetContext context)
        {
            // Translates a*5 to 5a
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
            // Case 1: Plain ID or INT (e.g., "in" or "0")
            if (context.ChildCount == 1) return context.GetText();

            // Parse the expressions inside the braces (e.g., "{i+1}" -> "$i$+1")
            var exprs = context.exprList().expr().Select(e =>
                e is PLinguaSNPParser.VarExprContext ? $"${e.GetText()}$" : e.GetText()
            );

            // Case 2: Nameless parameter (e.g., "{i+1}")
            if (context.GetChild(0).GetText() == "{")
            {
                return $"id_{string.Join("_", exprs)}"; // Prefix with id_ to make it a valid variable name
            }

            // Case 3: Standard parameterized (e.g., "Cx{i, 0}")
            string baseId = context.ID().GetText();
            return $"{baseId}_{string.Join("_", exprs)}";
        }

        private string TranslateNeuronBaseType(PLinguaSNPParser.NeuronIdContext context)
        {
            // Case 1: Plain ID or INT
            if (context.ChildCount == 1) return $"t_{context.GetText()}";

            // Case 2: Nameless parameter
            if (context.GetChild(0).GetText() == "{") return "t_dynamic";

            // Case 3: Standard parameterized
            return $"t_{context.ID().GetText()}";
        }

        private string VisitIndexLoop(PLinguaSNPParser.IndexLoopContext context)
        {
            var iterators = context.iterator().Select(i => i.GetText());
            return $" : {string.Join(", ", iterators)}";
        }

        private string TranslateExpr(PLinguaSNPParser.ExprContext context)
        {
            // Wrap variables in $ if needed, otherwise return plain text
            return context.GetText();
        }

        public string GenerateKplCode()
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Automatically translated from P-Lingua to kPWorkbench\n");
            if (GlobalVariables.Count > 0)
            {
                sb.Append("#define ");
                sb.AppendLine(string.Join(", ", GlobalVariables.Select(v => $"{v} = 10 /* TODO: Set Value */")));
                sb.AppendLine();
            }

            foreach (var type in TypeRules)
            {
                sb.AppendLine($"type {type.Key} {{");
                sb.AppendLine("    choice {"); // Assuming non-deterministic choice for SNP
                foreach (var rule in type.Value)
                {
                      sb.AppendLine($"        {rule}");
                }
                sb.AppendLine("    }");
                sb.AppendLine("}\n");
            }
            foreach (var instance in CompartmentInstances)
            {
                string finalInstance = instance;
                foreach (var spike in InitialSpikes)
                {
                    if (instance.StartsWith(spike.Key + " "))
                    {
                        finalInstance = instance.Replace("{}", $"{{{spike.Value}}}");
                    }
                }
                sb.AppendLine($"{finalInstance} .");
            }
            sb.AppendLine();

            foreach (var link in Links)
            {
                sb.AppendLine($"{link} .");
            }

            return sb.ToString();
        }
    }
}