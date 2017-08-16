using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Json;
using WorkflowEngine.Logging.Interfaces;
using WorkflowEngine.Tests.Mocks.Config;
using WorkflowEngine.Tests.Mocks.Logging;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Mocks.WorkQueue;
using WorkflowEngine.Tests.Schemas;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class PluginVersioningTests
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

            JsonUtils.SetGlobalJsonNetSettings();
        }

        [TestMethod]
        public async ThreadingTask Versioning_CompatiblePluginsSameVersion()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.ExternalAnswerWorkflow, Workflows.ExternalTask.V1Prime.ExternalAnswerWorkflow>(true);
        }

        [TestMethod]
        public async ThreadingTask Versioning_CompatiblePluginsDifferentMinorVersion()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.ExternalAnswerWorkflow, Workflows.ExternalTask.V15.ExternalAnswerWorkflow>(true);
        }


        [TestMethod]
        public async ThreadingTask Versioning_InompatiblePluginsDifferentMajorVersion_FirstOlder()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.ExternalAnswerWorkflow, Workflows.ExternalTask.V2.ExternalAnswerWorkflow>(false);
        }

        [TestMethod]
        public async ThreadingTask Versioning_InompatiblePluginsDifferentMajorVersion_FirstNewer()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V2.ExternalAnswerWorkflow, Workflows.ExternalTask.V1.ExternalAnswerWorkflow>(false);
        }

        [TestMethod]
        public async ThreadingTask Versioning_IncompatiblePluginsDifferentMinorVersion_FirstNewer()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V15.ExternalAnswerWorkflow, Workflows.ExternalTask.V1.ExternalAnswerWorkflow>(false);
        }


        private static async ThreadingTask RunPluginVersioningTest<TFirstPlugin, TSecondPlugin>(bool shouldSucceed)
            where TSecondPlugin : Workflow, new()
            where TFirstPlugin : Workflow, new()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<TFirstPlugin, ExternalDataHandlerTaskOutput>, WorkflowContainer<TFirstPlugin, ExternalDataHandlerTaskOutput>>();
            _container.RegisterType<IWorkflowContainer<TSecondPlugin, ExternalDataHandlerTaskOutput>, WorkflowContainer<TSecondPlugin, ExternalDataHandlerTaskOutput>>();
            var firstPluginContainer = _container.Resolve<IWorkflowContainer<TFirstPlugin, ExternalDataHandlerTaskOutput>>();
            firstPluginContainer.Initialize();

            var workflowInput = firstPluginContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await firstPluginContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = firstPluginContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = firstPluginContainer.Store();

            var secondPluginContainer = _container.Resolve<IWorkflowContainer<TSecondPlugin, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = secondPluginContainer.TryLoad(serializedWorkflowContext);
            Assert.IsTrue(loadSucceeded);

            // Second execution. External dependencies will be resolved
            result = await secondPluginContainer.ReExecute();
            workflowOutput = secondPluginContainer.GetOutput();
            if (shouldSucceed)
            {
                Assert.IsNotNull(workflowOutput);
                Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
            }
            else
            {
                Assert.IsNull(workflowOutput);
                Assert.IsTrue(result == WorkflowContainerExecutionResult.NotExecuted);
            }
        }

        private static IServiceProvider GetServiceProvider()
        {
            var externalService = new ExternalService();
            externalService.SetResult("4", 1);
            var serviceProvider = new MockServiceProvider();
            serviceProvider.SetService("ExternalService", externalService);
            return serviceProvider;
        }

    }
}
