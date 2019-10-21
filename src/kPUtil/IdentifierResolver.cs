using KpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpUtil {
    public class IdentifierResolver : IIdentifierResolver {

        public IEnumerable<MInstance> ResolveInstances(KPsystem kp, IInstanceIdentifier identifier) {
            List<MInstance> instances = new List<MInstance>();
            if (identifier is InstanceIdentifier) {
                InstanceIdentifier ident = identifier as InstanceIdentifier;
                if (ident.Indicator == InstanceIndicator.TYPE) {
                    foreach (MType type in kp.Types) {
                        if (type.Name == ident.Value) {
                            instances.AddRange(type.Instances);
                        }
                    }
                }
            }
            //solve if instance identifier is complex (i.e. includes more than one basic identifier)

            return instances;
        }
    }
}
