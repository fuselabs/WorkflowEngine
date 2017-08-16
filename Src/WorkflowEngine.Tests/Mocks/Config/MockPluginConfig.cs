using System.Collections.Generic;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine.Tests.Mocks.Config
{
    public class MockPluginConfig : IPluginConfig
    {
        private readonly IDictionary<string, string> _configs;

        [InjectionConstructor]
        public MockPluginConfig() : this(new Dictionary<string, string>())
        {
        }

        public MockPluginConfig(Dictionary<string, string> configs)
        {
            _configs = configs ?? new Dictionary<string, string>();
        }
        public string Get(string configKey)
        {
            if (_configs.ContainsKey(configKey))
                return _configs[configKey];

            return string.Empty;
        }
    }
}
