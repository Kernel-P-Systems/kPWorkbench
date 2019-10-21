using KpCore;
using KpUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace KpXML {

    public class KpSystemXMLReader {

        private string fileName;
        public string FileName { get { return fileName; } set { fileName = value; } }

        private KPsystem kp;
        private MType currentType;
        private Dictionary<string, MInstance> namedInstances;

        public KpSystemXMLReader(string fileName) {
            FileName = fileName;
        }

        public KPsystem Read() {
            kp = new KPsystem();

            XPathDocument doc = new XPathDocument(FileName);
            XPathNavigator nav = doc.CreateNavigator();
            XPathNavigator root = nav.SelectSingleNode("/kpsystem");
            XPathNavigator meta = root.SelectSingleNode("meta");
            readPItem(meta, kp);

            if (namedInstances == null) {
                namedInstances = new Dictionary<string, MInstance>();
            } else {
                namedInstances.Clear();
            }

            //iterate through all nodes that contain 
            XPathNodeIterator mtype = root.SelectChildren("mtype", String.Empty);
            while (mtype.MoveNext()) {
                string mtypeName = mtype.Current.GetAttribute("name", String.Empty);
                MType mt = kp[mtypeName];
                currentType = mt;
                readPItem(mtype.Current, mt);
                XPathNodeIterator it = mtype.Current.SelectChildren("instance", String.Empty);
                while (it.MoveNext()) {
                    mt.Instances.Add(readInstance(it.Current));
                }

                Dictionary<string, Rule> rules = new Dictionary<string, Rule>();
                it = mtype.Current.SelectChildren("rule", String.Empty);
                while (it.MoveNext()) {
                    Rule r = readRule(it.Current);
                    rules.Add(r.Name, r);
                }

                ExecutionStrategyHelper sh = null;
                XPathNavigator s = mtype.Current.SelectSingleNode("strategy");
                if (s != null) {
                    sh = readExecutionStrategy(s);
                }

                ExecutionStrategy ex = null;
                ExecutionStrategy exit = null;
                ExecutionStrategyHelper shit = sh;
                while (shit != null) {
                    if (exit == null) {
                        exit = new ExecutionStrategy(shit.Operator);
                        ex = exit;
                    } else {
                        exit.Next = new ExecutionStrategy(shit.Operator);
                        exit = exit.Next;
                    }
                    foreach (string r in shit.Rules) {
                        Rule rule = null;
                        rules.TryGetValue(r, out rule);
                        if (rule != null) {
                            exit.Rules.Add(rules[r]);
                        }
                    }
                    shit = shit.Next;
                }
                mt.ExecutionStrategy = ex;
            }

            XPathNodeIterator links = root.SelectChildren("link", String.Empty);
            while (links.MoveNext()) {
                XPathNavigator lnav = links.Current;
                InstanceIdentifier lhs = readInstanceIdentifier(lnav);
                if (lhs.Indicator == InstanceIndicator.NAME) {
                    MInstance lhsVal = null;
                    if (namedInstances.TryGetValue(lhs.Value, out lhsVal)) {
                        InstanceIdentifier rhs = readInstanceIdentifier(lnav.SelectSingleNode("with"));
                        if (rhs.Indicator == InstanceIndicator.NAME) {
                            MInstance rhsVal = null;
                            if (namedInstances.TryGetValue(rhs.Value, out rhsVal)) {
                                if (!Object.ReferenceEquals(rhsVal, lhsVal)) {
                                    lhsVal.ConnectBidirectionalTo(rhsVal);
                                }
                            }
                        } else if (rhs.Indicator == InstanceIndicator.TYPE) {
                            if(!String.IsNullOrEmpty(rhs.Value)) {
                                MType t = kp[rhs.Value];
                                foreach (MInstance inst in t.Instances) {
                                    if (!Object.ReferenceEquals(inst, lhsVal)) {
                                        lhsVal.ConnectBidirectionalTo(inst);
                                    }
                                }
                            }
                        }
                    }
                } else if (lhs.Indicator == InstanceIndicator.TYPE) {
                    if (!String.IsNullOrEmpty(lhs.Value)) {
                        MType t = kp[lhs.Value];
                        InstanceIdentifier rhs = readInstanceIdentifier(lnav.SelectSingleNode("with"));
                        if (rhs.Indicator == InstanceIndicator.NAME) {
                            MInstance rhsVal = null;
                            if (namedInstances.TryGetValue(rhs.Value, out rhsVal)) {
                                foreach(MInstance inst in t.Instances) {
                                    if (!Object.ReferenceEquals(inst, rhsVal)) {
                                        rhsVal.ConnectBidirectionalTo(inst);
                                    }
                                }
                            }
                        } else if (rhs.Indicator == InstanceIndicator.TYPE) {
                            if (!String.IsNullOrEmpty(rhs.Value)) {
                                MType t2 = kp[rhs.Value];
                                foreach (MInstance inst in t.Instances) {
                                    foreach (MInstance inst2 in t2.Instances) {
                                        if (!Object.ReferenceEquals(inst, inst2)) {
                                            inst.ConnectBidirectionalTo(inst2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return kp;
        }

        //add other meta items, not just name and description
        private PItem readPItem(XPathNavigator nav, PItem item = null) {
            if (item == null) {
                item = new PItem();
            }

            string attr = nav.GetAttribute("name", String.Empty);
            if (attr != null) {
                item.Name = attr;
            }
            attr = nav.GetAttribute("description", String.Empty);
            if (attr != null) {
                item.Description = attr;
            }

            XPathNavigator node = nav.SelectSingleNode("name");
            if (node != null) {
                item.Name = node.Value;
            }
            node = nav.SelectSingleNode("description");
            if (node != null) {
                item.Description = node.Value;
            }

            return item;
        }

        private MTuple readTuple(XPathNavigator nav) {
            string obj = nav.GetAttribute("obj", String.Empty);
            if (String.IsNullOrEmpty(obj)) {
                obj = nav.GetAttribute("object", String.Empty);
            }
            string multiplicity = nav.Value;
            if (!(String.IsNullOrEmpty(obj) || String.IsNullOrEmpty(multiplicity))) {
                return new MTuple(obj, Int32.Parse(multiplicity));
            }

            return null;
        }

        private Multiset readMultiset(XPathNavigator parentNode, Multiset x = null) {
            return readMultiset(parentNode, true, x);
        }

        private Multiset readMultiset(XPathNavigator parentNode, bool ignoreNullValues, Multiset x = null) {
            if (parentNode == null) {
                return x;
            }

            if (x == null) {
                x = new Multiset();
            }

            XPathNodeIterator mss = parentNode.SelectChildren("ms", String.Empty);
            while (mss.MoveNext()) {
                MTuple t = readTuple(mss.Current);
                if (t != null) {
                    x.Add(t.Obj, t.Multiplicity, ignoreNullValues);
                }
            }

            return x;
        }

        private TargetedMultiset readTargetedMultiset(XPathNavigator parentNode, TargetedMultiset x = null) {

            if (parentNode == null) {
                return x;
            }

            InstanceIdentifier ins = readInstanceIdentifier(parentNode);

            if (x == null) {
                x = new TargetedMultiset(ins);
            } else {
                x.Target = ins;
            }
            readMultiset(parentNode, x.Multiset);

            return x;
        }

        private MInstance readInstance(XPathNavigator nav, MInstance m = null) {

            if (m == null) {
                m = new MInstance();
            }

            readPItem(nav, m);
            readMultiset(nav, m.Multiset);

            if (m.HasName()) {
                namedInstances.Add(m.Name, m);
            }

            return m;
        }

        private InstanceIdentifier readInstanceIdentifier(XPathNavigator nav) {
            InstanceIdentifier id = null;
            string name = nav.GetAttribute("name", String.Empty);
            if (String.IsNullOrEmpty(name)) {
                string mtype = nav.GetAttribute("mtype", String.Empty);
                if (String.IsNullOrEmpty(mtype)) {
                    string label = nav.GetAttribute("label", String.Empty);
                    if (!String.IsNullOrEmpty(label)) {
                        id = new InstanceIdentifier(InstanceIndicator.LABEL, label);
                    }
                } else {
                    id = new InstanceIdentifier(InstanceIndicator.TYPE, mtype);
                    //register the newly found type with the kP system
                    if (kp != null) {
                        kp.EnsureType(mtype);
                    }
                }
            } else {
                id = new InstanceIdentifier(InstanceIndicator.NAME, name);
            }

            return id;
        }

        private Rule readRule(XPathNavigator nav) {
            ConsumerRule rule = null;

            XPathNavigator rhsNav = nav.SelectSingleNode("rhs");

            if (rhsNav != null) {
                Multiset m = readMultiset(rhsNav);
                XPathNodeIterator div;
                if (m.IsEmpty()) {
                    //then we can have structure changing rules
                    div = rhsNav.SelectChildren("instance", String.Empty);
                    if (div.Count > 0) {
                        DivisionRule r = new DivisionRule();
                        while (div.MoveNext()) {
                            string mtype = div.Current.GetAttribute("mtype", String.Empty);
                            InstanceBlueprint ib = null;
                            if (String.IsNullOrEmpty(mtype)) {
                                ib = new InstanceBlueprint(currentType);
                            } else {
                                ib = new InstanceBlueprint(kp[mtype]);
                            }
                            readMultiset(div.Current, ib.Multiset);
                            r.Rhs.Add(ib);
                        }
                        rule = r;
                    } else {
                        XPathNavigator v;
                        v = rhsNav.SelectSingleNode("linkCreate");
                        if (v == null) {
                            v = rhsNav.SelectSingleNode("linkDestroy");
                            if (v == null) {
                                v = rhsNav.SelectSingleNode("dissolve");
                                if (v != null) {
                                    rule = new DissolutionRule();
                                }
                            } else {
                                rule = LinkRule.LinkDestroy(readInstanceIdentifier(v));
                            }
                        } else {
                            rule = LinkRule.LinkCreate(readInstanceIdentifier(v));
                        }
                    }
                }
                
                if(rule == null) {
                    div = rhsNav.SelectChildren("target", String.Empty);
                    if (div.Count > 0) {
                        RewriteCommunicationRule rcr = new RewriteCommunicationRule();
                        rcr.Rhs.Add(m);
                        while (div.MoveNext()) {
                            TargetedMultiset tm = readTargetedMultiset(div.Current);
                            TargetedMultiset otm = null;
                            rcr.TargetRhs.TryGetValue(tm.Target, out otm);
                            if (otm == null) {
                                rcr.TargetRhs.Add(tm.Target, tm);
                            } else {
                                otm.Multiset.Add(tm.Multiset);
                            }
                        }
                        rule = rcr;
                    } else if(m != null) {
                        rule = new RewritingRule();
                        (rule as RewritingRule).Rhs.Add(m);
                    }
                }
            }

            if (rule == null) {
                rule = new ConsumerRule();
            }
            readPItem(nav, rule);
            rule.Guard = readGuard(nav.SelectSingleNode("guard"));
            readMultiset(nav.SelectSingleNode("lhs"), rule.Lhs);

            return rule;
        }

        private IGuard readGuard(XPathNavigator nav) {
            if (nav == null) {
                return null;
            }

            XPathNavigator root = nav.SelectSingleNode("eq | lt | gt | leq | geq | not | and | or");
            return readSelectedGuard(root);
        }

        private IGuard readSelectedGuard(XPathNavigator nav) {
            if (nav != null) {
                if (nav.Name == "eq") {
                    return new BasicGuard(readMultiset(nav, false), RelationalOperator.EQUAL);
                } else if (nav.Name == "lt") {
                    return new BasicGuard(readMultiset(nav, false), RelationalOperator.LT);
                } else if (nav.Name == "gt") {
                    return new BasicGuard(readMultiset(nav, false), RelationalOperator.GT);
                } else if (nav.Name == "leq") {
                    return new BasicGuard(readMultiset(nav, false), RelationalOperator.LEQ);
                } else if (nav.Name == "geq") {
                    return new BasicGuard(readMultiset(nav, false), RelationalOperator.GEQ);
                } else if (nav.Name == "not") {
                    return new NegatedGuard(readGuard(nav));
                } else if (nav.Name == "and") {
                    XPathNodeIterator it = nav.Select("eq | lt | gt | leq | geq | not | and | or");
                    if (it.MoveNext()) {
                        return readCompoundGuard(it, BinaryGuardOperator.AND);
                    } else {
                        return null;
                    }
                } else if (nav.Name == "or") {
                    XPathNodeIterator it = nav.Select("eq | lt | gt | leq | geq | not | and | or");
                    if (it.MoveNext()) {
                        return readCompoundGuard(it, BinaryGuardOperator.OR);
                    } else {
                        return null;
                    }
                }
            }

            return null;
        }

        private IGuard readCompoundGuard(XPathNodeIterator it, BinaryGuardOperator op) {
            IGuard g = readSelectedGuard(it.Current);
            if (it.MoveNext()) {
                return new CompoundGuard(op, g, readCompoundGuard(it, op));
            } else {
                return g;
            }
        }

        private ExecutionStrategyHelper readExecutionStrategy(XPathNavigator nav) {
            if (nav != null) {
                ExecutionStrategyHelper strategy = null;
                ExecutionStrategyHelper exs = null;
                XPathNodeIterator it = nav.Select("rule | choice | max | arb");
                string currentItem = null;

                while (it.MoveNext()) {
                    currentItem = it.Current.Name;
                    ExecutionStrategyHelper ex;
                    if (currentItem == "rule") {
                        if (exs == null) {
                            exs = new ExecutionStrategyHelper(StrategyOperator.SEQUENCE);
                        } else if (exs.Operator != StrategyOperator.SEQUENCE) {
                            ex = new ExecutionStrategyHelper(StrategyOperator.SEQUENCE);
                            exs.Next = ex;
                            exs = ex;
                        }
                        string x = it.Current.GetAttribute("name", String.Empty);
                        if (!String.IsNullOrEmpty(x)) {
                            exs.Rules.Add(x);
                        }
                    } else if (currentItem == "choice") {
                        ex = new ExecutionStrategyHelper(StrategyOperator.CHOICE);
                        if (exs == null) {
                            exs = ex;
                        } else {
                            exs.Next = ex;
                            exs = ex;
                        }
                        readRuleList(it.Current, exs.Rules);
                    } else if (currentItem == "max") {
                        ex = new ExecutionStrategyHelper(StrategyOperator.MAX);
                        if (exs == null) {
                            exs = ex;
                        } else {
                            exs.Next = ex;
                            exs = ex;
                        }
                        readRuleList(it.Current, exs.Rules);
                    } else if (currentItem == "arb") {
                        ex = new ExecutionStrategyHelper(StrategyOperator.ARBITRARY);
                        if (exs == null) {
                            exs = ex;
                        } else {
                            exs.Next = ex;
                            exs = ex;
                        }
                        readRuleList(it.Current, exs.Rules);
                    }

                    if (strategy == null) {
                        strategy = exs;
                    }
                }

                return strategy;
            }

            return new ExecutionStrategyHelper(StrategyOperator.SEQUENCE);
        }

        private List<string> readRuleList(XPathNavigator nav, List<string> rules) {
            if (rules == null) {
                rules = new List<string>();
            }
            XPathNodeIterator it = nav.SelectChildren("rule", String.Empty);
            while (it.MoveNext()) {
                string name = it.Current.GetAttribute("name", String.Empty);
                if (!String.IsNullOrEmpty(name)) {
                    rules.Add(name);
                }
            }

            return rules;
        }

        private class ExecutionStrategyHelper {
            private List<string> rules;
            public List<string> Rules { get { return rules; } }
            public StrategyOperator Operator { get; set; }

            private ExecutionStrategyHelper next;
            public ExecutionStrategyHelper Next { get { return next; } set { next = value; } }

            public ExecutionStrategyHelper() {
                rules = new List<string>();
            }

            public ExecutionStrategyHelper(StrategyOperator op)
                : this() {
                    Operator = op;
            }
        }
    }
}
