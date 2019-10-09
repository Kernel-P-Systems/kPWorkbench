using KpCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class JsonWriter {

        private TextWriter owt;
        public TextWriter Channel { get { return owt; } set { owt = value; } }

        private static string nl = System.Environment.NewLine;

        public JsonWriter(TextWriter channel, bool formatted) {
            if (channel == null) {
                throw new ArgumentNullException("channel");
            }
            Channel = channel;
        }

        public JsonWriter(TextWriter channel)
            : this(channel, false) {
        }

        public void Write(KPsystem kp) {
            owt.Write("{");
            if (writePItem(kp)) {
                owt.Write(", ");
            }
            owt.Write("\"types\":[");
            int i = 1;
            foreach (MType type in kp.Types) {
                writeType(type);
                if (i++ < kp.Types.Count) {
                    owt.Write(", ");
                }
            }
            owt.Write("]");
            owt.Write("}");
            owt.Flush();
        }

        public void WriteFormatted(KPsystem kp) {
            TextWriter tw = owt;
            owt = new StringWriter();
            Write(kp);
            tw.Write(JsonFormatter.Format(owt.ToString()));
            tw.Flush();
        }

        private bool writePItem(PItem item) {
            bool hasOne = false;
            if (!String.IsNullOrEmpty(item.Name)) {
                owt.Write("\"name\":\"" + Encode(item.Name.Trim()) + "\"");
                hasOne = true;
            }
            if (!String.IsNullOrEmpty(item.Description)) {
                if (hasOne) {
                    owt.Write(", ");
                }
                owt.Write("\"description\":\"" + Encode(item.Description.Trim()) + "\"");
                hasOne = true;
            }
            return hasOne;
        }

        private void writeType(MType type) {
            owt.Write("{");
            if (writePItem(type)) {
                owt.Write(", ");
            }
            owt.Write("\"instances\":[");
            int i = 1;
            foreach (MInstance instance in type.Instances) {
                writeInstance(instance);
                if (i++ < type.Instances.Count) {
                    owt.Write(", ");
                }
            }
            owt.Write("]");
            owt.Write(", \"strategy\":");
            writeExecutionStrategy(type.ExecutionStrategy);
            owt.Write("}");
        }

        private void writeInstance(MInstance instance) {
            owt.Write("{");
            if (writePItem(instance)) {
                owt.Write(", ");
            }
            owt.Write("\"multiset\":");
            writeMultiset(instance.Multiset);
            owt.Write(", \"links\":[");
            int i = 1;
            foreach (MInstance linked in instance.Connections) {
                if (linked.HasName()) {
                    owt.Write("\"" + linked.Name + "\"");
                } else {
                    owt.Write("\"" + linked.GetHashCode() + "\"");
                }
                if (i++ < instance.Connections.Count) {
                    owt.Write(", ");
                }
            }
            owt.Write("]");
            owt.Write("}");
        }

        private void writeMultiset(Multiset multiset) {
            owt.Write("{");
            int i = 1;
            int count = multiset.Count;
            foreach (KeyValuePair<string, int> m in multiset) {
                owt.Write("\"" + m.Key + "\":" + m.Value);
                if (i++ < count) {
                    owt.Write(", ");
                }
            }
            owt.Write("}");
        }

        private void writeTargetedMultiset(TargetedMultiset tm) {
            owt.Write("{");
            owt.Write("\"instanceIdentifier\":");
            writeInstanceIdentifier(tm.Target);
            owt.Write(", \"multiset\":");
            writeMultiset(tm.Multiset);
            owt.Write("}");
        }

        private void writeInstanceIdentifier(IInstanceIdentifier instanceIdentifier) {
            owt.Write("{");
            if (instanceIdentifier is InstanceIdentifier) {
                InstanceIdentifier iid = instanceIdentifier as InstanceIdentifier;
                owt.Write("\"indicator\":");
                writeInstanceIndicator(iid.Indicator);
                owt.Write(", \"value\":\"" + iid.Value + "\"");
            } else if (instanceIdentifier is CompoundInstanceIdentifier) {
                CompoundInstanceIdentifier ciid = instanceIdentifier as CompoundInstanceIdentifier;
                owt.Write("\"lhs\":");
                writeInstanceIdentifier(ciid.Lhs);
                owt.Write(",\"op\":");
                writeIIOperator(ciid.Operator);
                owt.Write("\"rhs\":");
                writeInstanceIdentifier(ciid.Rhs);
            }
            owt.Write("}");
        }

        private void writeInstanceIndicator(InstanceIndicator indicator) {
            switch (indicator) {
                case InstanceIndicator.TYPE: owt.Write("\"type\""); break;
                case InstanceIndicator.LABEL: owt.Write("\"label\""); break;
                case InstanceIndicator.NAME: owt.Write("\"name\""); break;
            }
        }

        private void writeIIOperator(IIOperator op) {
            switch (op) {
                case IIOperator.AND: owt.Write("\"AND\""); break;
                case IIOperator.OR: owt.Write("\"OR\""); break;
            }
        }

        private void writeExecutionStrategy(ExecutionStrategy strategy) {
            owt.Write("{");
            if (strategy != null && !strategy.IsEmpty()) {
                owt.Write("\"rules\":[");
                int i = 1;
                foreach (Rule rule in strategy.Rules) {
                    writeRule(rule);
                    if (i++ < strategy.Rules.Count) {
                        owt.Write(", ");
                    }
                }
                owt.Write("]");
                owt.Write(", \"operator\":");
                writeStrategyOperator(strategy.Operator);
                if (strategy.Next != null) {
                    owt.Write(", \"next\":");
                    writeExecutionStrategy(strategy.Next);
                }
            }
            owt.Write("}");
        }

        private void writeStrategyOperator(StrategyOperator op) {
            switch (op) {
                case StrategyOperator.SEQUENCE: owt.Write("\"sequence\""); break;
                case StrategyOperator.ARBITRARY: owt.Write("\"aribtrary\""); break;
                case StrategyOperator.MAX: owt.Write("\"max\""); break;
                case StrategyOperator.CHOICE: owt.Write("\"choice\""); break;
            }
        }

        private void writeRule(Rule rule) {
            owt.Write("{");
            bool pItem = writePItem(rule);
            if (!rule.IsEmpty()) {
                if (pItem) {
                    owt.Write(", ");
                }

                if (rule.IsGuarded) {
                    owt.Write("\"guard\":");
                    writeGuard(rule.Guard);
                    owt.Write(", ");
                }

                if (rule.Type == RuleType.CONSUMER) {
                    owt.Write("\"lhs\":");
                    writeMultiset((rule as ConsumerRule).Lhs);
                } else if (rule.Type == RuleType.MULTISET_REWRITING) {
                    RewritingRule r = rule as RewritingRule;
                    owt.Write("\"lhs\":");
                    writeMultiset(r.Lhs);
                    owt.Write(", \"rhs\":");
                    writeMultiset(r.Rhs);
                } else if (rule.Type == RuleType.REWRITE_COMMUNICATION) {
                    RewriteCommunicationRule r = rule as RewriteCommunicationRule;
                    owt.Write("\"lhs\":");
                    writeMultiset(r.Lhs);
                    owt.Write(", \"rhs\":");
                    writeMultiset(r.Rhs);
                    owt.Write(", \"targetRhs\":[");
                    int i = 1;
                    foreach (TargetedMultiset tm in r.TargetRhs.Values) {
                        writeTargetedMultiset(tm);
                        if (i++ < r.TargetRhs.Count) {
                            owt.Write(", ");
                        }
                    }
                    owt.Write("]");
                } else if (rule.Type == RuleType.MEMBRANE_DIVISION) {
                    DivisionRule r = rule as DivisionRule;
                    owt.Write("\"lhs\":");
                    writeMultiset(r.Lhs);
                    owt.Write(", \"instances\":[");
                    int i = 1;
                    foreach (InstanceBlueprint ib in r.Rhs) {
                        writeInstanceBlueprint(ib);
                        if (i++ < r.Rhs.Count) {
                            owt.Write(", ");
                        }
                    }
                    owt.Write("]");
                } else if (rule.Type == RuleType.LINK_CREATION) {
                    LinkRule r = rule as LinkRule;
                    owt.Write("\"lhs\":");
                    writeMultiset(r.Lhs);
                    owt.Write(",\"linkCreate\":");
                    writeInstanceIdentifier(r.Target);
                } else if (rule.Type == RuleType.LINK_DESTRUCTION) {
                    LinkRule r = rule as LinkRule;
                    owt.Write("\"lhs\":");
                    writeMultiset(r.Lhs);
                    owt.Write(",\"linkDestroy\":");
                    writeInstanceIdentifier(r.Target);
                } else if (rule.Type == RuleType.MEMBRANE_DISSOLUTION) {
                    DissolutionRule r = rule as DissolutionRule;
                    owt.Write("\"lhs\":");
                    writeMultiset(r.Lhs);
                    owt.Write(", \"dissolve\": true");
                }
            }

            owt.Write("}");
        }

        public void writeGuard(IGuard g) {
            if (g == null) {
                return;
            }
            owt.Write("{");
            if (g is BasicGuard) {
                BasicGuard bg = g as BasicGuard;
                owt.Write("\"operator\":");
                switch (bg.Operator) {
                    case RelationalOperator.EQUAL: owt.Write("\"eq\""); break;
                    case RelationalOperator.GEQ: owt.Write("\"geq\""); break;
                    case RelationalOperator.GT: owt.Write("\"gt\""); break;
                    case RelationalOperator.LT: owt.Write("\"lt\""); break;
                    case RelationalOperator.LEQ: owt.Write("\"leq\""); break;
                    case RelationalOperator.NOT_EQUAL: owt.Write("\"neq\""); break;
                }
                owt.Write(", \"multiset\":");
                writeMultiset(bg.Multiset);
            } else if (g is NegatedGuard) {
                NegatedGuard ng = g as NegatedGuard;
                owt.Write("\"operator\":\"not\"");
                owt.Write(", \"operatnd\":");
                writeGuard(ng.Operand);
            } else if (g is CompoundGuard) {
                CompoundGuard cg = g as CompoundGuard;
                owt.Write("\"operator\":");
                switch (cg.Operator) {
                    case BinaryGuardOperator.AND: owt.Write("\"and\""); break;
                    case BinaryGuardOperator.OR: owt.Write("\"or\""); break;
                }
                owt.Write(", \"lhs\":");
                writeGuard(cg.Lhs);
                owt.Write(", \"rhs\":");
                writeGuard(cg.Rhs);
            }
            owt.Write("}");
        }

        private void writeInstanceBlueprint(InstanceBlueprint ib) {
            owt.Write("{");
            owt.Write("\"type\":\"" + ib.Type.Name + "\"");
            owt.Write(", \"multiset\":");
            writeMultiset(ib.Multiset);
            owt.Write("}");
        }

        public static string Encode(string src) {
            return src.Replace("\"", "\\\"").Replace(nl, "\\n");
        }
    }

    public class JsonFormatter {
        public static string Indent = "    ";

        public static string Format(string input) {
            StringBuilder output = new StringBuilder(input.Length * 2);
            char? quote = null;
            int depth = 0;

            for (int i = 0; i < input.Length; ++i) {
                char ch = input[i];

                switch (ch) {
                    case '{':
                    case '[':
                        output.Append(ch);
                        if (!quote.HasValue) {
                            output.AppendLine();
                            output.Append(Indent.Repeat(++depth));
                        }
                        break;
                    case '}':
                    case ']':
                        if (quote.HasValue)
                            output.Append(ch);
                        else {
                            output.AppendLine();
                            output.Append(Indent.Repeat(--depth));
                            output.Append(ch);
                        }
                        break;
                    case '"':
                    case '\'':
                        output.Append(ch);
                        if (quote.HasValue) {
                            if (!output.IsEscaped(i))
                                quote = null;
                        } else quote = ch;
                        break;
                    case ',':
                        output.Append(ch);
                        if (!quote.HasValue) {
                            output.AppendLine();
                            output.Append(Indent.Repeat(depth));
                        }
                        break;
                    case ':':
                        if (quote.HasValue) output.Append(ch);
                        else output.Append(" : ");
                        break;
                    default:
                        if (quote.HasValue || !char.IsWhiteSpace(ch))
                            output.Append(ch);
                        break;
                }
            }

            return output.ToString();
        }
    }

    public static class StringExtensions {
        public static string Repeat(this string str, int count) {
            return new StringBuilder().Insert(0, str, count).ToString();
        }

        public static bool IsEscaped(this string str, int index) {
            bool escaped = false;
            while (index > 0 && str[--index] == '\\') escaped = !escaped;
            return escaped;
        }

        public static bool IsEscaped(this StringBuilder str, int index) {
            return str.ToString().IsEscaped(index);
        }
    }
}
