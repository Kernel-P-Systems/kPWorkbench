using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using KpExperiment.Verification.Translation;
using KpExperiment.Verification.Translation.Base;
using KpSpin.Simulation;
using KpUtil;
using System;
using System.Collections.Generic;

namespace KpSpin
{

    //static attributes
    public static class A
    {
        public static Attribute i = new Attribute("i");
        public static Attribute j = new Attribute("j");
        public static Attribute k = new Attribute("k");

        public static Attribute isDissolved = new Attribute("isDissolved");
        public static Attribute isDivided = new Attribute("isDivided");
        public static Attribute strChanged = new Attribute("strChanged");
        public static Attribute isComputing = new Attribute("isComputing");

        public static Attribute ks = new Attribute("ks");
        public static Attribute tar = new Attribute("tar");

        public static Constant zero = new Constant(0);
        public static Constant one = new Constant(1);
        public static Constant _one = new Constant(-1);

        public static Attribute MaxDivisions = new Attribute("MAX_DIVISIONS");
        public static Attribute MaxInstances = new Attribute("MAX_INSTANCES");
        public static Attribute MaxLinks = new Attribute("MAX_INSTANCES");
        public static Attribute MaxSteps = new Attribute("MAX_STEPS");


        public static Constant state_initial = new Constant("initial");
        public static Constant state_running = new Constant("running");
        public static Constant state_rules_applied = new Constant("rules_applied");
        public static Constant state_step_complete = new Constant("step_complete");
        public static Constant state_finished = new Constant("finished");
        public static Constant state_printing = new Constant("printing");
        public static Constant state_print_complete = new Constant("print_complete");
    }

    public class KpTranslator
    {
        private KPsystem src;
        private Experiment experiment;
        private KpMetaModel kmm;

        public KPsystem KPsystem { get { return src; } set { src = value; kmm = new KpMetaModel(src); } }

        private PromelaTranslationParams param;
        public PromelaTranslationParams Params { get { return param; } set { param = value; } }

        private string currentTrace;

        public KpTranslator(KPsystem kp)
            : this(kp, PromelaTranslationParams.Default())
        {
        }

        public KpTranslator(KPsystem kpModel, Experiment experiment, PromelaTranslationParams pars)
            : this(kpModel, pars)
        {
            this.experiment = experiment;
        }

        public KpTranslator(KPsystem kp, PromelaTranslationParams pars)
        {
            KPsystem = kp;
            Params = pars;
        }

