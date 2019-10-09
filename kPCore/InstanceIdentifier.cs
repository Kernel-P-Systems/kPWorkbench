using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpCore {


    //the subject property we evaluate when we wish to include an item in a certain scope
    //i.e type (name==val) or name (name==val)
    public enum InstanceIndicator {
        TYPE, NAME, LABEL
    }

    //operators used for the composition of complex instance identifiers = queries on instances
    public enum IIOperator {
        AND, OR
    }

    public interface IInstanceIdentifier {

    }

    /// <summary>
    /// An instance identifier is a property/value tuple which is used as a filter
    /// to describe a set of instances which satisfy the condition.
    /// </summary>
    public class InstanceIdentifier : IInstanceIdentifier {

        public InstanceIndicator Indicator { get; set; }
        public String Value { get; set; }

        public InstanceIdentifier(InstanceIndicator indicator, string val) {
            Indicator = indicator;
            Value = val;
        }

        public InstanceIdentifier(string val) {
            Indicator = InstanceIndicator.TYPE;
            Value = val;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is InstanceIdentifier) {
                InstanceIdentifier ii = obj as InstanceIdentifier;
                return ii.Indicator == this.Indicator && ii.Value == this.Value;
            }

            return false;
        }
    }

    public class CompoundInstanceIdentifier : IInstanceIdentifier {

        public IInstanceIdentifier Lhs { get; set; }
        public IInstanceIdentifier Rhs { get; set; }
        public IIOperator Operator { get; set; }

        public CompoundInstanceIdentifier(IIOperator op, IInstanceIdentifier lhs, IInstanceIdentifier rhs) {
            Operator = op;
            Lhs = lhs;
            Rhs = rhs;
        }

        public CompoundInstanceIdentifier And(CompoundInstanceIdentifier rhs) {
            return new CompoundInstanceIdentifier(IIOperator.AND, this, rhs);
        }

        public CompoundInstanceIdentifier Or(CompoundInstanceIdentifier rhs) {
            return new CompoundInstanceIdentifier(IIOperator.OR, this, rhs);
        }
    }
}
