using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using KpExperiment.Verification.Translation;
using KpExperiment.Verification.Translation.Base;
using KpUtil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KpSpin.SpinVerificationModel
{
    public class VerificationModelWriter
    {
        public const int INSTANCE_LIMIT = 1024;

        private TextWriter owt;
        public TextWriter Channel { get { return owt; } set { owt = value; } }
        public VerificationModelParams Params { get; set; }

        private KPsystem kp;
        private KpModel kpModel;
        private KpModel KpModel { get { return kpModel; } set { kpModel = value; kp = value.KPsystem; kpm = new KpMeta(kp); } }
        private KpMeta kpm;
        private string nl = System.Environment.NewLine;

        private int maxLinks;
        private int maxCompartments;

        private int indent = 0;

        public VerificationModelWriter(TextWriter channel)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }
            Channel = channel;
        }

        public VerificationModelWriter(TextWriter channel, VerificationModelParams param)
            : this(channel)
        {
            Params = param;
        }

        public void Write(KpModel kpModel, Experiment experiment = null)
        {
            KpModel = kpModel;

            maxCompartments = Params.MaxCompartments;
            if (Params.LinksEnabled)
            {
                maxLinks = Params.MaxLinks;
            }
            else
            {
                maxLinks = 0;
            }

            if (kpm.HasDivision)
            {
                if (maxCompartments < 0)
                {
                    maxCompartments = INSTANCE_LIMIT;
                }
                if (Params.LinksEnabled)
                {
                    if (maxLinks < 0 || maxLinks > maxCompartments - 1)
                    {
                        maxLinks = maxCompartments - 1;
                    }
                }
            }
            else
            {
                if (maxCompartments < 0 || maxCompartments > kpm.InstanceCount)
                {
                    maxCompartments = kpm.InstanceCount;
                }
                if (Params.LinksEnabled)
                {
                    if (maxLinks < 0 || maxLinks > kpm.AllInstance.Count - 1)
                    {
                        maxLinks = kpm.AllInstance.Count - 1;
                    }
                }
            }

            WriteConstants();
            owt.WriteLine();
            WriteSymbolAliases();
            owt.WriteLine();
            WriteCompartmentTypedef();
            owt.WriteLine();
            WriteGlobalVars();
            owt.WriteLine();

            if (experiment != null)
            {
                WriteProperties(experiment.LtlProperties);
            }

            if (Params.O1 || kpm.HasCommunication || kpm.HasStructureChangingRules)
            {
                WriteLine("c_code {");
                indent++;

                if (kpm.HasCommunication || kpm.HasLinkDestruction)
                {
                    WriteConnectedTargetSelect();
                    WriteLine();
                }

                if (kpm.HasLinkCreation)
                {
                    WriteLinkedFunction();
                    WriteLine();
                    WriteDisconnectedTargetSelect();
                    WriteLine();
                }

                if (kpm.HasLinkCreation || kpm.HasDivision)
                {
                    WriteLinkAddFunction();
                    WriteLine();
                    WriteBidirectionalLinkFunction();
                    WriteLine();
                }

                if (kpm.HasLinkDestruction || kpm.HasDivision || kpm.HasDissolution)
                {
                    WriteRemoveLinkFunction();
                    WriteLine();
                }

                if (kpm.HasDivision)
                {
                    WriteInvalidateLinksFunction();
                    WriteLine();
                    WriteCopyLinksFunction();
                    WriteLine();
                }

                if (Params.O1)
                {
                    WriteGetApplicabilityRateFunction();
                    WriteLine();
                }

                indent--;
                WriteLine("};");
                WriteLine();
            }


            //declare KP process
            WriteLine("active proctype KP() {");
            indent++;

            //intialiazation at step 0;
            WriteLine("atomic {");
            indent++;
            foreach (MType mtype in kp.Types)
            {
                foreach (MInstance instance in mtype.Instances)
                {
                    WriteLine("c[{0}].type = {1};", instance.Id, mtype.Id);
                    foreach (KeyValuePair<string, int> msi in instance.Multiset)
                    {
                        WriteLine("c[{0}].x[{1}_] = {2};", instance.Id, msi.Key, msi.Value);
                    }
                }
                WriteLine("cTCount[{0}] = {1};", mtype.Id, mtype.Instances.Count);
            }
            WriteLine("cCount = {0};", kpm.AllInstance.Count);

            if (maxLinks > 0)
            {
                int[] linkCountPerType = new int[kp.Types.Count];
                foreach (MInstance instance in kpm.AllInstance)
                {
                    int j = 0;
                    foreach (MInstance link in instance.Connections)
                    {
                        WriteLine("c[{0}].links[{1}] = {2};", instance.Id, j++, link.Id);
                        linkCountPerType[kpm.GetInstanceType(link).Id]++;
                    }
                    WriteLine("c[{0}].lCount = {1};", instance.Id, instance.Connections.Count);
                    for (int i = 0; i < linkCountPerType.Length; i++)
                    {
                        if (linkCountPerType[i] > 0)
                        {
                            WriteLine("c[{0}].lTCount[{1}] = {2};", instance.Id, i, linkCountPerType[i]);
                        }
                        linkCountPerType[i] = 0;
                    }
                }
            }

            if (kpm.HasDivision)
            {
                //the start index where new compartments are added as result of a division
                WriteLine("nCStart = cCount;");
            }

            WriteLine();
            WriteLine("step = 0;");
            indent--;
            //end init atomic
            WriteLine("}");

            WriteLine("state = initial;");

            WriteLine("assert(cCount > 0);");
            WriteLine("int i;");
            if (kpm.HasCommunication || kpm.HasLinkRules)
            {
                WriteLine("int ntar = 0;");
            }

            WriteLine();
            WriteLine("do :: step < MAX_STEPS && !deadlock ->");
            indent++;
            WriteLine("atomic {");
            WriteLine("state = running;");
            WriteLine("rulesAppliedThisStep = 0;");
            WriteLine("int cCountA = cCount - 1;");
            WriteLine("for(i: 0 .. cCountA) {");

            if (kpm.HasDivision || kpm.HasDissolution)
            {
                owt.Write("\t\t\tif :: ");
                if (kpm.HasDissolution)
                {
                    owt.Write("c[i].isDissolved");
                    if (kpm.HasDivision)
                    {
                        owt.Write(" || c[i].isDivided");
                    }
                }
                else
                {
                    if (kpm.HasDivision)
                    {
                        owt.Write("c[i].isDivided");
                    }
                }
                owt.WriteLine(" -> cCountA++; goto ExEnd;");
                owt.WriteLine("\t\t\t   :: else ->");
            }

            indent = 3;
            if (kpm.KPsystem.Types.Count > 0)
            {
                WriteLine("if");
                indent++;
                foreach (MType mtype in kp.Types)
                {
                    if (!mtype.ExecutionStrategy.IsEmpty())
                    {
                        WriteLine("/* TYPE {0} */", mtype.Name);
                        WriteLine(":: c[i].type == {0} ->", mtype.Id);
                        indent++;

                        ExecutionStrategy exs = mtype.ExecutionStrategy;
                        while (exs != null)
                        {
                            if (exs.Operator == StrategyOperator.CHOICE)
                            {
                                foreach (Rule r in exs.Rules)
                                {
                                    if (r.IsGuarded)
                                    {
                                        WriteLine("bit g{0}{1} = {2};", mtype.Id, r.Id, guardTranslate(r.Guard));
                                    }
                                }
                                WriteLine("if");
                                indent++;
                                foreach (Rule r in exs.Rules)
                                {
                                    WriteRule(r, mtype);
                                }
                                WriteLine(":: else -> skip;");
                                indent--;
                                WriteLine("fi;");
                                WriteLine();
                            }
                            else if (exs.Operator == StrategyOperator.ARBITRARY || exs.Operator == StrategyOperator.MAX)
                            {
                                foreach (Rule r in exs.Rules)
                                {
                                    if (r.IsGuarded)
                                    {
                                        WriteLine("bit g{0}{1} = {2};", mtype.Id, r.Id, guardTranslate(r.Guard));
                                    }
                                }

                                WriteLine("do");
                                indent++;
                                foreach (Rule r in exs.Rules)
                                {
                                    if (r.Type == RuleType.MULTISET_REWRITING)
                                    {
                                        if (Params.O1 && r.IsIndependentInGroup(exs.Rules))
                                        {
                                            //if O1 is enabled then apply optimization of rewriting rules in a max block
                                            RewritingRule rule = r as RewritingRule;
                                            Write(":: ");
                                            indent++;
                                            if (r.IsGuarded)
                                            {
                                                owt.WriteLine("g{0}{1} && ", mtype.Id, r.Id);
                                                WriteIndent();
                                            }

                                            int i = 1;
                                            int count = rule.Lhs.Count;
                                            foreach (KeyValuePair<string, int> kv in rule.Lhs)
                                            {
                                                owt.Write("c[i].x[{0}] >= {1}", promelaSymbol(kv.Key), kv.Value);
                                                if (i++ < count)
                                                {
                                                    owt.Write(" &&");
                                                    WriteLine();
                                                    WriteIndent();
                                                }
                                            }
                                            owt.WriteLine();
                                            WriteLine(" ->");
                                            WriteLine("c_code {");
                                            indent++;
                                            string rate = String.Format("rate{0}{1}", mtype.Id, r.Id);
                                            WriteLine("int i = PKP->i;");
                                            Write("int {0} = getApplicabilityRate(i, (int []){{", rate);
                                            i = 1;
                                            foreach (KeyValuePair<string, int> kv in rule.Lhs)
                                            {
                                                owt.Write("{0}, {1}", promelaSymbol(kv.Key), kv.Value);
                                                if (i++ < count)
                                                {
                                                    owt.Write(", ");
                                                }
                                            }
                                            owt.WriteLine("}}, {0});", 2 * count);

                                            owt.Write(subtractLhsWithRate(rule.Lhs, rate));
                                            owt.Write(addRhsWithRate(rule.Rhs, rate));
                                            indent--;
                                            WriteLine("};");
                                            WriteLine("rulesAppliedThisStep++;");
                                            indent--;
                                        }
                                        else
                                        {
                                            WriteRule(r, mtype);
                                        }
                                    }
                                    else
                                    {
                                        WriteRule(r, mtype);
                                    }
                                }
                                if (exs.Operator == StrategyOperator.MAX)
                                {
                                    WriteLine(":: else -> break;");
                                }
                                else
                                {
                                    WriteLine(":: break;");
                                }
                                indent--;
                                WriteLine("od;");
                                WriteLine();
                            }
                            else if (exs.Operator == StrategyOperator.SEQUENCE)
                            {
                                foreach (Rule r in exs.Rules)
                                {
                                    if (r.IsGuarded)
                                    {
                                        WriteLine("bit g{0}{1} = {2};", mtype.Id, r.Id, guardTranslate(r.Guard));
                                    }
                                    WriteLine("if ");
                                    indent++;
                                    WriteRule(r, mtype);
                                    WriteLine(":: else -> goto ExEnd;", mtype.Id);
                                    indent--;
                                    WriteLine("fi;");
                                    WriteLine();
                                }
                            }
                            exs = exs.Next;
                        }
                        indent--;
                    }
                }

                WriteLine(":: else -> skip;");
                indent--;
                WriteLine("fi;");
            }
            if (kpm.HasDivision || kpm.HasDissolution)
            {
                WriteLine("fi;");
            }
            WriteLine("ExEnd: skip;");
            indent--;
            //end for
            WriteLine("}");

            WriteLine();
            WriteLine("state = rules_applied;");

            //commit phase
            WriteLine();
            WriteLine("c_code {");
            indent++;
            WriteLine("int i, j;");
            WriteLine("int cCount = now.cCount;");
            WriteLine("for(i = 0; i < cCount; i++) {");
            indent++;
            if (kpm.HasDissolution || kpm.HasDivision)
            {
                WriteLine();
                Write("if(");
                if (kpm.HasDivision)
                {
                    owt.Write("now.c[i].isDivided");
                    if (kpm.HasDissolution)
                    {
                        owt.Write(" || now.c[i].isDissolved");
                    }
                }
                else if (kpm.HasDissolution)
                {
                    owt.Write("now.c[i].isDissolved");
                }
                owt.WriteLine(") {");
                indent++;
                WriteLine("cCount++;");
                indent--;
                WriteLine("} else {");
                indent++;
            }
            WriteLine("for(j = 0; j < A_SIZE; j++) {");
            indent++;
            WriteLine("now.c[i].x[j] += now.c[i].xt[j];");
            WriteLine("now.c[i].xt[j] = 0;");
            indent--;
            //end inner for
            WriteLine("}");

            if (kpm.HasLinkCreation)
            {
                WriteLine();
                WriteLine("if(now.c[i].createLinkTo > -1) {");
                indent++;
                WriteLine("int cli = now.c[i].createLinkTo;");
                Write("if(!(");
                if (kpm.HasDissolution)
                {
                    owt.Write("now.c[cli].isDissolved || now.c[cli].flagDissolved || ");
                }
                owt.WriteLine("now.c[cli].createLinkTo == i)) {");
                indent++;
                WriteLine("connectBidirectional(i, cli);");
                indent--;
                WriteLine("}");
                WriteLine("now.c[i].createLinkTo = -1;");
                indent--;
                WriteLine("}");
            }

            if (kpm.HasLinkDestruction)
            {
                WriteLine();
                WriteLine("if(now.c[i].destroyLinkTo > -1) {");
                indent++;
                WriteLine("int dli = now.c[i].destroyLinkTo;");
                Write("if(!(");
                if (kpm.HasDissolution)
                {
                    //if the compartment we are connecting to has dissolved or its dissolution is pending
                    //then skip the action since the dissolution itself took/will take care of the link removal
                    owt.Write("now.c[dli].isDissolved || now.c[dli].flagDissolved || ");
                }
                //don't attempt to destroy the same link twice, if both compartments attempt at the same link
                //then skip the first action and do the other
                owt.WriteLine("now.c[dli].destroyLinkTo == i)) {");
                indent++;
                WriteLine("removeLink(i, now.c[i].destroyLinkTo);");
                WriteLine("removeLink(now.c[i].destroyLinkTo, i);");
                indent--;
                WriteLine("}");
                WriteLine("now.c[i].destroyLinkTo = -1;");
                indent--;
                WriteLine("}");
            }

            if (kpm.HasDissolution)
            {
                WriteLine();
                WriteLine("if(now.c[i].flagDissolved) {");
                indent++;
                WriteLine("int lCount = now.c[i].lCount;");
                WriteLine("for(j = 0; j < lCount; j++) {");
                indent++;
                WriteLine("int link = now.c[i].links[j];");
                WriteLine("if(link > -1) {");
                indent++;
                WriteLine("removeLink(link, i);");
                WriteLine("now.c[i].links[j] = -1;");
                indent--;
                WriteLine("} else {");
                indent++;
                WriteLine("lCount++;");
                indent--;
                WriteLine("}");
                indent--;
                WriteLine("}");
                WriteLine("now.c[i].lCount = 0;");
                WriteLine("for(j = 0; j < {0}; j++) {{", kp.Types.Count);
                indent++;
                WriteLine("now.c[i].lTCount[j] = 0;");
                indent--;
                WriteLine("}");
                WriteLine("now.c[i].isDissolved = 1;");
                WriteLine("now.c[i].flagDissolved = 0;");
                WriteLine("now.cCount--;");
                WriteLine("now.cTCount[now.c[i].type]--;");
                indent--;
                WriteLine("}");
            }
            if (kpm.HasDivision)
            {
                WriteLine();
                WriteLine("if(now.c[i].flagDivided) {");
                indent++;
                WriteLine("now.c[i].isDivided = 1;");
                WriteLine("now.c[i].flagDivided = 0;");
                WriteLine("now.cCount--;");
                WriteLine("now.cTCount[now.c[i].type]--;");
                indent--;
                WriteLine("}");
            }

            if (kpm.HasStructureChangingRules)
            {
                WriteLine("now.c[i].structureChanged = 0;");
            }

            if (kpm.HasDissolution || kpm.HasDivision)
            {
                indent--;
                //end if
                WriteLine("}");
            }
            indent--;
            //end for
            WriteLine("}");

            if (kpm.HasDivision)
            {
                WriteLine();
                WriteLine("int nCCount = now.nCCount;");
                WriteLine("int nCStart = now.nCStart;");
                WriteLine("for(i = 1; i <= nCCount; ++i) {");
                indent++;
                WriteLine("int k = nCStart - i;");
                WriteLine("for(j = 0; j < A_SIZE; j++) {");
                indent++;
                WriteLine("now.c[k].x[j] += now.c[now.c[k].inheritsFrom].x[j];");
                indent--;
                WriteLine("}");
                WriteLine("copyLinks(now.c[k].inheritsFrom, k);");
                WriteLine("now.cTCount[now.c[k].type]++;");
                indent--;
                WriteLine("}");
                WriteLine("for(i = 1; i <= nCCount; ++i) {");
                indent++;
                WriteLine("int k = nCStart - i;");
                WriteLine("if(now.c[now.c[k].inheritsFrom].lCount > 0) {");
                indent++;
                WriteLine("invalidateLinks(now.c[k].inheritsFrom);");
                indent--;
                WriteLine("}");
                indent--;
                WriteLine("}");
                WriteLine("now.cCount += nCCount;");
                WriteLine("now.nCCount = 0;");
            }

            WriteLine("if(now.rulesAppliedThisStep == 0) {");
            indent++;
            WriteLine("now.deadlock = 1;");
            indent--;
            WriteLine("} else {");
            indent++;
            WriteLine("now.step++;");
            indent--;
            WriteLine("}");

            indent--;
            //end c_code
            WriteLine("};");
            WriteLine("state = step_complete;");

            indent--;

            //end atomic
            WriteLine("}");
            WriteLine(":: else -> break;");
            //end big do
            WriteLine("od;");

            WriteLine("state = finished;");

            indent--;
            //end active proctype 
            WriteLine("}");
            owt.Flush();
            owt.Close();
        }

        private string guardTranslate(IGuard guard)
        {
            StringBuilder buf = new StringBuilder();
            if (guard is BasicGuard)
            {
                BasicGuard g = guard as BasicGuard;
                foreach (KeyValuePair<string, int> kv in g.Multiset)
                {
                    buf.AppendFormat("c[i].x[{0}] {1} {2} && ", promelaSymbol(kv.Key), guardOperator(g), kv.Value);
                }
                //remove the final " &&"
                buf.Remove(buf.Length - 4, 4);
            }
            else if (guard is NegatedGuard)
            {
                buf.AppendFormat("!({0})", guardTranslate((guard as NegatedGuard).Operand));
            }
            else if (guard is CompoundGuard)
            {
                CompoundGuard g = guard as CompoundGuard;

                bool useP = false;
                if (g.Lhs is CompoundGuard)
                {
                    CompoundGuard cgLhs = g.Lhs as CompoundGuard;
                    if (cgLhs.Operator != g.Operator)
                    {
                        useP = true;
                    }
                }
                else if (g.Lhs is BasicGuard && g.Operator == BinaryGuardOperator.OR)
                {
                    if ((g.Lhs as BasicGuard).Multiset.Count > 1)
                    {
                        useP = true;
                    }
                }

                if (useP)
                {
                    buf.Append("(").Append(guardTranslate(g.Lhs)).Append(")");
                }
                else
                {
                    buf.Append(guardTranslate(g.Lhs));
                }

                if (g.Operator == BinaryGuardOperator.AND)
                {
                    buf.Append(" && ");
                }
                else
                {
                    buf.Append(" || ");
                }

                useP = false;
                if (g.Rhs is CompoundGuard)
                {
                    CompoundGuard cgRhs = g.Rhs as CompoundGuard;
                    if (cgRhs.Operator != g.Operator)
                    {
                        useP = true;
                    }
                }
                else if (g.Rhs is BasicGuard && g.Operator == BinaryGuardOperator.OR)
                {
                    if ((g.Rhs as BasicGuard).Multiset.Count > 1)
                    {
                        useP = true;
                    }
                }

                if (useP)
                {
                    buf.Append("(").Append(guardTranslate(g.Rhs)).Append(")");
                }
                else
                {
                    buf.Append(guardTranslate(g.Rhs));
                }
            }

            return buf.ToString();
        }

        private string guardOperator(BasicGuard bg)
        {
            switch (bg.Operator)
            {
                case KpCore.RelationalOperator.EQUAL: return "==";
                case KpCore.RelationalOperator.NOT_EQUAL: return "!=";
                case KpCore.RelationalOperator.LT: return "<";
                case KpCore.RelationalOperator.LEQ: return "<=";
                case KpCore.RelationalOperator.GEQ: return ">=";
                case KpCore.RelationalOperator.GT: return ">";
            }

            return "";
        }

        private void WriteRule(Rule r, MType mtype)
        {

            //conditions which follow rules even in repetitive blocks
            //1. lhs must exist in instance multiset
            //2. if rule is guarded, then the guard bool var must hold
            //3. structure changing rule => structure changed?
            //4.  communication rule => foreach communicative tuple, 
            //make sure there exists a link to a compartment of the specified type
            //5. link rule => make sure there exists a link to be destroyed or a compartment that 
            //the instance is not connected to

            Write(":: ");
            indent++;
            if (r.IsGuarded)
            {
                owt.WriteLine("g{0}{1} &&", mtype.Id, r.Id);
                WriteIndent();
            }
            if (r.IsStructureChangingRule())
            {
                owt.WriteLine("!c[i].structureChanged &&");
                WriteIndent();
            }

            int i = 1;
            int count = (r as ConsumerRule).Lhs.Count;
            foreach (KeyValuePair<string, int> kv in (r as ConsumerRule).Lhs)
            {
                owt.Write("c[i].x[{0}] >= {1}", promelaSymbol(kv.Key), kv.Value);
                if (i++ < count)
                {
                    owt.Write(" &&");
                    WriteLine();
                    WriteIndent();
                }
            }
            bool moreConditions = r.Type == RuleType.REWRITE_COMMUNICATION ||
                r.Type == RuleType.LINK_CREATION || r.Type == RuleType.LINK_DESTRUCTION;
            if (moreConditions)
            {
                owt.Write(" &&");
            }
            WriteLine();

            if (r.Type == RuleType.REWRITE_COMMUNICATION)
            {
                RewriteCommunicationRule rcr = r as RewriteCommunicationRule;
                if (rcr.TargetRhs.Count > 0)
                {
                    i = 1;
                    count = rcr.TargetRhs.Count;
                    foreach (TargetedMultiset tm in rcr.TargetRhs.Values)
                    {
                        if (tm.Target is InstanceIdentifier)
                        {
                            if ((tm.Target as InstanceIdentifier).Indicator == InstanceIndicator.TYPE)
                            {
                                MType targetType = kp[(tm.Target as InstanceIdentifier).Value];
                                Write("c[i].lTCount[{0}] > 0", targetType.Id);
                                if (i++ < count)
                                {
                                    owt.Write(" &&");
                                }
                                WriteLine();
                            }
                        }
                    }
                }
            }
            else if (r.Type == RuleType.LINK_DESTRUCTION)
            {
                LinkRule lr = r as LinkRule;
                if (lr.Target is InstanceIdentifier)
                {
                    if ((lr.Target as InstanceIdentifier).Indicator == InstanceIndicator.TYPE)
                    {
                        MType targetType = kp[(lr.Target as InstanceIdentifier).Value];
                        WriteLine("c[i].lTCount[{0}] > 0", targetType.Id);
                    }
                }
            }
            else if (r.Type == RuleType.LINK_CREATION)
            {
                LinkRule lr = r as LinkRule;
                if (lr.Target is InstanceIdentifier)
                {
                    if ((lr.Target as InstanceIdentifier).Indicator == InstanceIndicator.TYPE)
                    {
                        MType targetType = kp[(lr.Target as InstanceIdentifier).Value];
                        //must exclude self when detecting the number of available compartments of a type that is not linked to
                        if (targetType == mtype)
                        {
                            WriteLine("c[i].lTCount[{0}] < cTCount[{0}] - 1", targetType.Id);
                        }
                        else
                        {
                            WriteLine("c[i].lTCount[{0}] < cTCount[{0}]", targetType.Id);
                        }
                    }
                }
            }

            WriteLine(" ->");
            Multiset lhs = (r as ConsumerRule).Lhs;
            owt.Write(subtractLhs(lhs));

            switch (r.Type)
            {
                case RuleType.MULTISET_REWRITING:
                    {
                        owt.Write(addRhs((r as RewritingRule).Rhs));
                        WriteLine("rulesAppliedThisStep++;");
                    } break;
                case RuleType.REWRITE_COMMUNICATION:
                    {
                        RewriteCommunicationRule rcr = r as RewriteCommunicationRule;
                        if (!rcr.Rhs.IsEmpty())
                        {
                            owt.WriteLine(addRhs(rcr.Rhs));
                        }
                        foreach (TargetedMultiset tm in rcr.TargetRhs.Values)
                        {
                            if (tm.Target is InstanceIdentifier)
                            {
                                if ((tm.Target as InstanceIdentifier).Indicator == InstanceIndicator.TYPE)
                                {
                                    MType targetType = kp[(tm.Target as InstanceIdentifier).Value];
                                    WriteLine("select(ntar: 0 .. c[i].lTCount[{0}] - 1);", targetType.Id);
                                    WriteLine("c_code {");
                                    indent++;
                                    WriteLine("int j;");
                                    WriteLine("j = selectTarget(PKP->ntar, PKP->i, {0});", targetType.Id);
                                    foreach (KeyValuePair<string, int> kv in tm.Multiset)
                                    {
                                        WriteLine("now.c[j].xt[{0}] += {1};",
                                             promelaSymbol(kv.Key), kv.Value);
                                    }
                                    indent--;
                                    WriteLine("};");
                                }
                            }
                        }
                        WriteLine("rulesAppliedThisStep++;");
                    } break;
                case RuleType.MEMBRANE_DISSOLUTION:
                    {
                        WriteLine("c[i].flagDissolved = 1;");
                        WriteLine("c[i].structureChanged = 1;");
                        WriteLine("rulesAppliedThisStep++;");
                        WriteLine("goto ExEnd;");
                    } break;
                case RuleType.MEMBRANE_DIVISION:
                    {
                        DivisionRule dr = r as DivisionRule;
                        foreach (InstanceBlueprint ib in dr.Rhs)
                        {
                            WriteLine("c[nCStart].type = {0};", ib.Type.Id);
                            foreach (KeyValuePair<string, int> kv in ib.Multiset)
                            {
                                WriteLine("c[nCStart].x[{0}] = {1};", promelaSymbol(kv.Key), kv.Value);
                            }
                            WriteLine("c[nCStart].inheritsFrom = i;");
                            WriteLine("nCStart++;");
                            WriteLine("nCCount++;");
                        }

                        WriteLine("c[i].flagDivided = 1;");
                        WriteLine("c[i].structureChanged = 1;");
                        WriteLine("rulesAppliedThisStep++;");
                        WriteLine("goto ExEnd;");
                    } break;
                case RuleType.LINK_CREATION:
                    {
                        LinkRule lr = r as LinkRule;
                        if (lr.Target is InstanceIdentifier)
                        {
                            if ((lr.Target as InstanceIdentifier).Indicator == InstanceIndicator.TYPE)
                            {
                                MType targetType = kp[(lr.Target as InstanceIdentifier).Value];
                                if (targetType == mtype)
                                {
                                    //if the target type is the same type as this, then exclude the compartment from the list 
                                    //of existing link-free membranes
                                    //in other word, a compartment can have at most n-1 links to compartments of the same type,
                                    //where n is the number of compartments of that type.
                                    WriteLine("select(ntar: 0 .. (cTCount[{0}] - c[i].lTCount[{0}] - 2));", targetType.Id);
                                }
                                else
                                {
                                    WriteLine("select(ntar: 0 .. (cTCount[{0}] - c[i].lTCount[{0}] - 1));", targetType.Id);
                                }
                                WriteLine("c_code {");
                                indent++;
                                WriteLine("int j;");
                                WriteLine("j = selectLinkFreeTarget(PKP->ntar, PKP->i, {0});", targetType.Id);
                                WriteLine("now.c[PKP->i].createLinkTo = j;");
                                indent--;
                                WriteLine("};");
                                WriteLine("rulesAppliedThisStep++;");
                                WriteLine("c[i].structureChanged = 1;");
                            }
                        }
                    } break;
                case RuleType.LINK_DESTRUCTION:
                    {
                        LinkRule lr = r as LinkRule;
                        if (lr.Target is InstanceIdentifier)
                        {
                            if ((lr.Target as InstanceIdentifier).Indicator == InstanceIndicator.TYPE)
                            {
                                MType targetType = kp[(lr.Target as InstanceIdentifier).Value];
                                WriteLine("select(ntar: 0 .. c[i].lTCount[{0}] - 1);", targetType.Id);
                                WriteLine("c_code {");
                                indent++;
                                WriteLine("int j;");
                                WriteLine("j = selectTarget(PKP->ntar, PKP->i, {0});", targetType.Id);
                                WriteLine("now.c[PKP->i].destroyLinkTo = j;");
                                indent--;
                                WriteLine("};");
                                WriteLine("rulesAppliedThisStep++;");
                                WriteLine("c[i].structureChanged = 1;");
                            }
                        }
                    } break;
                default:
                    {
                        owt.WriteLine("skip;");
                    } break;
            }
            indent--;
        }

        private void appendIndentation(StringBuilder buf)
        {
            for (int i = 0; i < indent; i++)
            {
                buf.Append("\t");
            }
        }

        private string getIndent()
        {
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                buf.Append("\t");
            }

            return buf.ToString();
        }

        private string subtractLhs(Multiset ms)
        {
            StringBuilder buf = new StringBuilder();
            string ind = getIndent();
            foreach (KeyValuePair<string, int> kv in ms)
            {
                buf.Append(ind)
                    .AppendFormat("c[i].x[{0}] = c[i].x[{0}] - {1}; ", promelaSymbol(kv.Key), kv.Value)
                    .AppendLine();
            }
            return buf.ToString();
        }

        private string addRhs(Multiset ms)
        {
            StringBuilder buf = new StringBuilder();
            string ind = getIndent();
            foreach (KeyValuePair<string, int> kv in ms)
            {
                buf.Append(ind)
                    .AppendFormat("c[i].xt[{0}] = c[i].xt[{0}] + {1};", promelaSymbol(kv.Key), kv.Value)
                    .AppendLine();
            }

            return buf.ToString();
        }

        private string subtractLhsWithRate(Multiset ms, string rate)
        {
            StringBuilder buf = new StringBuilder();
            string ind = getIndent();
            foreach (KeyValuePair<string, int> kv in ms)
            {
                buf.Append(ind)
                    .AppendFormat("now.c[i].x[{0}] = now.c[i].x[{0}] - {1} * {2}; ", promelaSymbol(kv.Key), kv.Value, rate)
                    .AppendLine();
            }
            return buf.ToString();
        }

        private string addRhsWithRate(Multiset ms, string rate)
        {
            StringBuilder buf = new StringBuilder();
            string ind = getIndent();
            foreach (KeyValuePair<string, int> kv in ms)
            {
                buf.Append(ind)
                    .AppendFormat("now.c[i].xt[{0}] = now.c[i].xt[{0}] + {1} * {2};", promelaSymbol(kv.Key), kv.Value, rate)
                    .AppendLine();
            }

            return buf.ToString();
        }

        private void WriteConstants()
        {
            owt.WriteLine("#define MAX_STEPS " + Params.Steps);
            owt.WriteLine("#define A_SIZE " + kpm.Alphabet.Count);
            owt.WriteLine("#define C_SIZE " + maxCompartments);

            owt.WriteLine("#define L_SIZE " + maxLinks);
        }

        private void WriteSymbolAliases()
        {
            foreach (string s in kpm.Alphabet.Symbols)
            {
                owt.WriteLine("int {0} = {1};", promelaSymbol(s), kpm.Alphabet[s]);
            }
            owt.WriteLine("c_code {");
            foreach (string s in kpm.Alphabet.Symbols)
            {
                owt.WriteLine("\tint {0} = {1};", promelaSymbol(s), kpm.Alphabet[s]);
            }
            owt.WriteLine("};");
        }

        private string promelaSymbol(string symbol)
        {
            return symbol + "_";
        }

        private void WriteCompartmentTypedef()
        {
            owt.WriteLine("mtype = {initial, running, rules_applied, step_complete, finished}");
            owt.WriteLine();
            owt.WriteLine("typedef Compartment {");
            owt.WriteLine("\tint type;");
            owt.WriteLine("\tint x[A_SIZE] = 0;");
            owt.WriteLine("\tint xt[A_SIZE] = 0;");
            if (maxLinks > 0)
            {
                owt.WriteLine("\tint links[L_SIZE] = -1;");
                owt.WriteLine("\tint lTCount[{0}] = 0;", kp.Types.Count);
                owt.WriteLine("\tint lCount = 0;");
            }
            if (kpm.HasStructureChangingRules)
            {
                owt.WriteLine("\tbit structureChanged = 0;");
                if (kpm.HasDissolution)
                {
                    owt.WriteLine("\tbit flagDissolved = 0;");
                    owt.WriteLine("\tbit isDissolved = 0;");
                }
                if (kpm.HasDivision)
                {
                    owt.WriteLine("\tbit flagDivided = 0;");
                    owt.WriteLine("\tbit isDivided = 0;");
                    owt.WriteLine("\tint inheritsFrom = -1;");
                }
                if (kpm.HasLinkCreation)
                {
                    owt.WriteLine("\tint createLinkTo = -1;");
                }
                if (kpm.HasLinkDestruction)
                {
                    owt.WriteLine("\tint destroyLinkTo = -1;");
                }
            }
            owt.WriteLine("}");
        }

        private void WriteGlobalVars()
        {
            WriteLine("Compartment c[C_SIZE];");
            WriteLine("mtype state;");
            WriteLine("int cCount = 0;");
            WriteLine("int cTCount[{0}] = 0;", kp.Types.Count);
            if (kpm.HasDivision)
            {
                WriteLine("int nCCount = 0;");
                WriteLine("int nCStart = 0;");
            }
            WriteLine("int step = -1;");
            WriteLine("int rulesAppliedThisStep = 0;");
            WriteLine("bit deadlock = 0;");
        }

        private void WriteProperties(IEnumerable<ILtlProperty> properties)
        {
            var kpMetaModel = new KpMetaModel(kp);
            var propertyIndex = 1;

            WriteLine("/* LTL Properties */");

            foreach (var property in properties)
            {
                var translatedProperty = PropertyTranslationManager.Instance.Translate(property, kpMetaModel, ModelCheckingTarget.Promela);
                WriteLine(String.Format("ltl prop{0} {{ {1} }}", propertyIndex++, translatedProperty));
            }

            WriteLine();
        }

        private void WriteConnectedTargetSelectInline()
        {
            owt.WriteLine("inline selectConnectedTarget(ci, ti) {");
            owt.WriteLine("\tint cskip = -1;");
            owt.WriteLine("\tint k = 0;");
            owt.WriteLine("\tdo :: cskip < ntar ->");
            owt.WriteLine("\t\tif :: c[ci].links[k] > -1 && c[c[ci].links[k]].type == ti ->");
            owt.WriteLine("\t\t\tcskip++;");
            owt.WriteLine("\t\t\tj = k;");
            owt.WriteLine("\t\t   :: else -> skip;");
            owt.WriteLine("\t\tfi;");
            owt.WriteLine("\t\tk++;");
            owt.WriteLine("\t   :: else -> break;");
            owt.WriteLine("\tod;");
            //end inline declaration
            owt.WriteLine("}");
        }

        private void WriteConnectedTargetSelect()
        {
            WriteLine("int selectTarget(int ntar, int ci, int ti) {");
            indent++;
            WriteLine("int cskip = -1;");
            WriteLine("int k = -1;");
            WriteLine("int link = -1;");
            WriteLine("while(cskip < ntar) {");
            indent++;
            WriteLine("link = now.c[ci].links[++k];");
            WriteLine("if(link > -1 && now.c[link].type == ti) {");
            indent++;
            WriteLine("++cskip;");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
            WriteLine("return link;");
            indent--;
            WriteLine("}");
        }

        private void WriteDisconnectedTargetSelectInline()
        {
            owt.WriteLine("inline selectDisconnectedTarget(ci, ti) {");
            owt.WriteLine("\tint cskip = -1;");
            owt.WriteLine("\tint k = 0;");
            owt.WriteLine("\tdo :: cskip < ntar ->");
            owt.WriteLine("\t\tif :: c[ci].links[k] == -1 && c[c[ci].links[k]].type == ti ->");
            owt.WriteLine("\t\t\tcskip++;");
            owt.WriteLine("\t\t\tj = k;");
            owt.WriteLine("\t\t   :: else -> skip;");
            owt.WriteLine("\t\tfi;");
            owt.WriteLine("\t\tk++;");
            owt.WriteLine("\t   :: else -> break;");
            owt.WriteLine("\tod;");
            //end inline declaration
            owt.WriteLine("}");
        }

        private void WriteDisconnectedTargetSelect()
        {
            WriteLine("int selectLinkFreeTarget(int ntar, int ci, int ti) {");
            indent++;
            WriteLine("int cskip = -1;");
            WriteLine("int k = -1;");
            WriteLine("while(cskip < ntar) {");
            indent++;
            WriteLine("++k;");
            Write("if(!(ci == k || ");
            if (kpm.HasDivision)
            {
                owt.Write("now.c[k].isDivided || ");
            }
            if (kpm.HasDissolution)
            {
                owt.Write("now.c[k].isDissolved || ");
            }
            owt.WriteLine("linked(ci, k))) {");
            indent++;
            WriteLine("++cskip;");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
            WriteLine("return k;");
            indent--;
            WriteLine("}");
        }

        private void WriteLinkedFunction()
        {
            WriteLine("int linked(int ci1, int ci2) {");
            indent++;
            WriteLine("int i = 0;");
            WriteLine("int lCount = now.c[ci1].lCount;");
            WriteLine("for(i = 0; i < lCount; i++) {");
            indent++;
            WriteLine("if(now.c[ci1].links[i] == ci2) {");
            indent++;
            WriteLine("return 1;");
            indent--;
            WriteLine("} else if(now.c[ci1].links[i] == -1) {");
            indent++;
            WriteLine("lCount++;");
            indent--;
            WriteLine("}");
            indent--;
            //end for
            WriteLine("}");
            WriteLine("return 0;");
            indent--;
            WriteLine("}");
        }

        private void WriteCopyLinksFunction()
        {
            //assumes "to" is a newly created membrane instance
            WriteLine("void copyLinks(int from, int to) {");
            indent++;
            WriteLine("int i, j;");
            WriteLine("int lCount = now.c[to].lCount;");
            WriteLine("int fromLCount = now.c[from].lCount;");
            WriteLine("for(i = 0; i < fromLCount; i++) {");
            indent++;
            WriteLine("int li = now.c[from].links[i];");
            WriteLine("if(li > -1) {");
            indent++;
            if (kpm.HasDivision)
            {
                WriteLine("if(now.c[li].isDivided) {");
                indent++;
                WriteLine("for(j = 1; j <= now.nCCount; j++) {");
                indent++;
                WriteLine("int cj = now.nCStart - j;");
                WriteLine("int x = now.c[cj].inheritsFrom;");
                WriteLine("if(x == li) {");
                indent++;
                WriteLine("now.c[to].links[lCount++] = cj;");
                WriteLine("now.c[to].lTCount[now.c[cj].type]++;");
                //do not add bidirectional link here since it will be added in the iteration;
                indent--;
                WriteLine("}");
                indent--;
                WriteLine("}");
                indent--;
                WriteLine("} else {");
                indent++;
            }
            WriteLine("now.c[to].links[lCount++] = li;");
            WriteLine("now.c[to].lTCount[now.c[li].type]++;");
            WriteLine("addLink(li, to);");
            if (kpm.HasDivision)
            {
                indent--;
                WriteLine("}");
            }
            indent--;
            //end if
            WriteLine("} else {");
            indent++;
            WriteLine("fromLCount++;");
            indent--;
            WriteLine("}");
            indent--;
            //end for
            WriteLine("}");
            WriteLine("now.c[to].lCount = lCount;");
            indent--;
            WriteLine("}");
        }

        private void WriteInvalidateLinksFunction()
        {
            WriteLine("void invalidateLinks(int p) {");
            indent++;
            WriteLine("int i;");
            WriteLine("int lCount = now.c[p].lCount;");
            WriteLine("for(i = 0; i < lCount; i++) {");
            indent++;
            WriteLine("if(now.c[p].links[i] > -1) {");
            indent++;
            WriteLine("removeLink(now.c[p].links[i], p);");
            WriteLine("now.c[p].links[i] = -1;");
            indent--;
            WriteLine("} else {");
            indent++;
            WriteLine("lCount++;");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
            WriteLine("now.c[p].lCount = 0;");
            WriteLine("for(i = 0; i < {0}; i++) {{", kp.Types.Count);
            indent++;
            WriteLine("now.c[p].lTCount[i] = 0;");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
        }

        private void WriteLinkAddFunction()
        {
            WriteLine("void addLink(int from, int to) {");
            indent++;
            WriteLine("int i = 0;");
            WriteLine("while(now.c[from].links[i] > -1) {");
            indent++;
            WriteLine("++i;");
            indent--;
            WriteLine("}");
            WriteLine("now.c[from].links[i] = to;");
            WriteLine("now.c[from].lCount++;");
            WriteLine("now.c[from].lTCount[now.c[to].type]++;");
            indent--;
            WriteLine("}");
        }

        private void WriteBidirectionalLinkFunction()
        {
            WriteLine("void connectBidirectional(int from, int to) {");
            indent++;
            WriteLine("addLink(from, to);");
            WriteLine("addLink(to, from);");
            indent--;
            WriteLine("}");
        }

        private void WriteRemoveLinkFunction()
        {
            WriteLine("void removeLink(int from, int to) {");
            indent++;
            WriteLine("int i;");
            WriteLine("int lCount = now.c[from].lCount;");
            WriteLine("for(i = 0; i < lCount; i++) {");
            indent++;
            WriteLine("if(now.c[from].links[i] == to) {");
            indent++;
            WriteLine("now.c[from].links[i] = -1;");
            WriteLine("now.c[from].lCount--;");
            WriteLine("now.c[from].lTCount[now.c[to].type]--;");
            WriteLine("return;");
            indent--;
            WriteLine("} else if(now.c[from].links[i] < 0) {");
            indent++;
            WriteLine("lCount++;");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
        }

        private void WriteGetApplicabilityRateFunction()
        {
            WriteLine("int getApplicabilityRate(int ci, int ruleLhs[], int lhsSize) {");
            indent++;
            WriteLine("int i;");
            WriteLine("int app = -1;");
            WriteLine("for(i = 0; i < lhsSize; i += 2) {");
            indent++;
            WriteLine("int obj = ruleLhs[i];");
            WriteLine("int mult = ruleLhs[i + 1];");
            WriteLine("if(now.c[ci].x[obj] < mult) {");
            indent++;
            WriteLine("return 0;");
            indent--;
            WriteLine("} else {");
            indent++;
            WriteLine("int cap = now.c[ci].x[obj] / mult;");
            WriteLine("if(app == -1) {");
            indent++;
            WriteLine("app = cap;");
            indent--;
            WriteLine("} else if(cap < app) {");
            indent++;
            WriteLine("app = cap;");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
            indent--;
            WriteLine("}");
            WriteLine("return app;");
            indent--;
            WriteLine("}");
        }

        private void WriteIndent()
        {
            for (int i = 0; i < indent; i++)
            {
                owt.Write("\t");
            }
        }

        private void Write(string text, params object[] args)
        {
            WriteIndent();
            owt.Write(text, args);
        }

        private void Write(string text)
        {
            WriteIndent();
            owt.Write(text);
        }

        private void WriteLine()
        {
            owt.WriteLine();
        }

        private void WriteLine(string text, params object[] args)
        {
            Write(text, args);
            WriteLine();
        }

        private void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

    }
}
