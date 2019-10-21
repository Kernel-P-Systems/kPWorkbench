using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {
    public enum RuleType {
        CONSUMER, MULTISET_REWRITING, REWRITE_COMMUNICATION,
        MEMBRANE_DIVISION, MEMBRANE_CREATION, MEMBRANE_DISSOLUTION,
        LINK_CREATION, LINK_DESTRUCTION
    }

    public abstract class Rule : PItem {

        protected RuleType type;
        private IGuard guard;

        public RuleType Type { get { return type; } }
        public bool IsGuarded { get { return !(guard == null); } }

        public IGuard Guard { get { return guard; } set { guard = value; } }

        public Rule(RuleType t) {
            this.type = t;
        }

        public Rule(RuleType t, string name)
            : this(t) {
            Name = name;
        }

        public Rule(RuleType t, string name, string description)
            : this(t, name) {
            Description = description;
        }

        public void RemoveGuard() {
            Guard = null;
        }

        public virtual bool IsStructureChangingRule() {
            return type == RuleType.MEMBRANE_DIVISION || type == RuleType.MEMBRANE_DISSOLUTION || type == RuleType.LINK_CREATION
                || type == RuleType.LINK_DESTRUCTION;
        }

        public virtual bool IsIndependentInGroup(IEnumerable<Rule> rules) {
            return false;
        }

        public abstract bool IsEmpty();
        /// <summary>
        /// Tests whether this Rule is applicable to an instance of the specified type belonging to the kP system kp.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        /// <param name="kp"></param>
        /// <returns></returns>
        public virtual bool IsApplicable(MInstance instance, MType type, KPsystem kp) {
            return IsGuarded ? Guard.IsSatisfiedBy(instance.Multiset) : true;
        }
    }

    public class ConsumerRule : Rule {
        protected Multiset lhs;
        public Multiset Lhs { get { return lhs; } }

        public ConsumerRule()
            : this(new Multiset()) {
        }

        public ConsumerRule(Multiset ms)
            : base(RuleType.CONSUMER) {
            lhs = ms;
        }

        public override bool IsEmpty() {
            return lhs.IsEmpty();
        }

        public override bool IsApplicable(MInstance instance, MType type, KPsystem kp) {
            return base.IsApplicable(instance, type, kp) && instance.Multiset >= lhs;
        }

        public override bool IsIndependentInGroup(IEnumerable<Rule> rules) {
            foreach (Rule r in rules) {
                if (!ReferenceEquals(r, this)) {
                    if (r is ConsumerRule) {
                        if (!(r as ConsumerRule).Lhs.IsDisjointFrom(Lhs)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        } 
    }

    public class RewritingRule : ConsumerRule {

        protected Multiset rhs;
        public Multiset Rhs { get { return rhs; } }

        public RewritingRule()
            : base() {
            type = RuleType.MULTISET_REWRITING;
            rhs = new Multiset();
        }

        public void AddRhs(Multiset ms) {
            rhs.Add(ms);
        }

        public override bool IsEmpty() {
            return base.IsEmpty() && rhs.IsEmpty();
        }
    }

    public class RewriteCommunicationRule : RewritingRule {
        protected Dictionary<IInstanceIdentifier, TargetedMultiset> targetRhs;
        public Dictionary<IInstanceIdentifier, TargetedMultiset> TargetRhs { get { return targetRhs; } }

        public RewriteCommunicationRule()
            : base() {
                targetRhs = new Dictionary<IInstanceIdentifier, TargetedMultiset>();
            type = RuleType.REWRITE_COMMUNICATION;
        }

        public void AddRhs(Dictionary<IInstanceIdentifier, TargetedMultiset> src) {
            foreach (KeyValuePair<IInstanceIdentifier, TargetedMultiset> kv in src) {
                TargetedMultiset tm = null;
                targetRhs.TryGetValue(kv.Key, out tm);
                if (tm == null) {
                    targetRhs.Add(kv.Key, kv.Value);
                } else {
                    tm.Multiset.Add(kv.Value.Multiset);
                }
            }
        }

        public override bool IsEmpty() {
            if (base.IsEmpty()) {
                foreach (TargetedMultiset tm in targetRhs.Values) {
                    if (!tm.IsEmpty()) {
                        return false;
                    }
                }
            } else {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This is very inneficient. Consider alternatives ...
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        /// <param name="kp"></param>
        /// <returns></returns>
        public override bool IsApplicable(MInstance instance, MType type, KPsystem kp) {
            if (!base.IsApplicable(instance, type, kp)) {
                return false;
            }
            foreach (IInstanceIdentifier identifier in TargetRhs.Keys) {
                if (identifier is InstanceIdentifier) {
                    InstanceIdentifier ii = identifier as InstanceIdentifier;
                    if (ii.Indicator == InstanceIndicator.TYPE) {
                        MType mt = kp[ii.Value];
                        bool atLeastOneConnection = false;
                        foreach (MInstance connection in instance.Connections) {
                            if (mt.Instances.Contains(connection)) {
                                atLeastOneConnection = true;
                                break;
                            }
                        }
                        if (!atLeastOneConnection) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

    public class DivisionRule : ConsumerRule {

        protected List<InstanceBlueprint> rhs;
        public List<InstanceBlueprint> Rhs { get { return rhs; } protected set { rhs = value; } }

        public DivisionRule()
            : this(new Multiset()) {
        }

        public DivisionRule(Multiset ms) : base(ms) {
            rhs = new List<InstanceBlueprint>();
            type = RuleType.MEMBRANE_DIVISION;
        }

        public override bool IsEmpty() {
            return base.IsEmpty() && rhs.Count == 0;
        }
    }

    public class LinkRule : ConsumerRule {

        protected IInstanceIdentifier target;
        public IInstanceIdentifier Target { get { return target; } set { target = value; } }

        private LinkRule(RuleType t, Multiset ms, IInstanceIdentifier tar)
            : base(ms) {
            type = t;
            Target = tar;
        }

        public static LinkRule LinkCreate(IInstanceIdentifier tar) {
            return new LinkRule(RuleType.LINK_CREATION, new Multiset(), tar);
        }

        public static LinkRule LinkCreate(Multiset ms, IInstanceIdentifier tar) {
            return new LinkRule(RuleType.LINK_CREATION, ms, tar);
        }

        public static LinkRule LinkDestroy(IInstanceIdentifier tar) {
            return new LinkRule(RuleType.LINK_DESTRUCTION, new Multiset(), tar);
        }

        public static LinkRule LinkDestroy(Multiset ms, IInstanceIdentifier tar) {
            return new LinkRule(RuleType.LINK_DESTRUCTION, ms, tar);
        }

        public override bool IsEmpty() {
            return base.IsEmpty() && target == null;
        }
    }

    public class DissolutionRule : ConsumerRule {

        public DissolutionRule()
            : base() {
            type = RuleType.MEMBRANE_DISSOLUTION;
        }

        public DissolutionRule(Multiset ms)
            : this() {
            lhs = ms;
        }

        public override bool IsEmpty() {
            return false;
        }
    }
}
