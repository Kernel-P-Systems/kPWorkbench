using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpLingua.Input
{
    public class KpLinguaModelBuilder : IKpLinguaVisitor<object>
    {
        private KPsystem _kPsystem;
        private Dictionary<string, List<MInstance>> _instancesByType;
        private Dictionary<string, MInstance> _instancesByName; 
        private string _currentTypeName;

        public KpLinguaModelBuilder()
        {
            _kPsystem = new KPsystem();
            _instancesByType = new Dictionary<string, List<MInstance>>();
            _instancesByName = new Dictionary<string, MInstance>();
        }

        public KPsystem GetKpSystem(KpLinguaParser.KPsystemContext context)
        {
            return VisitKPsystem(context) as KPsystem;
        }

        public object VisitKPsystem(KpLinguaParser.KPsystemContext context)
        {
            foreach (var statementContext in context.statement())
            {
                statementContext.Accept(this);
            }

            return _kPsystem;
        }

        public object VisitStatement(KpLinguaParser.StatementContext context)
        {
            VisitChildren(context);

            return null;
        }

        public object VisitTypeDefinition(KpLinguaParser.TypeDefinitionContext context)
        {
            var identifierContext = context.Identifier();
            var ruleSetContext = context.ruleSet();
            _currentTypeName = GetIdentifier(identifierContext);

            if (_kPsystem.TryGetType(_currentTypeName) != null)
            {
                throw new KpLinguaSemanticException(string.Format("A membrane type with name '{0}' has already been defined", _currentTypeName), identifierContext.Symbol.Line, identifierContext.Symbol.Column);
            }

            var type = new MType(_currentTypeName);
            
            _kPsystem.AddType(type);
            
            if (ruleSetContext != null)
            {
                type.ExecutionStrategy = ruleSetContext.Accept(this) as ExecutionStrategy;
            }

            return type;
        }

        public object VisitRuleSet(KpLinguaParser.RuleSetContext context)
        {
            var executionStrategyContexts = context.executionStrategy();
            var executionStrategy = executionStrategyContexts.First().Accept(this) as ExecutionStrategy;
            var firstExecutionStrategy = executionStrategy;

            foreach (var executionStrategyContext in executionStrategyContexts.Skip(1)) 
            {
                executionStrategy.Next = executionStrategyContext.Accept(this) as ExecutionStrategy;

                executionStrategy = executionStrategy.Next;
            }

            return firstExecutionStrategy;
        }

        public object VisitExecutionStrategy(KpLinguaParser.ExecutionStrategyContext context)
        {
            return context.children.First().Accept(this);
        }

        public object VisitSequenceExecutionStrategy(KpLinguaParser.SequenceExecutionStrategyContext context)
        {
            var rules = context.rule().Select(r => r.Accept(this) as Rule).ToArray();
            return new ExecutionStrategy(StrategyOperator.SEQUENCE, rules);
        }

        public object VisitMaxExecutionStrategy(KpLinguaParser.MaxExecutionStrategyContext context)
        {
            var executionStrategy = default(ExecutionStrategy);
            var ruleContext = context.rule();

            if (ruleContext != null)
            {
                var rules = ruleContext.Select(r => r.Accept(this) as Rule).ToArray();
                executionStrategy = new ExecutionStrategy(StrategyOperator.MAX, rules);
            }
            else
            {
                executionStrategy = new ExecutionStrategy(StrategyOperator.MAX);
            }

            return executionStrategy;
        }

        public object VisitChoiceExecutionStrategy(KpLinguaParser.ChoiceExecutionStrategyContext context)
        {
            var executionStrategy = default(ExecutionStrategy);
            var ruleContext = context.rule();

            if (ruleContext != null)
            {
                var rules = ruleContext.Select(r => r.Accept(this) as Rule).ToArray();
                executionStrategy = new ExecutionStrategy(StrategyOperator.CHOICE, rules);
            }
            else
            {
                executionStrategy = new ExecutionStrategy(StrategyOperator.CHOICE);
            }

            return executionStrategy;
        }

        public object VisitArbitraryExecutionStrategy(KpLinguaParser.ArbitraryExecutionStrategyContext context)
        {
            var executionStrategy = default(ExecutionStrategy);
            var ruleContext = context.rule();

            if (ruleContext != null)
            {
                var rules = ruleContext.Select(r => r.Accept(this) as Rule).ToArray();
                executionStrategy = new ExecutionStrategy(StrategyOperator.ARBITRARY, rules);
            }
            else
            {
                executionStrategy = new ExecutionStrategy(StrategyOperator.ARBITRARY);
            }

            return executionStrategy;
        }

        public object VisitRule(KpLinguaParser.RuleContext context)
        {
            var rule = context.ruleRightHandSide().Accept(this) as ConsumerRule;

            var guardContexet = context.guard();
            var multisetContext = context.nonEmptyMultiset();

            if (guardContexet != null)
            {
                rule.Guard = guardContexet.Accept(this) as IGuard;
            }

            rule.Lhs.Add(multisetContext.Accept(this) as Multiset);

            return rule;
        }

        public object VisitRuleRightHandSide(KpLinguaParser.RuleRightHandSideContext context)
        {
            var rule = default(ConsumerRule);

            var ruleMultisetContexts = context.ruleMultiset();
            var divisionContexts = context.division();
            var dissolutionContext = context.dissolution();
            var linkCreationContext = context.linkCreation();
            var linkDestructionContext = context.linkDestruction();

            if (ruleMultisetContexts != null)
            {
                var rewritingRule = default(RewritingRule);

                if (ruleMultisetContexts.Any(rm => rm.targetedMultiset() != null))
                {
                    var rewriteCommunicationRule = new RewriteCommunicationRule();
                    var targetedMultisets = ruleMultisetContexts.Where(rm => rm.targetedMultiset() != null).Select(rm => rm.targetedMultiset().Accept(this) as TargetedMultiset);
                    rewriteCommunicationRule.AddRhs(targetedMultisets.ToDictionary(tm => tm.Target));

                    rewritingRule = rewriteCommunicationRule;
                }
                else
                {
                    rewritingRule = new RewritingRule();
                }

                var multisets = ruleMultisetContexts.Where(rm => rm.nonEmptyMultiset() != null).Select(rm => rm.nonEmptyMultiset().Accept(this) as Multiset);
                foreach (var multiset in multisets)
                {
                    rewritingRule.AddRhs(multiset);
                }
                
                rule = rewritingRule;
            }
            else if (divisionContexts != null)
            {
                var divisionRule = new DivisionRule();

                foreach(var divisionContext in divisionContexts) 
                {
                    divisionRule.Rhs.Add(divisionContext.Accept(this) as InstanceBlueprint);
                }

                rule = divisionRule;
            }
            else if (dissolutionContext != null)
            {
                rule = dissolutionContext.Accept(this) as DissolutionRule;
            }
            else if (linkCreationContext != null)
            {

            }
            else if (linkDestructionContext != null)
            {

            }

            return rule;
        }

        public object VisitDivision(KpLinguaParser.DivisionContext context)
        {
            var instanceBlueprint = default(InstanceBlueprint);

            var typeReferenceContext = context.typeReference();
            var multisetContext = context.nonEmptyMultiset();
            var type = typeReferenceContext != null ? typeReferenceContext.Accept(this) as MType : _kPsystem[_currentTypeName];

            if (multisetContext != null)
            {
                instanceBlueprint = new InstanceBlueprint(type, multisetContext.Accept(this) as Multiset);
            }
            else
            {
                instanceBlueprint = new InstanceBlueprint(type);
            }

            return instanceBlueprint;
        }

        public object VisitDissolution(KpLinguaParser.DissolutionContext context)
        {
            return new DissolutionRule();
        }

        public object VisitLinkCreation(KpLinguaParser.LinkCreationContext context)
        {
            return LinkRule.LinkCreate(new InstanceIdentifier(context.typeReference().Accept(this) as string));
        }

        public object VisitGuard(KpLinguaParser.GuardContext context)
        {
            var guardOperands = context.andGuardExpression().Select(c => c.Accept(this) as IGuard).Reverse().ToArray();

            return GetGuard(guardOperands, BinaryGuardOperator.OR);
        }

        public object VisitAndGuardExpression(KpLinguaParser.AndGuardExpressionContext context)
        {
            var guardOperands = context.guardOperand().Select(c => c.Accept(this) as IGuard).Reverse().ToArray();

            return GetGuard(guardOperands, BinaryGuardOperator.AND);
        }

        public object VisitGuardOperand(KpLinguaParser.GuardOperandContext context)
        {
            var guard = default(IGuard);

            var multisetContext = context.nonEmptyMultiset();
            var guardContext = context.guard();
            var relationalOperatorContext = context.RelationalOperator();

            if (multisetContext != null)
            {
                var multiset = multisetContext.Accept(this) as Multiset;
                var relationalOperator = relationalOperatorContext != null ? GetRelationalOperator(relationalOperatorContext) : RelationalOperator.EQUAL;

                guard = new BasicGuard(multiset, relationalOperator);
            }
            else if (guardContext != null)
            {
                guard = guardContext.Accept(this) as IGuard;
            }

            return guard;
        }

        public object VisitLinkDestruction(KpLinguaParser.LinkDestructionContext context)
        {
            return LinkRule.LinkDestroy(new InstanceIdentifier(context.typeReference().Accept(this) as string));
        }

        public object VisitNonEmptyMultiset(KpLinguaParser.NonEmptyMultisetContext context)
        {
            var multiset = new Multiset();

            foreach (var multisetAtomContext in context.multisetAtom())
            {
                multiset.Add(multisetAtomContext.Accept(this) as Multiset);
            }

            return multiset;
        }

        public object VisitMultisetAtom(KpLinguaParser.MultisetAtomContext context)
        {
            var multiset = default(Multiset);

            var multiplicityContext = context.Multiplicity();
            var identifier = GetIdentifier(context.Identifier());

            if (multiplicityContext != null)
            {
                multiset = new Multiset(identifier, GetMultiplicity(multiplicityContext));
            }
            else
            {
                multiset = new Multiset(identifier);
            }

            return multiset;
        }

        public object VisitTargetedMultiset(KpLinguaParser.TargetedMultisetContext context)
        {
            var targetedMultiset = default(TargetedMultiset);
            var instanceIdentifier = new InstanceIdentifier(context.typeReference().Accept(this) as string);

            var multisetAtomContext = context.multisetAtom();

            if (multisetAtomContext != null)
            {
                targetedMultiset = new TargetedMultiset(instanceIdentifier, multisetAtomContext.Accept(this) as Multiset);
            }
            else
            {
                targetedMultiset = new TargetedMultiset(instanceIdentifier, context.nonEmptyMultiset().Accept(this) as Multiset);
            }

            return targetedMultiset;
        }

        public object VisitTypeReference(KpLinguaParser.TypeReferenceContext context)
        {
            var identifier = GetIdentifier(context.Identifier());

            return identifier;
        }

        public object VisitInstance(KpLinguaParser.InstanceContext context)
        {
            var instance = new MInstance();
            var typeName = context.typeReference().Accept(this) as string;
            var identifierContext = context.Identifier();
            var multisetContext = context.nonEmptyMultiset();

            if (identifierContext != null)
            {
                instance.Name = GetIdentifier(identifierContext);

                if (_instancesByName.ContainsKey(instance.Name))
                {
                    throw new KpLinguaSemanticException(string.Format("A membrane instance with name '{0}' has already been defined", instance.Name), identifierContext.Symbol.Line, identifierContext.Symbol.Column);
                }
            }

            if(multisetContext != null) 
            {
                instance.ReplaceMultiset(multisetContext.Accept(this) as Multiset);
            }

            RegisterMInstance(instance, typeName);

            return instance;
        }

        public object VisitInstantiation(KpLinguaParser.InstantiationContext context)
        {
            var instanceContexts = context.instance();

            foreach (var instanceContext in instanceContexts)
            {
                instanceContext.Accept(this);
            }

            return null;
        }

        public object VisitLink(KpLinguaParser.LinkContext context)
        {
            var linkOperandContexts = context.linkOperand();
            var instanceSets = linkOperandContexts.Select(c => GetInstances(c)).ToArray();

            var firstInstanceSet = instanceSets.First();

            foreach (var secondInstanceSet in instanceSets.Skip(1))
            {
                PerformLinkage(firstInstanceSet, secondInstanceSet);

                firstInstanceSet = secondInstanceSet;
            }

            return null;
        }

        public object VisitLinkWildcardOperand(KpLinguaParser.LinkWildcardOperandContext context)
        {
            return context.typeReference().Accept(this);
        }

        #region Not Implemented Methods

        public object VisitChildren(IRuleNode node)
        {
            throw new NotImplementedException();
        }

        public object VisitErrorNode(IErrorNode node)
        {
            throw new Exception(node.ToString());
        }

        public object VisitTerminal(ITerminalNode node)
        {
            throw new NotImplementedException();
        }

        public object Visit(IParseTree tree)
        {
            throw new NotImplementedException();
        }

        public object VisitEmptyMultiset(KpLinguaParser.EmptyMultisetContext context)
        {
            throw new NotImplementedException();
        }

        public object VisitRuleMultiset(KpLinguaParser.RuleMultisetContext context)
        {
            throw new NotImplementedException();
        }

        public object VisitLinkOperand(KpLinguaParser.LinkOperandContext context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Methods

        private void VisitChildren(ParserRuleContext context)
        {
            foreach (var child in context.children)
            {
                child.Accept(this);
            }
        }

        private string GetIdentifier(ITerminalNode node)
        {
            return node.Symbol.Text;
        }

        private int GetMultiplicity(ITerminalNode node)
        {
            return int.Parse(node.Symbol.Text);
        }

        private void RegisterMInstance(MInstance instance, string typeName)
        {
            _kPsystem[typeName].Instances.Add(instance);

            if (!string.IsNullOrEmpty(instance.Name))
            {
                _instancesByName.Add(instance.Name, instance);
            }

            var registeredInstances = default(List<MInstance>);
            if (!_instancesByType.TryGetValue(typeName, out registeredInstances))
            {
                _instancesByType.Add(typeName, registeredInstances = new List<MInstance>());
            }
            registeredInstances.Add(instance);
        }

        private void PerformLinkage(IReadOnlyList<MInstance> firstInstanceSet, IReadOnlyList<MInstance> secondInstanceSet)
        {
            foreach (var firstInstance in firstInstanceSet)
            {
                foreach (var secondInstance in secondInstanceSet)
                {
                    firstInstance.ConnectBidirectionalTo(secondInstance);
                }
            }
        }

        private IReadOnlyList<MInstance> GetInstances(KpLinguaParser.LinkOperandContext context)
        {
            var instances = new List<MInstance>();

            var instanceContext = context.instance();
            var identifierContext = context.Identifier();
            var wildcardContext = context.linkWildcardOperand();

            if (instanceContext != null)
            {
                instances.Add(instanceContext.Accept(this) as MInstance);
            }

            if (identifierContext != null)
            {
                var instance = default(MInstance);
                var instanceName = GetIdentifier(identifierContext);

                if (_instancesByName.TryGetValue(instanceName, out instance))
                {
                    instances.Add(instance);
                }
            }

            if (wildcardContext != null)
            {
                var typeName = wildcardContext.Accept(this) as string;
                var instancesOfType = default(List<MInstance>);

                if (_instancesByType.TryGetValue(typeName, out instancesOfType))
                {
                    instances.AddRange(instancesOfType);
                }
            }

            return instances;
        }

        private IGuard GetGuard(IGuard[] guardOperands, BinaryGuardOperator op)
        {
            var guard = default(IGuard);

            if (guardOperands.Length == 1)
            {
                guard = guardOperands[0];
            }
            else
            {
                var compoundGuard = new CompoundGuard(op, guardOperands[1], guardOperands[0]);

                foreach (var guardOperand in guardOperands.Skip(2))
                {
                    compoundGuard = new CompoundGuard(op, guardOperand, compoundGuard);
                }

                guard = compoundGuard;
            }

            return guard;
        }

        private RelationalOperator GetRelationalOperator(ITerminalNode node)
        {
            switch (node.Symbol.Text)
            {
                case ">=": return RelationalOperator.GEQ;
                case "<=": return RelationalOperator.LEQ;
                case ">": return RelationalOperator.GT;
                case "<": return RelationalOperator.LT;
                case "=": return RelationalOperator.EQUAL;
                case "!" : return RelationalOperator.NOT_EQUAL;
            }

            return default(RelationalOperator);
        }

        #endregion
    }
}
