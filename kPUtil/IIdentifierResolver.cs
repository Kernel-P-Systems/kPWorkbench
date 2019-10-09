using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public interface IIdentifierResolver {

        IEnumerable<MInstance> ResolveInstances(KPsystem kp, IInstanceIdentifier identifier);
    }
}
