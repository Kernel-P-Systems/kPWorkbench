using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KpCore;

namespace KpFLAME
{
    public class FlameRulesGenerator
    {
        private ObjectsId objectsId;
        
        private MembraneId membraneId;
        public FlameRulesGenerator(ObjectsId objectsId, MembraneId membraneId)
        {
            this.objectsId = objectsId;
            this.membraneId = membraneId;
        }

        public string Multiset(Multiset m)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{{");
            bool b = false;
            foreach (string o in m.Objects)
            {
                if (b)
                    sb.Append(", ");
                b = true;
                sb.Append("{");
                sb.AppendFormat("{0}, {1}", objectsId[o], m[o]);
                //sb.AppendFormat("{0}, {1}", o, m[o]);
                sb.Append("}");
            }
            sb.Append("}}");
            return sb.ToString();
        }

        public string Rules(ExecutionStrategy es)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            bool b = false;
            while (es != null)
            {
                foreach (Rule r in es.Rules)
                {
                    if (b)
                        sb.Append(", ");
                    b = true;
                    sb.Append(Rule(r));
                }
                es = es.Next;
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string Rule(Rule rule)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append(Multiset(((ConsumerRule)rule).Lhs));
            if (rule is DivisionRule)
            {
                sb.Append(", 2, {");
                bool b = false;
                foreach (InstanceBlueprint rhs in ((DivisionRule)rule).Rhs)
                {
                    if (b)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("{");
                    sb.Append(Multiset(rhs.Multiset));
                    sb.Append(", {-1}");
                    sb.Append("}");
                    b = true;
                }
                sb.Append("}");
            }
            else
                if (rule is RewriteCommunicationRule)
                {
                    sb.Append(", 1, {");
                    bool b = false;
                    if (((RewriteCommunicationRule)rule).Rhs.Count > 0)
                    {
                        sb.Append("{");
                        sb.Append(Multiset(((RewriteCommunicationRule)rule).Rhs));
                        sb.Append(", {-1}");
                        sb.Append("}");
                        b = true;
                    }
                    foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in ((RewriteCommunicationRule)rule).TargetRhs)
                    {
                        if (b)
                            sb.Append(", ");
                        sb.Append("{");
                        sb.Append(Multiset(kv.Value.Multiset));
                        sb.Append(", {");
                        InstanceIdentifier i = kv.Key as InstanceIdentifier;
                        sb.AppendFormat("{0}", membraneId[i.Value]);
                        //sb.AppendFormat("{0}", i.Value);
                        sb.Append("}");
                        sb.Append("}");
                        b = true;
                    }
                    sb.Append("}");
                }
                else
                    if (rule is RewritingRule)
                    {
                        sb.Append(", 1, {");
                        sb.Append("{");
                        sb.Append(Multiset(((RewritingRule)rule).Rhs));
                        sb.Append(", {-1}");
                        sb.Append("}");
                        sb.Append("}");
                    }
                    else
                        if (rule is DissolutionRule)
                        {
                            sb.Append(", 3, {}");
                        }
                        else
                            if (rule is ConsumerRule)
                            {
                                sb.Append(", 0, {}");
                            }
            sb.Append("}");
            return sb.ToString();
        }

        public string Guards(ExecutionStrategy es)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            List<string> list = new List<string>();
            while (es != null)
            {
                foreach (Rule r in es.Rules)
                {
                    StringBuilder sb1 = new StringBuilder();
                    if (r.Guard != null)
                    {
                        sb1.Append("{{");
                        sb1.Append(Guard(r.Guard));
                        sb1.Append(", 0}");
                        sb1.Append("}");
                    }
                    else
                        sb1.Append("{{}}");
                    list.Add(sb1.ToString());
                }
                es = es.Next;
            }
            while (list.Count > 0 && list[list.Count - 1] == "")
                list.RemoveAt(list.Count - 1);
            bool b = false;
            foreach (string s in list)
            {
                if (b)
                    sb.Append(", ");
                b = true;
                sb.Append(s);
            }
            sb.Append("}");
            return sb.ToString();
        }

