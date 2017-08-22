using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Json;
using WorkflowEngine.Logging.Interfaces;
using WorkflowEngine.Tests.Mocks.Config;
using WorkflowEngine.Tests.Mocks.Logging;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Mocks.WorkQueue;
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;

namespace WorkflowEngine.Tests
{
    public class UnityContainerFixture : IDisposable
    {
        public IUnityContainer Container;

        public UnityContainerFixture()
        {
            Container = new UnityContainer();
            Container.RegisterType<IPluginServices, PluginServices>();
            Container.RegisterType<IWorkQueue, MockWorkQueue>();
            Container.RegisterType<IPluginConfig, MockPluginConfig>();
            Container.RegisterType<ILogger, MockLogger>();
            Container.RegisterType<IServiceProvider, MockServiceProvider>();

            JsonUtils.SetGlobalJsonNetSettings();
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }
}
