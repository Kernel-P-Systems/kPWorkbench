using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace SpikingNeuralPreprocessing.SnpToKpsTranslators;

internal interface ISnpVariantStrategy
{
    void GenerateTypeDefinition(StringBuilder sb, string kplTypeId, string xmlTypeId,
        Dictionary<string, IEnumerable<XElement>> typeRulesMap,
        Dictionary<string, int> typeThresholdMap,
        List<(string Target, int Weight)> instanceTargets,
        Func<XElement, IEnumerable<(string Target, int Weight)>, bool, string> ruleGeneratorFunc);
}