        private string Guard(IGuard guard, bool negated = false, int ooperation = 0)
        {
            StringBuilder sb = new StringBuilder();
            if (guard is BasicGuard)
            {
                BasicGuard basicGuard = (BasicGuard)guard;
                int rel = 0;
                switch (basicGuard.Operator)
                {
                    case RelationalOperator.LT: rel = 0; break;
                    case RelationalOperator.LEQ: rel = 1; break;
                    case RelationalOperator.EQUAL: rel = 2; break;
                    case RelationalOperator.NOT_EQUAL: rel = 3; break;
                    case RelationalOperator.GT: rel = 4; break;
                    case RelationalOperator.GEQ: rel = 5; break;
                }
                if (negated)
                    rel += 7;
                sb.Append("{");
                sb.AppendFormat("{0}, ", rel);
                Multiset multiset = basicGuard.Multiset;
                sb.Append("{");
                string o = multiset.Objects.ToList()[0];
                sb.Append(string.Format("{0}, {1}", objectsId[o], multiset[o]));
                sb.Append("}");
                sb.AppendFormat(", {0}", ooperation);
                sb.Append("}");
            }
            else
                if (guard is NegatedGuard)
                {
                    NegatedGuard negatedGuard = (NegatedGuard)guard;
                    sb.Append(Guard(negatedGuard.Operand, true));
                }
                else
                    if (guard is CompoundGuard)
                    {
                        CompoundGuard compoundGuard = (CompoundGuard)guard;
                        int op;
                        if (compoundGuard.Operator == BinaryGuardOperator.AND)
                            op = 0;
                        else
                            op = 1;
                        sb.Append(Guard(compoundGuard.Lhs, negated, op));
                        sb.Append(", ");
                        sb.Append(Guard(compoundGuard.Rhs, negated, ooperation));
                    }
            return sb.ToString();
        }
    }


    public class FlameCodeRulesGenerator
    {
        private ObjectsId objectsId;

        private MembraneId membraneId;
        public FlameCodeRulesGenerator(ObjectsId objectsId, MembraneId membraneId)
        {
            this.objectsId = objectsId;
            this.membraneId = membraneId;
        }

        public List<int> Multiset(Multiset m)
        {
            List<int> list = new List<int>();
            foreach (string o in m.Objects)
            {
                list.Add(objectsId[o]);
                list.Add(m[o]);
            }
            list.Insert(0, list.Count / 2);
            return list;
        }

        public List<int> Rules(ExecutionStrategy es)
        {
            List<int> list = new List<int>();
            List<int> pos = new List<int>();
            if (es != null)
            {
                foreach (Rule r in es.Rules)
                {
                    pos.Add(list.Count);
                    list.AddRange(Rule(r));
                }
                int n = es.Rules.Count;
                pos = pos.Select(a => a + n + 1).ToList();
                pos.Insert(0, n);
                list.InsertRange(0, pos);
            }
            return list;
        }

        private List<int> Rule(Rule rule)
        {
            List<int> list = new List<int>();
            list.AddRange(Multiset(((ConsumerRule)rule).Lhs));
            if (rule is DivisionRule)
            {
                list.Add(2);
                list.Add(((DivisionRule)rule).Rhs.Count);
                foreach (InstanceBlueprint rhs in ((DivisionRule)rule).Rhs)
                {
                    list.AddRange(Multiset(rhs.Multiset));
                }
            }
            else
                if (rule is RewriteCommunicationRule)
                {
                    list.Add(1);
                    list.Add(((RewriteCommunicationRule)rule).Rhs.Count + ((RewriteCommunicationRule)rule).TargetRhs.Count);
                    bool b = false;
                    if (((RewriteCommunicationRule)rule).Rhs.Count > 0)
                    {
                        list.AddRange((Multiset(((RewriteCommunicationRule)rule).Rhs)));
                        list.Add(-1);
                    }
                    foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in ((RewriteCommunicationRule)rule).TargetRhs)
                    {
                        list.AddRange((Multiset(kv.Value.Multiset)));
                        InstanceIdentifier i = kv.Key as InstanceIdentifier;
                        list.Add(membraneId[i.Value]);
                    }
                }
                else
                    if (rule is RewritingRule)
                    {
                        list.Add(0);
                        list.AddRange(Multiset(((RewritingRule)rule).Rhs));
                    }
                    else
                        if (rule is DissolutionRule)
                        {
                            list.Add(3);
                        }
                        else
                            if (rule is ConsumerRule)
                            {
                                list.Add(0);
                            }
            return list;
        }

        public List<int> Guards(ExecutionStrategy es)
        {
            List<int> list = new List<int>();
            List<int> pos = new List<int>();
            foreach (Rule r in es.Rules)
            {
                if (r.Guard != null)
                {
                    pos.Add(list.Count);
                    List<int> g = new List<int>();
                    Guard(g, r.Guard);
                    list.AddRange(g);
                    list.Add(-1);
                }
                else
                    pos.Add(-1);
            }
            int n = es.Rules.Count;
            pos = pos.Select(a => a > -1 ? a + n + 1:0).ToList();
            pos.Insert(0, n);
            list.InsertRange(0, pos);
            return list;
        }

        private void Guard(List<int> list, IGuard guard, bool negated = false)
        {
            if (guard is BasicGuard)
            {
                BasicGuard basicGuard = (BasicGuard)guard;
                int rel = 0;
                switch (basicGuard.Operator)
                {
                    case RelationalOperator.LT: rel = 0; break;
                    case RelationalOperator.LEQ: rel = 1; break;
                    case RelationalOperator.EQUAL: rel = 2; break;
                    case RelationalOperator.NOT_EQUAL: rel = 3; break;
                    case RelationalOperator.GT: rel = 4; break;
                    case RelationalOperator.GEQ: rel = 5; break;
                }
                if (negated)
                    rel += 8;
                list.Add(rel);
                Multiset multiset = basicGuard.Multiset;
                string o = multiset.Objects.ToList()[0];
                list.Add(objectsId[o]);
                list.Add(multiset[o]);
            }
            else
                if (guard is NegatedGuard)
                {
                    NegatedGuard negatedGuard = (NegatedGuard)guard;
                    Guard(list, negatedGuard.Operand, true);
                }
                else
                    if (guard is CompoundGuard)
                    {
                        CompoundGuard compoundGuard = (CompoundGuard)guard;
                        int op;
                        if (compoundGuard.Operator == BinaryGuardOperator.AND)
                            op = 0;
                        else
                            op = 1;
                        Guard(list, compoundGuard.Lhs, negated);
                        list.Add(op);
                        Guard(list, compoundGuard.Rhs, negated);
                    }
        }
    }
}
