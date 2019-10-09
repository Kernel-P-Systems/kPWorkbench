using KpExperiment.Model.Verification;
using KpExperiment.Verification.Translation.Base;
using KpUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KpExperiment.Verification.Translation
{
    public class PropertyTranslationManager
    {
        private static readonly Lazy<PropertyTranslationManager> _instance = new Lazy<PropertyTranslationManager>(() => new PropertyTranslationManager());

        private PropertyTranslationManager() { }

        public static PropertyTranslationManager Instance
        {
            get { return _instance.Value; }
        }

        public string Translate(ILtlProperty ltlProperty, KpMetaModel kpMetaModel, ModelCheckingTarget target)
        {
            var propertyTranslator = default(ILtlPropertyTranslator);

            switch (target)
            {
                case ModelCheckingTarget.Promela: propertyTranslator = new PromelaLtlTranslator(kpMetaModel); break;
                case ModelCheckingTarget.NuSmv: propertyTranslator = new NuSmvLtlTranslator(kpMetaModel); break;
            }

            return ltlProperty.Accept(propertyTranslator);
        }

        public string Translate(ICtlProperty ctlProperty, KpMetaModel kpMetaModel, ModelCheckingTarget target)
        {
            var propertyTranslator = default(ICtlPropertyTranslator);

            switch (target)
            {
                case ModelCheckingTarget.NuSmv: propertyTranslator = new NuSmvCtlTranslator(kpMetaModel); break;
            }

            return ctlProperty.Accept(propertyTranslator);
        }
    }
}