        public PromelaModel Translate()
        {
            PromelaModel model = new PromelaModel();

            if (src == null)
            {
                return model;
            }

            Params.MaxInstances = 1000;

            if (Params.MaxSteps == 0)
            {
                Params.MaxSteps = 5;
            }

            if (kmm.HasDivision)
            {
                model.Constants.Add(new ConstantDeclaration(A.MaxDivisions.Name, Params.MaxDivisions));
                model.Constants.Add(new ConstantDeclaration(A.MaxInstances.Name, Params.MaxInstances));
            }

            if (kmm.HasLinkOps)
            {
                model.Constants.Add(new ConstantDeclaration(A.MaxLinks.Name, Params.MaxLinks));
            }

            model.Constants.Add(new ConstantDeclaration(A.MaxSteps.Name, Params.MaxSteps));

            model.MTypeDeclaration.Add(
                A.state_initial.Value,
                A.state_running.Value,
                A.state_rules_applied.Value,
                A.state_step_complete.Value,
                A.state_finished.Value);
            if (Params.PrintConfiguration)
            {
                model.MTypeDeclaration.Add(A.state_printing.Value, A.state_print_complete.Value);
            }

            VariableDeclaration state_decl = new VariableDeclaration(VarType.MTYPE, "state");
            Attribute state = state_decl.ToAttribute();
            model.GlobalVariables.Add(state_decl);

            VariableDeclaration step_decl = new VariableDeclaration(VarType.INT, "step", 0);
            Attribute step = step_decl.ToAttribute();
            model.GlobalVariables.Add(step_decl);

            VariableDeclaration halted_decl = new VariableDeclaration(VarType.BIT, "halt", 0);
            Attribute halted = halted_decl.ToAttribute();
            model.GlobalVariables.Add(halted_decl);

            List<Statement> init = model.Init.NestedStatements;

            VariableDeclaration v_i = new VariableDeclaration(VarType.INT, A.i.Name, 0);
            VariableDeclaration v_j = new VariableDeclaration(VarType.INT, A.j.Name, 0);

            Dictionary<int, List<int>> chooseTargetInlines = new Dictionary<int, List<int>>();
            Dictionary<int, Dictionary<int, bool>> copyInstanceInlines = new Dictionary<int, Dictionary<int, bool>>();

            if (experiment != null && experiment.LtlProperties != null)
            {
                model.PropertyTranslations.AddRange(TranslateProperties(experiment.LtlProperties));
            }

            //typedefs & processes
            foreach (MTypeMeta typeMeta in kmm.TypeMeta)
            {
                if (typeMeta.IsEmpty)
                {
                    continue;
                }
                ConstantDeclaration a_size_decl = new ConstantDeclaration("A" + typeMeta.Id + "_SIZE", typeMeta.AlphabetSize);
                Attribute a_size = new Attribute(a_size_decl.Name);
                model.Constants.Add(a_size_decl);

                TypeDef def = new TypeDef("C" + typeMeta.Id);
                model.TypeDefs.Add(def);
                def.NestedStatements.Add(new ArrayDeclaration(VarType.INT, "x", a_size, 0));
                def.NestedStatements.Add(new ArrayDeclaration(VarType.INT, "xt", a_size, 0));

                foreach (KeyValuePair<MTypeMeta, int> kv in typeMeta.Connections)
                {
                    MTypeMeta con = kv.Key;
                    if (Params.LinksEnabled)
                    {
                        int linkSize = kv.Value > -1 ? kv.Value : Params.MaxLinks;
                        def.NestedStatements.Add(new ArrayDeclaration(VarType.INT, "c" + con.Id + "Links", linkSize));
                        def.NestedStatements.Add(new VariableDeclaration(VarType.INT, "c" + con.Id + "LCount", 0));
                        def.NestedStatements.Add(new VariableDeclaration(VarType.INT, "c" + con.Id + "LSize", 0));
                    }
                }

                int cSize = typeMeta.Instances.Length;

                def.NestedStatements.Add(new VariableDeclaration(VarType.BIT, A.isComputing.Name, 0));
                if (typeMeta.HasStructureChangingRules)
                {
                    if (typeMeta.HasDissolution)
                    {
                        def.NestedStatements.Add(new VariableDeclaration(VarType.BIT, A.isDissolved.Name, 0));
                    }
                    if (typeMeta.HasDivision)
                    {
                        def.NestedStatements.Add(new VariableDeclaration(VarType.BIT, A.isDivided.Name, 0));
                        cSize = -1;
                    }
                }

                if (typeMeta.MaxDivisionRate > 0)
                {
                    cSize = -1;
                    def.NestedStatements.Add(new VariableDeclaration(VarType.BIT, A.strChanged.Name, 0));
                }

                if (cSize > 0 || cSize == -1)
                {
                    if (cSize == -1)
                    {
                        cSize = Params.MaxInstances;
                    }
                    ArrayDeclaration ci = new ArrayDeclaration(def, "m" + typeMeta.Id, cSize);
                    model.GlobalVariables.Add(ci);

                    VariableDeclaration si_decl = new VariableDeclaration(VarType.INT, "m" + typeMeta.Id + "Size", 0);
                    Attribute si = si_decl.ToAttribute();
                    model.GlobalVariables.Add(si_decl);
                    init.Add(new Assignment(si, typeMeta.Instances.Length));

                    Attribute sit = null;
                    if (typeMeta.MaxDivisionRate > 0)
                    {
                        VariableDeclaration sit_decl = new VariableDeclaration(VarType.INT, "m" + typeMeta.Id + "SizeT", 0);
                        sit = sit_decl.ToAttribute();
                        model.GlobalVariables.Add(sit_decl);
                        init.Add(new Assignment(sit, si));
                    }

                    VariableDeclaration vi_decl = new VariableDeclaration(VarType.INT, "m" + typeMeta.Id + "Count", 0);
                    Attribute vi = vi_decl.ToAttribute();
                    model.GlobalVariables.Add(vi_decl);
                    init.Add(new Assignment(vi, si));

                    foreach (MInstanceMeta instanceMeta in typeMeta.MInstanceMetaSet)
                    {
                        foreach (KeyValuePair<string, int> kv in instanceMeta.MInstance.Multiset)
                        {
                            Symbol s = typeMeta.Symbol(kv.Key);
                            //in init: mi[k].x[j] = value;
                            init.Add(new Assignment(ci.ToAttribute(instanceMeta.Id).Dot("x", s.Id), kv.Value));
                        }

                        Attribute ciAttr = ci.ToAttribute(instanceMeta.Id);

                        if (Params.LinksEnabled)
                        {
                            foreach (MInstance mi in instanceMeta.MInstance.Connections)
                            {
                                MInstanceMeta im = kmm.GetInstanceMeta(mi);

                                //only add this connection if the type meta recorded an interest in this connection
                                if (typeMeta.Connections.ContainsKey(im.MTypeMeta))
                                {
                                    Attribute lCount = ciAttr.Dot(new Attribute("c" + im.MTypeMeta.Id + "LCount"));
                                    Attribute lAttr = ciAttr.Dot(new Attribute("c" + im.MTypeMeta.Id + "Links", lCount));
                                    init.Add(new Assignment(lAttr, im.Id));
                                    init.Add(Instruction.Increment(lCount));
                                }
                            }

                            foreach (MTypeMeta mtm in typeMeta.Connections.Keys)
                            {
                                init.Add(new Assignment(ciAttr.Dot(new Attribute("c" + mtm.Id + "LSize")), ciAttr.Dot(new Attribute("c" + mtm.Id + "LCount"))));
                            }
                        }
                    }

                    ProcessDef proctype = new ProcessDef("P" + typeMeta.Id);
                    proctype.Parameters.Add(new ProcessParam(VarType.INT, "k"));
                    model.Processes.Add(proctype);
                    BracedStatement proctypeAtomic = BracedStatement.Atomic();
                    proctype.NestedStatements.Add(proctypeAtomic);
                    List<Statement> pat = proctypeAtomic.NestedStatements;
                    Attribute ck = ci.ToAttribute("k");

                    bool iDeclRequired = false;
                    bool ksDeclRequired = false;
                    bool includeEOP = false;
                    ExecutionStrategy ex = typeMeta.MType.ExecutionStrategy;

                    while (ex != null)
                    {
                        ConditionalStatement cs = null;
                        foreach (Rule r in ex.Rules)
                        {
                            bool ruleIsEventuallyApplicable = true;

                            if (r is ConsumerRule)
                            {
                                ConsumerRule consR = r as ConsumerRule;
                                RuleMeta rm = kmm.GetRuleMeta(r);
                                Attribute guard = new Attribute("g" + rm.Id);
                                ICondition condition = null;

                                if (r.IsGuarded)
                                {
                                    condition = guardToCondition(r.Guard, ck, typeMeta);
                                }

                                Branch mainBr = new Branch();

                                foreach (KeyValuePair<string, int> kv in consR.Lhs)
                                {
                                    Symbol x = typeMeta.Symbol(kv.Key);
                                    Attribute obj = ck.Dot("x", x.Id);
                                    mainBr.Actions.Add(new Assignment(obj, new ArithmeticExpression(PromelaArithmeticOperator.MINUS, obj, kv.Value)));
                                }

                                if (r.Type == RuleType.MULTISET_REWRITING)
                                {
                                    foreach (KeyValuePair<string, int> kv in (consR as RewritingRule).Rhs)
                                    {
                                        Symbol x = typeMeta.Symbol(kv.Key);
                                        Attribute obj_temp = ck.Dot("xt", x.Id);
                                        mainBr.Actions.Add(new Assignment(obj_temp, new ArithmeticExpression(PromelaArithmeticOperator.PLUS, obj_temp, kv.Value)));
                                    }
                                }
                                else if (r.Type == RuleType.REWRITE_COMMUNICATION)
                                {
                                    RewriteCommunicationRule rcr = r as RewriteCommunicationRule;

                                    //a communication rule is eventually applicable if there is a possibility that all its communicative atoms 
                                    //are target satisfiable (there will be a connection to at least one target of the specified type)

                                    foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in rcr.TargetRhs)
                                    {
                                        if (kv.Key is InstanceIdentifier)
                                        {
                                            InstanceIdentifier iid = kv.Key as InstanceIdentifier;
                                            if (iid.Indicator == InstanceIndicator.TYPE)
                                            {
                                                MTypeMeta mtm = kmm.GetTypeMetaByName(iid.Value);
                                                if (mtm.IsEmpty)
                                                {
                                                    ruleIsEventuallyApplicable = false;
                                                }
                                                else if (Params.LinksEnabled)
                                                {
                                                    if (!typeMeta.Connections.ContainsKey(mtm))
                                                    {
                                                        ruleIsEventuallyApplicable = false;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (ruleIsEventuallyApplicable)
                                    {
                                        string form = null;
                                        List<IValue> args = null;
                                        if (Params.PrintTargetSelection)
                                        {
                                            form = "$" + rm.Id + " " + typeMeta.Id + " %d";
                                            args = new List<IValue>();
                                            args.Add(A.k);
                                        }

                                        foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in rcr.TargetRhs)
                                        {
                                            if (kv.Key is InstanceIdentifier)
                                            {
                                                InstanceIdentifier iid = kv.Key as InstanceIdentifier;
                                                if (iid.Indicator == InstanceIndicator.TYPE)
                                                {
                                                    MTypeMeta mtm = kmm.GetTypeMetaByName(iid.Value);
                                                    ICondition c = null;
                                                    if (Params.LinksEnabled)
                                                    {
                                                        c = new RelCondition(PromelaRelOperator.GT, ck.Dot("c" + mtm.Id + "LCount"), A.zero);
                                                    }
                                                    else
                                                    {
                                                        c = new RelCondition(PromelaRelOperator.GT, new Attribute("m" + mtm.Id + "Count"), A.zero);
                                                    }

                                                    if (condition == null)
                                                    {
                                                        condition = c;
                                                    }
                                                    else
                                                    {
                                                        condition = new CompositeCondition(PromelaLogicalOperator.AND, condition, c);
                                                    }

                                                    VariableDeclaration tar_decl = new VariableDeclaration(VarType.INT, "tar" + rm.Id + mtm.Id, -1);
                                                    Attribute tar_i = tar_decl.ToAttribute();
                                                    pat.Add(tar_decl);

                                                    //Keep track of inline definitions added to the model and don't duplicate any definition
                                                    List<int> inlinesTo = null;
                                                    if (chooseTargetInlines.TryGetValue(typeMeta.Id, out inlinesTo))
                                                    {
                                                        if (!inlinesTo.Contains(mtm.Id))
                                                        {
                                                            model.InlineDefinitions.Add(chooseTargetInline(typeMeta, mtm));
                                                            inlinesTo.Add(mtm.Id);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        inlinesTo = new List<int>();
                                                        inlinesTo.Add(mtm.Id);
                                                        model.InlineDefinitions.Add(chooseTargetInline(typeMeta, mtm));
                                                    }

                                                    string inlineName = "chooseTarget";
                                                    if (Params.LinksEnabled)
                                                    {
                                                        inlineName += typeMeta.Id.ToString() + mtm.Id.ToString();
                                                        mainBr.Actions.Add(Instruction.InlineCall(inlineName, tar_i.Name, A.k.Name));
                                                    }
                                                    else
                                                    {
                                                        inlineName += mtm.Id;
                                                        mainBr.Actions.Add(Instruction.InlineCall(inlineName, tar_i.Name));
                                                    }

                                                    if (Params.PrintTargetSelection)
                                                    {
                                                        form += " " + mtm.Id + " %d";
                                                        args.Add(tar_i);
                                                    }

                                                    foreach (KeyValuePair<string, int> ms in kv.Value.Multiset)
                                                    {
                                                        Symbol x = mtm.Symbol(ms.Key);
                                                        Attribute obj_temp = new Attribute("m" + mtm.Id, tar_i).Dot("xt", x.Id);
                                                        mainBr.Actions.Add(new Assignment(obj_temp,
                                                            new ArithmeticExpression(PromelaArithmeticOperator.PLUS, obj_temp, ms.Value)));
                                                    }
                                                }
                                            }

                                            iDeclRequired = true;
                                            ksDeclRequired = true;
                                        }

                                        foreach (KeyValuePair<string, int> kv in rcr.Rhs)
                                        {
                                            Symbol x = typeMeta.Symbol(kv.Key);
                                            Attribute obj_temp = ck.Dot("xt", x.Id);
                                            mainBr.Actions.Add(new Assignment(obj_temp,
                                                new ArithmeticExpression(PromelaArithmeticOperator.PLUS, obj_temp, kv.Value)));
                                        }

                                        if (form != null)
                                        {
                                            mainBr.Actions.Add(new Printf(form + "\\n", args.ToArray()));
                                        }
                                    }
                                }
                                else if (r.Type == RuleType.MEMBRANE_DIVISION)
                                {
                                    DivisionRule dr = r as DivisionRule;
                                    mainBr.Actions.Add(new Assignment(ck.Dot(A.strChanged), 1));

                                    foreach (InstanceBlueprint ib in dr.Rhs)
                                    {
                                        MTypeMeta mtm = kmm.GetTypeMetaByType(ib.Type);
                                        Attribute targetSize = new Attribute("m" + mtm.Id + "SizeT");
                                        Attribute cts = new Attribute("m" + mtm.Id, targetSize);


                                        bool skipInline = false;
                                        Dictionary<int, bool> inlinesTo = null;
                                        if (copyInstanceInlines.TryGetValue(typeMeta.Id, out inlinesTo))
                                        {
                                            if (!inlinesTo.TryGetValue(mtm.Id, out skipInline))
                                            {
                                                Inline il = copyInstanceInline(typeMeta, mtm);
                                                skipInline = il.NestedStatements.Count == 0;
                                                if (!skipInline)
                                                {
                                                    model.InlineDefinitions.Add(il);
                                                }
                                                inlinesTo.Add(mtm.Id, skipInline);
                                            }
                                        }
                                        else
                                        {
                                            inlinesTo = new Dictionary<int, bool>();
                                            copyInstanceInlines.Add(typeMeta.Id, inlinesTo);

                                            Inline il = copyInstanceInline(typeMeta, mtm);
                                            skipInline = il.NestedStatements.Count == 0;
                                            if (!skipInline)
                                            {
                                                model.InlineDefinitions.Add(il);
                                            }
                                            inlinesTo.Add(mtm.Id, skipInline);
                                        }

                                        if (!skipInline)
                                        {
                                            mainBr.Actions.Add(Instruction.InlineCall("copyInstance" + typeMeta.Id + mtm.Id, A.k.Name, targetSize.Name));
                                        }

                                        foreach (KeyValuePair<string, int> kv in ib.Multiset)
                                        {
                                            Symbol x = mtm.Symbol(kv.Key);
                                            Symbol y = typeMeta.Symbol(kv.Key);
                                            Attribute ctsx = cts.Dot("x", x.Id);
                                            mainBr.Actions.Add(new Assignment(ctsx,
                                                new ArithmeticExpression(PromelaArithmeticOperator.PLUS, ctsx, kv.Value)));
                                        }
                                        mainBr.Actions.Add(Instruction.Increment(targetSize));
                                    }

                                    mainBr.Actions.Add(new Assignment(ck.Dot(A.isDivided), 1));
                                    mainBr.Actions.Add(new Assignment(ck.Dot(A.strChanged), 1));
                                }
                                else if (r.Type == RuleType.LINK_CREATION)
                                {

                                }
                                else if (r.Type == RuleType.LINK_DESTRUCTION)
                                {

                                }

                                if (!ruleIsEventuallyApplicable)
                                {
                                    if (ex.Operator == StrategyOperator.SEQUENCE)
                                    {
                                        //if this rule blocks the rest from executing, then discard all future rules or ensembles in 
                                        //this set
                                        break;
                                    }
                                    else
                                    {
                                        //discard this rule only, other may still execute
                                        continue;
                                    }
                                }


                                if (Params.PrintRuleExecution)
                                {
                                    mainBr.Actions.Add(new Printf("@" + rm.Id + " " + typeMeta.Id + " %d\\n", A.k));
                                }

                                if (condition != null)
                                {
                                    VariableDeclaration guard_decl = new VariableDeclaration(VarType.BIT, guard.Name, condition.ToPromela());
                                    pat.Add(guard_decl);
                                }

                                BasicGuard bg = new BasicGuard(consR.Lhs, KpCore.RelationalOperator.GEQ);

                                if (condition == null)
                                {
                                    mainBr.Condition = guardToCondition(bg, ck, typeMeta);
                                    if (r.Type == RuleType.MEMBRANE_DIVISION)
                                    {
                                        mainBr.Condition = new CompositeCondition(PromelaLogicalOperator.AND,
                                            mainBr.Condition,
                                            new NegatedCondition(
                                                new Condition(ck.Dot(A.strChanged))));
                                    }
                                }
                                else
                                {
                                    mainBr.Condition = new CompositeCondition(PromelaLogicalOperator.AND,
                                        new Condition(guard),
                                        guardToCondition(bg, ck, typeMeta));
                                }

                                if (ex.Operator == StrategyOperator.CHOICE)
                                {
                                    if (cs == null)
                                    {
                                        cs = new IfStatement();
                                    }
                                    cs.Branch(mainBr);
                                }
                                else if (ex.Operator == StrategyOperator.ARBITRARY)
                                {
                                    if (cs == null)
                                    {
                                        cs = new DoStatement();
                                    }
                                    cs.Branch(mainBr);
                                }
                                else if (ex.Operator == StrategyOperator.MAX)
                                {
                                    if (cs == null)
                                    {
                                        cs = new DoStatement();
                                    }
                                    cs.Branch(mainBr);
                                }
                                else
                                {
                                    //it must be a sequence
                                    cs = new IfStatement();
                                    pat.Add(cs);
                                    cs.Branch(mainBr);
                                    cs.Else(Instruction.Goto("EOP"));
                                    includeEOP = true;
                                }
                            }
                        }

                        if (cs != null)
                        {
                            if (ex.Operator != StrategyOperator.SEQUENCE)
                            {
                                pat.Add(cs);
                                if (ex.Operator == StrategyOperator.CHOICE)
                                {
                                    cs.Else(Instruction.Skip());
                                }
                                else if (ex.Operator == StrategyOperator.MAX)
                                {
                                    cs.Else(Instruction.Break());
                                }
                                else
                                {
                                    cs.Branch(new Branch(Instruction.Break()));
                                }
                            }
                        }

                        ex = ex.Next;
                    }


                    if (iDeclRequired)
                    {
                        pat.Insert(0, new VariableDeclaration(VarType.INT, A.i.Name, 0));
                    }
                    if (ksDeclRequired)
                    {
                        pat.Insert(1, new VariableDeclaration(VarType.BIT, A.ks.Name, 1));
                    }

                    if (includeEOP)
                    {
                        pat.Add(new Label("EOP", Instruction.Skip()));
                    }
                    pat.Add(new Assignment(ck.Dot(A.isComputing), 0));
                }
            }

            ProcessDef printConfig = null;
            if (Params.PrintConfiguration)
            {
                printConfig = new ProcessDef("printConfiguration");
                CompoundStatement ds = BracedStatement.D_step();
                List<Statement> pcs = ds.NestedStatements;
                printConfig.NestedStatements.Add(ds);
                pcs.Add(v_i);
                pcs.Add(v_j);

                List<Attribute> pfa = new List<Attribute>();
                Printf pf = new Printf("#%d{", step);
                pcs.Add(pf);
                foreach (MTypeMeta typeMeta in kmm.TypeMeta)
                {
                    if (typeMeta.IsEmpty)
                    {
                        continue;
                    }
                    Attribute m_i = new Attribute("m" + typeMeta.Id, A.i);
                    pcs.Add(new Printf(typeMeta.Id + "{"));
                    ForStatement fs = new ForStatement(A.i.Name, A.zero, new Attribute("m" + typeMeta.Id + "Size - 1"));
                    IValue dis = null;
                    if (typeMeta.HasDissolution)
                    {
                        dis = m_i.Dot(A.isDissolved);
                    }
                    else
                    {
                        dis = A.zero;
                    }

                    IValue div = null;
                    if (typeMeta.HasDivision)
                    {
                        div = m_i.Dot(A.isDivided);
                    }
                    else
                    {
                        div = A.zero;
                    }

                    fs.NestedStatements.Add(new Printf("%d{0{%d %d}", A.i, dis, div));

                    fs.NestedStatements.Add(new Printf("1{"));
                    ForStatement fss = new ForStatement(A.j.Name, A.zero, new Attribute("A" + typeMeta.Id + "_SIZE - 1"));
                    fss.NestedStatements.Add(new Printf(" %d %d", A.j, m_i.Dot("x[j]")));
                    fs.NestedStatements.Add(fss);
                    fs.NestedStatements.Add(new Printf("}}"));

                    pcs.Add(fs);
                    pcs.Add(new Printf("}"));
                }
                pcs.Add(new Printf("}\\n"));
                pcs.Add(new Assignment(state, A.state_print_complete));
                model.Processes.Add(printConfig);
            }


            //scheduler
            ProcessDef scheduler = new ProcessDef("Scheduler");
            scheduler.NestedStatements.Add(new Assignment(state, A.state_initial));
            if (Params.PrintConfiguration && printConfig != null)
            {
                scheduler.NestedStatements.Add(new Assignment(state, A.state_printing));
                scheduler.NestedStatements.Add(Instruction.ProcessRun(printConfig));
                scheduler.NestedStatements.Add(new PredicateStatement(
                    new RelCondition(PromelaRelOperator.EQ, state, A.state_print_complete)));
            }
            scheduler.NestedStatements.Add(v_i);
            scheduler.NestedStatements.Add(v_j);

            BracedStatement at1 = BracedStatement.Atomic();
            foreach (MTypeMeta typeMeta in kmm.TypeMeta)
            {
                if (typeMeta.IsEmpty)
                {
                    continue;
                }

                Attribute m_i = new Attribute("m" + typeMeta.Id, A.i);
                ForStatement fs = new ForStatement(A.i.Name, A.zero, new Attribute("m" + typeMeta.Id + "Size - 1"));
                at1.NestedStatements.Add(fs);

                ICondition excl = null;
                if (typeMeta.HasDissolution)
                {
                    excl = new Condition(m_i.Dot(A.isDissolved));
                }
                if (typeMeta.HasDivision)
                {
                    ICondition c = new Condition(m_i.Dot(A.isDivided));
                    if (excl == null)
                    {
                        excl = c;
                    }
                    else
                    {
                        excl = new CompositeCondition(PromelaLogicalOperator.AND, excl, c);
                    }
                }

                if (excl == null)
                {
                    fs.NestedStatements.Add(new Assignment(m_i.Dot("isComputing"), A.one));
                    fs.NestedStatements.Add(Instruction.ProcessRun("P" + typeMeta.Id, A.i.Name));
                }
                else
                {
                    //if isDissolved or isDivided skip else run process associated to type
                    IfStatement ifs = new IfStatement();
                    Branch branch = new Branch(excl, Instruction.Skip());
                    ifs.Branch(branch);
                    ifs.Else(new Assignment(m_i.Dot("isComputing"), A.one), Instruction.ProcessRun("P" + typeMeta.Id, A.i.Name));
                    fs.NestedStatements.Add(ifs);
                }
            }

            Branch br = new Branch(
                    new CompositeCondition(PromelaLogicalOperator.AND,
                        new RelCondition(PromelaRelOperator.LT, step, A.MaxSteps),
                            new NegatedCondition(new Condition(halted.Name))),
                                at1);

            foreach (MTypeMeta typeMeta in kmm.TypeMeta)
            {
                if (typeMeta.IsEmpty)
                {
                    continue;
                }
                ForStatement fs = new ForStatement(A.i.Name, A.zero, new Attribute("m" + typeMeta.Id + "Size - 1"));
                br.Actions.Add(fs);

                fs.NestedStatements.Add(new PredicateStatement(
                    new RelCondition(PromelaRelOperator.EQ, new Attribute("m" + typeMeta.Id, A.i).Dot("isComputing"), A.zero)));
            }

            br.Actions.Add(new Assignment(state, A.state_rules_applied));

            BracedStatement ds2 = BracedStatement.D_step();
            br.Actions.Add(ds2);
            foreach (MTypeMeta typeMeta in kmm.TypeMeta)
            {

                if (typeMeta.IsEmpty)
                {
                    continue;
                }

                Attribute m_i = new Attribute("m" + typeMeta.Id, A.i);
                ForStatement fs = new ForStatement(A.i.Name, A.zero, new Attribute("m" + typeMeta.Id + "Size - 1"));
                ds2.NestedStatements.Add(fs);

                ICondition excl = null;
                if (typeMeta.HasDissolution)
                {
                    excl = new Condition(m_i.Dot(A.isDissolved));
                }
                if (typeMeta.HasDivision)
                {
                    ICondition c = new Condition(m_i.Dot(A.isDivided));
                    if (excl == null)
                    {
                        excl = c;
                    }
                    else
                    {
                        excl = new CompositeCondition(PromelaLogicalOperator.AND, excl, c);
                    }
                }

                ForStatement symbolFor = new ForStatement(A.j.Name, A.zero, new Attribute("A" + typeMeta.Id + "_SIZE - 1"));
                Attribute mi_x_j = m_i.Dot(new Attribute("x", A.j));
                Attribute mi_xt_j = m_i.Dot(new Attribute("xt", A.j));
                symbolFor.NestedStatements.Add(
                    new Assignment(mi_x_j,
                        new ArithmeticExpression(PromelaArithmeticOperator.PLUS, mi_x_j, mi_xt_j)));
                symbolFor.NestedStatements.Add(new Assignment(mi_xt_j, 0));

                if (excl == null)
                {
                    fs.NestedStatements.Add(symbolFor);
                }
                else
                {
                    //if isDissolved skip else run process associated to type
                    IfStatement ifs = new IfStatement();
                    Branch branch = new Branch(excl, Instruction.Skip());
                    ifs.Branch(branch);
                    ifs.Else(symbolFor);
                    fs.NestedStatements.Add(ifs);
                }

                if (typeMeta.MaxDivisionRate > 0)
                {
                    ForStatement strChangedFor = new ForStatement(A.i.Name, A.zero, new Attribute("m" + typeMeta.Id + "Size - 1"));
                    ds2.NestedStatements.Add(strChangedFor);
                    strChangedFor.NestedStatements.Add(new Assignment(new Attribute("m" + typeMeta.Id, A.i).Dot(A.strChanged), A.zero));
                    ds2.NestedStatements.Add(new Assignment(new Attribute("m" + typeMeta.Id + "Size"), new Attribute("m" + typeMeta.Id + "SizeT")));
                }
            }

            br.Actions.Add(Instruction.Increment(step));
            br.Actions.Add(new Assignment(state, A.state_step_complete));

            if (Params.PrintConfiguration && printConfig != null)
            {
                br.Actions.Add(new Assignment(state, A.state_printing));
                br.Actions.Add(Instruction.ProcessRun(printConfig));
                br.Actions.Add(new PredicateStatement(
                    new RelCondition(PromelaRelOperator.EQ, state, A.state_print_complete)));
            }

            DoStatement dos = new DoStatement();
            dos.Branch(br);
            dos.Else(Instruction.Break());
            scheduler.NestedStatements.Add(dos);
            scheduler.NestedStatements.Add(new Assignment(state, A.state_finished));

            model.Processes.Add(scheduler);

            init.Add(Instruction.ProcessRun(scheduler));

            return model;

        }

        private ICondition multisetToCondition(Multiset ms, Attribute attr, MTypeMeta mtm)
        {
            if (ms.Count == 0)
            {
                return null;
            }

            ICondition r = null;
            foreach (KeyValuePair<string, int> kv in ms)
            {
                ICondition cond = new RelCondition(PromelaRelOperator.GEQ, attr.Dot("x", mtm.SymbolId(kv.Key)), new Attribute(kv.Value.ToString()));
                if (r == null)
                {
                    r = cond;
                }
                else
                {
                    r = new CompositeCondition(PromelaLogicalOperator.AND, r, cond);
                }
            }

            return r;
        }

        private ICondition guardToCondition(IGuard guard, Attribute attr, MTypeMeta mtm)
        {
            if (guard == null)
            {
                return null;
            }

            if (guard is BasicGuard)
            {
                ICondition r = null;
                BasicGuard bg = guard as BasicGuard;
                PromelaRelOperator op = PromelaRelOperator.EQ;
                switch (bg.Operator)
                {
                    case KpCore.RelationalOperator.EQUAL: op = PromelaRelOperator.EQ; break;
                    case KpCore.RelationalOperator.GEQ: op = PromelaRelOperator.GEQ; break;
                    case KpCore.RelationalOperator.GT: op = PromelaRelOperator.GT; break;
                    case KpCore.RelationalOperator.LT: op = PromelaRelOperator.LT; break;
                    case KpCore.RelationalOperator.LEQ: op = PromelaRelOperator.LEQ; break;
                    case KpCore.RelationalOperator.NOT_EQUAL: op = PromelaRelOperator.NEQ; break;
                }
                foreach (KeyValuePair<string, int> kv in bg.Multiset)
                {
                    ICondition cond = new RelCondition(op, attr.Dot("x", mtm.SymbolId(kv.Key)), new Attribute(kv.Value.ToString()));
                    if (r == null)
                    {
                        r = cond;
                    }
                    else
                    {
                        r = new CompositeCondition(PromelaLogicalOperator.AND, r, cond);
                    }
                }
                return r;
            }
            else if (guard is NegatedGuard)
            {
                return new NegatedCondition(guardToCondition((guard as NegatedGuard).Operand, attr, mtm));
            }
            else if (guard is CompoundGuard)
            {
                CompoundGuard cg = guard as CompoundGuard;
                PromelaLogicalOperator op = PromelaLogicalOperator.AND;
                if (cg.Operator == BinaryGuardOperator.OR)
                {
                    op = PromelaLogicalOperator.OR;
                }
                return new CompositeCondition(op, guardToCondition(cg.Lhs, attr, mtm), guardToCondition(cg.Rhs, attr, mtm));
            }

            return null;
        }

        private Inline chooseTargetInline(MTypeMeta from, MTypeMeta to)
        {

            Inline inline = null;

            if (Params.LinksEnabled)
            {
                inline = new Inline("chooseTarget" + from.Id + to.Id, A.tar.Name, A.k.Name);
            }
            else
            {
                inline = new Inline("chooseTarget" + to.Id, A.tar.Name);
            }
            inline.NestedStatements.Add(new Assignment(A.tar, -1));
            inline.NestedStatements.Add(new Assignment(A.i, 0));
            inline.NestedStatements.Add(new Assignment(A.ks, 1));
            DoStatement doKeepSearching = new DoStatement();
            inline.NestedStatements.Add(doKeepSearching);
            Branch mbr = new Branch(new Condition(A.ks));
            doKeepSearching.Branch(mbr);

            IfStatement ifs = new IfStatement();
            ICondition c = null;

            if (Params.LinksEnabled)
            {
                Attribute cLinks_i = new Attribute("m" + from.Id, A.k).Dot(new Attribute("c" + to.Id + "Links", A.i));
                c = new RelCondition(PromelaRelOperator.NEQ, cLinks_i, A._one);
                ifs.Branch(new Branch(c, new Assignment(A.tar, cLinks_i)));
                ifs.Else(Instruction.Skip());
                mbr.Actions.Add(ifs);
            }
            else
            {
                if (to.HasDissolution || to.HasDivision)
                {
                    if (to.HasDivision)
                    {
                        ICondition cDiv = new NegatedCondition(
                            new Condition(new Attribute("m" + to.Id, A.i).Dot(A.isDivided)));
                        if (c == null)
                        {
                            c = cDiv;
                        }
                        else
                        {
                            c = new CompositeCondition(PromelaLogicalOperator.AND, c, cDiv);
                        }
                    }
                    if (to.HasDissolution)
                    {
                        ICondition cdis = new NegatedCondition(
                            new Condition(new Attribute("m" + to.Id, A.i).Dot(A.isDissolved)));
                        if (c == null)
                        {
                            c = cdis;
                        }
                        else
                        {
                            c = new CompositeCondition(PromelaLogicalOperator.AND, c, cdis);
                        }
                    }

                    ifs.Branch(new Branch(c, new Assignment(A.tar, A.i)));
                    ifs.Else(Instruction.Skip());
                    mbr.Actions.Add(ifs);
                }
                else
                {
                    mbr.Actions.Add(new Assignment(A.tar, A.i));
                }
            }

            IfStatement kif = new IfStatement();
            Attribute ssize = null;
            if (Params.LinksEnabled)
            {
                ssize = new Attribute("m" + from.Id, A.k).Dot(new Attribute("c" + to.Id + "LSize - 1"));
            }
            else
            {
                ssize = new Attribute("m" + to.Id + "Size - 1");
            }
            Branch kbr = new Branch(new RelCondition(PromelaRelOperator.LT, A.i, ssize));
            kif.Branch(kbr);

            IfStatement kif2 = new IfStatement();
            kif2.Branch(new Branch(
                new RelCondition(PromelaRelOperator.EQ, A.tar, A._one),
                Instruction.Increment(A.i)));

            IfStatement kif3 = new IfStatement();
            kif3.Branch(new Branch(Instruction.Increment(A.i)));
            kif3.Branch(new Branch(new Assignment(A.ks, A.zero)));
            kif2.Else(kif3);
            kbr.Actions.Add(kif2);

            kif.Else(new Assignment(A.ks, A.zero));
            mbr.Actions.Add(kif);

            doKeepSearching.Else(Instruction.Break());


            return inline;
        }

        private Inline copyInstanceInline(MTypeMeta src, MTypeMeta dest)
        {
            Attribute k1 = new Attribute("k1");
            Attribute k2 = new Attribute("k2");
            Inline inline = new Inline("copyInstance" + src.Id + dest.Id, k1.Name, k2.Name);


            foreach (Symbol x in src.Symbols)
            {
                Symbol y = dest.Symbol(x.Name);
                //there are situations where the parent type does not contain the symbol in its alphabet
                if (y != null)
                {
                    inline.NestedStatements.Add(new Assignment(
                        new Attribute("m" + dest.Id, k2).Dot("x", y.Id),
                        new Attribute("m" + src.Id, k1).Dot("x", x.Id)));
                }
            }

            return inline;
        }

        public KpSystemConfiguration TranslateConfig(string execTrace)
        {
            currentTrace = execTrace;
            KpSystemConfiguration config = new KpSystemConfiguration();
            config.KPsystem = new KPsystem();
            readTrace(config);

            return config;
        }

        // example trace: #0{0{0{0{0 0}1{ 0 1 1 0 2 0 3 0 4 0 5 0 6 0 7 0 8 0}}1{0{0{0 0}1{ 0 1 1 0 2 0 3 0}}}
        private unsafe void readTrace(KpSystemConfiguration config)
        {
            if (currentTrace == null)
            {
                throw new ArgumentNullException("Trace cannot be null");
            }
            KPsystem kp = config.KPsystem;

            fixed (char* trace = currentTrace)
            {
                char* x = trace;
                if (*x == '#')
                {
                    x++;
                }

                config.Step = readInt(x, out x);
                parseChar('{', x, out x);

                bool keepParsingTypes = true;
                while (keepParsingTypes)
                {
                    int tId = readInt(x, out x);
                    MTypeMeta mt = kmm.GetTypeMetaById(tId);
                    MType type = new MType(mt.MType.Name, mt.MType.Description);
                    kp.AddType(type);
                    type.Id = tId;

                    parseChar('{', x, out x);

                    bool keepParsingInstances = true;
                    while (keepParsingInstances)
                    {
                        if (peekChar('}', x))
                        {
                            keepParsingInstances = false;
                            continue;
                        }
                        int iId = readInt(x, out x);
                        MInstance mi = new MInstance();
                        type.Instances.Add(mi);
                        mi.Id = iId;
                        MInstanceMeta mim = null;
                        if (mt.MInstanceMeta.TryGetValue(iId, out mim))
                        {
                            mi.Name = mim.MInstance.Name;
                            mi.Description = mim.MInstance.Description;
                        }
                        else
                        {
                            mi.IsCreated = true;
                        }

                        parseChar('{', x, out x);

                        int segment0 = readInt(x, out x);
                        if (segment0 == 0)
                        {
                            parseChar('{', x, out x);
                            int isDissolved = readInt(x, out x);
                            int isDivided = readInt(x, out x);
                            mi.IsDissolved = isDissolved == 0 ? false : true;
                            mi.IsDivided = isDivided == 0 ? false : true;
                            parseChar('}', x, out x);
                        }

                        int segment1 = readInt(x, out x);
                        if (segment1 == 1)
                        {
                            parseChar('{', x, out x);
                            bool keepParsingMultiset = true;
                            while (keepParsingMultiset)
                            {
                                if (peekChar('}', x))
                                {
                                    keepParsingMultiset = false;
                                    continue;
                                }
                                int symbolId = readInt(x, out x);
                                int multiplicity = readInt(x, out x);
                                Symbol obj = mt.Symbol(symbolId);
                                mi.Multiset.Add(obj.Name, multiplicity);
                            }
                            parseChar('}', x, out x);
                        }

                        //instance closed
                        parseChar('}', x, out x);
                    }

                    parseChar('}', x, out x);
                    if (peekChar('}', x))
                    {
                        keepParsingTypes = false;
                    }
                }

                //last brace which encompasses the step data
                parseChar('}', x, out x);
            }
        }

        public KpRuleApplcation TranslateRuleAppliaction(string trace)
        {
            return readRuleApplication(trace);
        }

        private unsafe KpRuleApplcation readRuleApplication(string trace)
        {
            KpRuleApplcation rApp = new KpRuleApplcation();
            fixed (char* input = trace)
            {
                char* x = input;

                parseChar('@', x, out x);
                int rId = readInt(x, out x);
                int typeMetaId = readInt(x, out x);
                int instanceId = readInt(x, out x);
                MTypeMeta mtm = kmm.GetTypeMetaById(typeMetaId);
                rApp.MType = mtm.MType;
                rApp.Rule = mtm.RuleMeta[rId].Rule;
                rApp.Rule.Id = rId;
                MInstance instance = new MInstance();
                MInstanceMeta im = null;
                if (mtm.MInstanceMeta.TryGetValue(instanceId, out im))
                {
                    instance.Name = im.MInstance.Name;
                    instance.Description = im.MInstance.Description;
                }
                instance.Id = instanceId;
                rApp.Instance = instance;
            }

            return rApp;
        }

        public KpTargetSelection TranslateTargetSelection(string trace)
        {
            return readTargetSelection(trace);
        }

        private List<string> TranslateProperties(IEnumerable<ILtlProperty> properties)
        {
            var propertyIndex = 1;
            var propertyList = new List<String>();

            foreach (var property in properties)
            {
                var translatedProperty = PropertyTranslationManager.Instance.Translate(property, kmm, ModelCheckingTarget.Promela);
                propertyList.Add(String.Format("ltl prop{0} {{ {1} }}", propertyIndex++, translatedProperty));
            }

            return propertyList;
        }

        private unsafe KpTargetSelection readTargetSelection(string trace)
        {
            KpTargetSelection kts = new KpTargetSelection();
            fixed (char* input = trace)
            {
                char* x = input;

                parseChar('$', x, out x);
                int rId = readInt(x, out x);
                int typeMetaId = readInt(x, out x);
                int instanceId = readInt(x, out x);
                MTypeMeta mtm = kmm.GetTypeMetaById(typeMetaId);
                kts.MType = mtm.MType;
                kts.Rule = mtm.RuleMeta[rId].Rule;
                kts.Rule.Id = rId;
                MInstance instance = new MInstance();
                MInstanceMeta im = null;
                if (mtm.MInstanceMeta.TryGetValue(instanceId, out im))
                {
                    instance.Name = im.MInstance.Name;
                    instance.Description = im.MInstance.Description;
                }
                instance.Id = instanceId;
                kts.Instance = instance;

                while (peekDigit(x))
                {
                    int tId = readInt(x, out x);
                    int tar = readInt(x, out x);

                    mtm = kmm.GetTypeMetaById(tId);
                    instance = new MInstance();
                    if (mtm.MInstanceMeta.TryGetValue(tar, out im))
                    {
                        instance.Name = im.MInstance.Name;
                        instance.Description = im.MInstance.Description;
                    }
                    instance.Id = tar;
                    kts.Target.Add(instance, mtm.MType);
                }
            }

            return kts;
        }

        private unsafe bool peekChar(char expected, char* src)
        {
            src = skipSpace(src);
            if (*src == expected)
            {
                return true;
            }

            return false;
        }

        private unsafe bool peekDigit(char* src)
        {
            src = skipSpace(src);
            if (isDigit(*src))
            {
                return true;
            }

            return false;
        }

        private unsafe void parseChar(char c, char* src, out char* rest)
        {
            rest = src;

            src = skipSpace(src);
            if (*src == c)
            {
                src++;
            }
            else
            {
                throw new InvalidTraceException(currentTrace);
            }

            rest = src;
        }

        private unsafe char* skipSpace(char* input)
        {
            while (isSpaceChar(*input))
            {
                input++;
            }
            return input;
        }

        private unsafe bool isSpaceChar(char x)
        {
            return x == ' ' || x == '\r' || x == '\n' || x == '\t';
        }

        private unsafe bool isDigit(char x)
        {
            return x >= '0' && x <= '9';
        }

        private unsafe int readInt(char* x, out char* rest)
        {
            string m = new string(x);
            x = skipSpace(x);
            int i = 0;
            char* y = x;
            while (isDigit(*x))
            {
                x++;
                i++;
            }

            rest = x;

            if (i > 0)
            {
                return Int32.Parse(new String(y, 0, i));
            }

            throw new InvalidTraceException(currentTrace);
        }
    }

    public class InvalidTraceException : Exception
    {
        public InvalidTraceException(string trace)
            : base("Invalid trace format: '" + trace + "'")
        {
        }
    }
}
