using KpCore;
using KpExperiment.Model;
using KpExperiment.Model.Verification;
using KpExperiment.Verification.Runtime;
using KpExperiment.Verification.Translation.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelChecking.Runtime
{
    public class ModelCheckingManager
    {
        private static Lazy<ModelCheckingManager> _instance = new Lazy<ModelCheckingManager>(() => new ModelCheckingManager());

        public static ModelCheckingManager Instance
        {
            get { return _instance.Value; }
        }
        public async Task Verify(FileInfo kplModelFile, IEnumerable<IProperty> properties, FileInfo verificationModelFile, ModelCheckingTarget target, IModelCheckingProgressMonitor monitor)
        {
            var executor = GetExecutor(target);
            await executor.Verify(kplModelFile, properties, verificationModelFile, monitor);
        }

        private IModelCheckingExecutor GetExecutor(ModelCheckingTarget target)
        {
            var executor = default(IModelCheckingExecutor);

            switch (target)
            {
                case ModelCheckingTarget.Promela: executor = new SpinExecutor(); break;
                case ModelCheckingTarget.NuSmv: executor = new NuSmvExecutor(); break;
            }

            return executor;
        }
    }
}
