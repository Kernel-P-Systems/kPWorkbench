using System.Collections.Generic;

namespace KPLinguaPreprocessing
{
    internal class Rule : IParsingComponent
    {
        private readonly List<IParsingComponent> _components = new List<IParsingComponent>();

        public void Add(IParsingComponent component)
        {
            _components.Add(component);
        }

        public string Process(string rulesWithParameters)
        {
            string result = rulesWithParameters;
            for (int index = 0; index < _components.Count; index++)
            {
                string value = _components[index].Process(string.Empty);
                result = result.Replace($"@{index}@", value);
            }
            return result;
        }
    }
}
