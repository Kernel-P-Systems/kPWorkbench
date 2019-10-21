using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpLingua {
    public class KpLinguaTranslator {

        public static string TranslateMultiset(Multiset ms)
        {
            StringBuilder buf = new StringBuilder();
            if (ms.IsEmpty())
            {
                buf.Append("{}");
            }
            else
            {
                int i = 1;
                int count = ms.Count;
                foreach (KeyValuePair<string, int> kv in ms)
                {
                    if (kv.Value > 1)
                    {
                        buf.Append(kv.Value);
                    }
                    buf.Append(kv.Key);
                    if (i++ < count)
                    {
                        buf.Append(", ");
                    }
                }
            }
            return buf.ToString();
        }

        public static string TranslateTargetedMultiset(IEnumerable<TargetedMultiset> tms)
        {
            StringBuilder buf = new StringBuilder();

            int i = 1;
            int count = tms.Count();
            foreach (TargetedMultiset tm in tms)
            {
                if (tm.Multiset.Count > 0)
                {
                    if (tm.Target is InstanceIdentifier)
                    {
                        InstanceIdentifier iid = tm.Target as InstanceIdentifier;
                        if (iid.Indicator == InstanceIndicator.TYPE)
                        {
                            if (tm.Multiset.Count > 1)
                            {
                                buf.Append("{");
                            }
                            buf.Append(TranslateMultiset(tm.Multiset));
                            if (tm.Multiset.Count > 1)
                            {
                                buf.Append("}");
                            }

                            buf.Append(" (").Append(iid.Value).Append(")");
                            if (i++ < count)
                            {
                                buf.Append(", ");
                            }
                        }
                    }
                }
            }

            return buf.ToString();
        }

        public static string TranslateGuard(IGuard guard)
        {
            StringBuilder buf = new StringBuilder();
            if (guard is BasicGuard)
            {
                BasicGuard bg = guard as BasicGuard;
                switch (bg.Operator)
                {
                    case RelationalOperator.EQUAL: buf.Append("="); break;
                    case RelationalOperator.GEQ: buf.Append(">="); break;
                    case RelationalOperator.GT: buf.Append(">"); break;
                    case RelationalOperator.LEQ: buf.Append("<="); break;
                    case RelationalOperator.LT: buf.Append("<"); break;
                    case RelationalOperator.NOT_EQUAL: buf.Append("!"); break;
                }
                //buf.Append(" ");
                if (bg.Multiset.Count > 1)
                {
                    buf.Append("{").Append(TranslateMultiset(bg.Multiset)).Append("}");
                }
                else
                {
                    buf.Append(TranslateMultiset(bg.Multiset));
                }
            }
            else if (guard is NegatedGuard)
            {
                buf.Append("!(").Append(TranslateGuard((guard as NegatedGuard).Operand)).Append(")");
            }
            else if (guard is CompoundGuard)
            {
                CompoundGuard cg = guard as CompoundGuard;
                bool useP = false;
                if (cg.Lhs is CompoundGuard)
                {
                    CompoundGuard cgLhs = cg.Lhs as CompoundGuard;
                    if (cgLhs.Operator != cg.Operator)
                    {
                        useP = true;
                    }
                }

                if (useP)
                {
                    buf.Append("(").Append(TranslateGuard(cg.Lhs)).Append(")");
                }
                else
                {
                    buf.Append(TranslateGuard(cg.Lhs));
                }

                if (cg.Operator == BinaryGuardOperator.AND)
                {
                    buf.Append(" & ");
                }
                else
                {
                    buf.Append(" | ");
                }

                useP = false;
                if (cg.Rhs is CompoundGuard)
                {
                    CompoundGuard cgRhs = cg.Rhs as CompoundGuard;
                    if (cgRhs.Operator != cg.Operator)
                    {
                        useP = true;
                    }
                }

                if (useP)
                {
                    buf.Append("(").Append(TranslateGuard(cg.Rhs)).Append(")");
                }
                else
                {
                    buf.Append(TranslateGuard(cg.Rhs));
                }
            }

            return buf.ToString();
        }

        public static string TranslateRule(Rule r, MType container = null)
        {
            StringBuilder buf = new StringBuilder();

            if (r is ConsumerRule)
            {
                ConsumerRule rule = r as ConsumerRule;
                if (r.IsGuarded)
                {
                    buf.Append(TranslateGuard(r.Guard));
                    buf.Append(": ");
                }

                buf.Append(TranslateMultiset(rule.Lhs));
                buf.Append(" -> ");
                switch (r.Type)
                {
                    case RuleType.MULTISET_REWRITING:
                        {
                            buf.Append(TranslateMultiset((r as RewritingRule).Rhs));
                        } break;
                    case RuleType.REWRITE_COMMUNICATION:
                        {
                            RewriteCommunicationRule rcr = r as RewriteCommunicationRule;
                            if (!rcr.Rhs.IsEmpty())
                            {
                                buf.Append(TranslateMultiset(rcr.Rhs));
                                if (rcr.TargetRhs.Count > 0)
                                {
                                    buf.Append(", ");
                                }
                            }
                            buf.Append(TranslateTargetedMultiset(rcr.TargetRhs.Values));
                        } break;
                    case RuleType.MEMBRANE_DISSOLUTION:
                        {
                            buf.Append("#");
                        } break;
                    case RuleType.LINK_CREATION:
                        {
                            LinkRule lr = r as LinkRule;
                            if (lr.Target is InstanceIdentifier)
                            {
                                InstanceIdentifier iid = lr.Target as InstanceIdentifier;
                                if (iid.Indicator == InstanceIndicator.TYPE)
                                {
                                    buf.Append("-(").Append(iid.Value).Append(")");
                                }
                            }
                        } break;
                    case RuleType.LINK_DESTRUCTION:
                        {
                            LinkRule lr = r as LinkRule;
                            if (lr.Target is InstanceIdentifier)
                            {
                                InstanceIdentifier iid = lr.Target as InstanceIdentifier;
                                if (iid.Indicator == InstanceIndicator.TYPE)
                                {
                                    buf.Append("\\-(").Append(iid.Value).Append(")");
                                }
                            }
                        } break;
                    case RuleType.MEMBRANE_DIVISION:
                        {
                            DivisionRule dr = r as DivisionRule;
                            foreach (InstanceBlueprint ib in dr.Rhs)
                            {
                                buf.Append("[");
                                if (!ib.Multiset.IsEmpty())
                                {
                                    buf.Append(TranslateMultiset(ib.Multiset));
                                }
                                buf.Append("]");
                                if (container != null)
                                {
                                    if (container.Name == ib.Type.Name)
                                    {
                                        continue;
                                    }
                                }
                                buf.Append("(").Append(ib.Type.Name).Append(")");
                            }
                        } break;
                }

                buf.Append(" .");
            }

            return buf.ToString();
        }
    }
}
