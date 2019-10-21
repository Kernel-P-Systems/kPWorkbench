using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpLingua {
    public static class KpLingua {

        public static string Rule(Rule r) {
            StringBuilder buf = new StringBuilder();

            if (r.IsGuarded) {
                buf.Append(Guard(r.Guard)).Append(": ");
            }

            if (r is ConsumerRule) {
                buf.Append(Multiset((r as ConsumerRule).Lhs)).Append(" -> ");
                switch (r.Type) {
                    case RuleType.MULTISET_REWRITING: {
                        buf.Append(Multiset((r as RewritingRule).Rhs));
                        } break;
                    case RuleType.REWRITE_COMMUNICATION: {
                        RewriteCommunicationRule rcr = r as RewriteCommunicationRule;
                        if (!rcr.Rhs.IsEmpty()) {
                            buf.Append(Multiset(rcr.Rhs)).Append(", ");
                        }
                        int tCount = rcr.TargetRhs.Count;
                        int i = 1;
                        foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in rcr.TargetRhs) {
                            if (kv.Key is InstanceIdentifier) {
                                if ((kv.Key as InstanceIdentifier).Indicator == InstanceIndicator.TYPE) {
                                    buf.AppendFormat("{{{0}}}({1})", Multiset(kv.Value.Multiset), (kv.Key as InstanceIdentifier).Value);
                                    if (i < tCount) {
                                        buf.Append(", ");
                                    }
                                }
                            }
                        }
                        } break;
                    case RuleType.MEMBRANE_DISSOLUTION: {
                        buf.Append("#");
                        } break;
                    case RuleType.MEMBRANE_DIVISION: {
                        DivisionRule dr = r as DivisionRule;
                        foreach (InstanceBlueprint ib in dr.Rhs) {
                            buf.AppendFormat("[{0}]({1})", Multiset(ib.Multiset, true), ib.Type.Name);
                        }
                        } break;
                    case RuleType.LINK_CREATION: {
                        LinkRule lr = r as LinkRule;
                        if (lr.Target is InstanceIdentifier) {
                            InstanceIdentifier iid = lr.Target as InstanceIdentifier;
                            if (iid.Indicator == InstanceIndicator.TYPE) {
                                buf.AppendFormat("- ({0})", iid.Value);
                            }
                        }
                        } break;
                    case RuleType.LINK_DESTRUCTION: {
                        LinkRule lr = r as LinkRule;
                        if (lr.Target is InstanceIdentifier) {
                            InstanceIdentifier iid = lr.Target as InstanceIdentifier;
                            if (iid.Indicator == InstanceIndicator.TYPE) {
                                buf.AppendFormat("\\- ({0})", iid.Value);
                            }
                        }
                        } break;
                }
            }

            return buf.ToString();
        }

        public static string Instance(MInstance instance) {
            StringBuilder buf = new StringBuilder();

            return buf.ToString();
        }

        public static string Guard(IGuard guard) {
            StringBuilder buf = new StringBuilder();

            if (guard is BasicGuard) {
                BasicGuard g = guard as BasicGuard;
                switch (g.Operator) {
                    case RelationalOperator.EQUAL: {
                        buf.Append("=");
                        } break;
                    case RelationalOperator.NOT_EQUAL: {
                        buf.Append("!");
                        } break;
                    case RelationalOperator.GEQ: {
                        buf.Append(">=");
                        } break;
                    case RelationalOperator.LEQ: {
                        buf.Append("<=");
                        } break;
                    case RelationalOperator.LT: {
                        buf.Append("<");
                        } break;
                    case RelationalOperator.GT: {
                        buf.Append(">");
                        } break;
                }

                buf.Append(Multiset(g.Multiset));
            } else if (guard is NegatedGuard) {
                NegatedGuard g = guard as NegatedGuard;
                buf.AppendFormat("!({0})", Guard(g.Operand));
            } else if (guard is CompoundGuard) {
                CompoundGuard g = guard as CompoundGuard;
                
                bool useP = false;
                if (g.Lhs is CompoundGuard) {
                    CompoundGuard cgLhs = g.Lhs as CompoundGuard;
                    if (cgLhs.Operator != g.Operator) {
                        useP = true;
                    }
                }

                if (useP) {
                    buf.Append("(").Append(Guard(g.Lhs)).Append(")");
                } else {
                    buf.Append(Guard(g.Lhs));
                }

                if (g.Operator == BinaryGuardOperator.AND) {
                    buf.Append(" & ");
                } else {
                    buf.Append(" | ");
                }

                useP = false;
                if (g.Rhs is CompoundGuard) {
                    CompoundGuard cgRhs = g.Rhs as CompoundGuard;
                    if (cgRhs.Operator != g.Operator) {
                        useP = true;
                    }
                }

                if (useP) {
                    buf.Append("(").Append(Guard(g.Rhs)).Append(")");
                } else {
                    buf.Append(Guard(g.Rhs));
                }
            }


            return buf.ToString();
        }

        public static string Multiset(Multiset ms, bool blankIfEmpty = false) {
            StringBuilder buf = new StringBuilder();

            int count = ms.Count;
            if (count == 0) {
                if (!blankIfEmpty) {
                    buf.Append("{}");
                }
            } else  {
                int i = 1;
                foreach (KeyValuePair<string, int> kv in ms) {
                    if (kv.Value == 0) {
                        continue;
                    } else if (kv.Value == 1) {
                        buf.AppendFormat("{0}", kv.Key);
                    } else {
                        buf.AppendFormat("{0}{1}", kv.Value, kv.Key);
                    }
                    if (i++ < count) {
                        buf.Append(", ");
                    }
                }
            }

            return buf.ToString();
        }

        public static string ToKpl(this Rule r) {
            return Rule(r);
        }

        public static string ToKpl(this MInstance instance) {
            return Instance(instance);
        }

        public static string ToKpl(this IGuard guard) {
            return Guard(guard);
        }

        public static string ToKpl(this Multiset multiset) {
            return Multiset(multiset);
        }

    }
}
