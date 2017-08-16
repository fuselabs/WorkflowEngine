using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Logging.Interfaces;
using WorkflowEngine.Tests.Mocks.Config;
using WorkflowEngine.Tests.Mocks.Logging;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Mocks.WorkQueue;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class CancellationTests
    {
        private static IUnityContainer _container;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _container = new UnityContainer();
            _container.RegisterType<IPluginServices, PluginServices>();
            _container.RegisterType<IPluginConfig, MockPluginConfig>();
            _container.RegisterType<IServiceProvider, MockServiceProvider>();
            _container.RegisterType<ILogger, MockLogger>();
            _container.RegisterType<IWorkQueue, MockWorkQueue>();
        }

        [TestMethod]
        public async ThreadingTask Cancellation_NoCancellation()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<CancellationWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<CancellationWorkflowInput>();

            var workflowOutput = await workflow.Execute<CancellableTaskOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
            Assert.IsFalse(workflowOutput.Data.Cancelled);
        }

        [TestMethod]
        public async ThreadingTask Cancellation_PluginCancellation()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<CancellationWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<CancellationWorkflowInput>();
            workflowInput.Data.CancelPlugin = true;

            var workflowOutput = await workflow.Execute<CancellableTaskOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(workflowOutput.Data.Cancelled);
        }

        [TestMethod]
        public async ThreadingTask Cancellation_ExecutionCancellation()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<CancellationWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<CancellationWorkflowInput>();
            workflowInput.Data.CancelExecution = true;

            var workflowOutput = await workflow.Execute<CancellableTaskOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(workflowOutput.Data.Cancelled);
        }
    }
}
