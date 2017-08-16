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
    public class PlaygroundTests
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
        public async ThreadingTask WorkflowEngine_SmokeTest()
        {

            const string email = "Hi, can you please help me find out if Tom and Dan prefer Green or Blue?";
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();

            var input = pluginServices.CreatePluginData<IdentifyRequestTypeInput>();
            input.Data.Email = email;

            var identifyWorkflow = pluginServices.GetOrCreatePlugin<IdentifyRequestTypeWorkflow>();

            var output = await identifyWorkflow.Execute<IdentifyRequestTypeOutput>(new PluginInputs { { "input", input } });
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Data.Type == RequestType.Voting);
            Assert.IsTrue(pluginServices.AllPluginsExecuted());
        }

        [TestMethod]
        public async ThreadingTask WorkflowEngine_SmokeTestWithContainer()
        {
            const string email = "Hi, can you please help me find out if Tom and Dan prefer Green or Blue?";
            _container.RegisterType<IWorkflowContainer<IdentifyRequestTypeWorkflow, IdentifyRequestTypeOutput>, WorkflowContainer<IdentifyRequestTypeWorkflow, IdentifyRequestTypeOutput>>();

            var workflowContainer = _container.Resolve<IWorkflowContainer<IdentifyRequestTypeWorkflow, IdentifyRequestTypeOutput>>();
            workflowContainer.Initialize();

            var input = workflowContainer.CreateWorkflowInput<IdentifyRequestTypeInput>();
            input.Data.Email = email;

            var result = await workflowContainer.Execute(new PluginInputs { { "input", input } });
            var output = workflowContainer.GetOutput();
            Assert.IsNotNull(output);
            Assert.IsTrue(output.Data.Type == RequestType.Voting);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
        }
    }
}
