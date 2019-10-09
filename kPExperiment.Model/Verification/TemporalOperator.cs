using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Model.Verification
{
    public enum TemporalOperator
    {
        Always,
        Eventually,
        Next,
        Never,
        InfinitelyOften,
        SteadyState,
        Until,
        WeakUntil,
        FollowedBy,
        PrecededBy,
    }
}
