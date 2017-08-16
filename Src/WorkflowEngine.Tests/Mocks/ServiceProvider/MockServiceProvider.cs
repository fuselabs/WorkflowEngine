using System.Collections.Generic;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine.Tests.Mocks.ServiceProvider
{
    class MockServiceProvider : IServiceProvider
    {
        private readonly IDictionary<string, object> _services = new Dictionary<string, object>();

        public void SetService(string serviceName, object service)
        {
            _services[serviceName] = service;
        }

        public object GetService(string serviceName)
        {
            return _services.ContainsKey(serviceName) ? _services[serviceName] : null;
        }
    }
}
