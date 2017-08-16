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
using WorkflowEngine.Tests.Workflows.ExternalTask;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class SchemaVersioningTests
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
        public async ThreadingTask Versioning_BreakingSchemaChange()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            // Simulate a breaking schema change by changing the type of the Question 
            // Property from String to a nested object
            serializedWorkflowContext = serializedWorkflowContext.Replace("\"2+2\"", "{\"QuestionString\":\"2+2\"}");

            workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.IsFalse(loadSucceeded);
        }

        [TestMethod]
        public async ThreadingTask Versioning_CompatibleSchemas_SameVersion()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV1.ExternalAnswerWorkflow>(new Version(1, 0), true);
        }

        [TestMethod]
        public async ThreadingTask Versioning_CompatibleSchemas_DifferentMinorVersion_NewerSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV1.ExternalAnswerWorkflow>(new Version(1, 5), true);
        }

        [TestMethod]
        public async ThreadingTask Versioning_IncompatibleSchemas_DifferentMinorVersion_OlderSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV15.ExternalAnswerWorkflow>(new Version(1, 0), false);
        }

        [TestMethod]
        public async ThreadingTask Versioning_InompatibleSchemas_DifferentMajorVersion_NewerSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV15.ExternalAnswerWorkflow>(new Version(2, 0), false);
        }

        [TestMethod]
        public async ThreadingTask Versioning_InompatibleSchemas_DifferentMajorVersion_OlderSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV2.ExternalAnswerWorkflow>(new Version(1, 0), false);
        }



        private static async ThreadingTask RunPluginVersioningTest<TWorkflow>(Version targetVersion, bool shouldSucceed)
            where TWorkflow : Workflow, new()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";
            UpdateSchemaVersion(workflowInput, targetVersion);

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _container.Resolve<IWorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.IsTrue(loadSucceeded);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();

            if (shouldSucceed)
            {
                Assert.IsNotNull(workflowOutput);
                Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
            }
            else
            {
                Assert.IsNull(workflowOutput);
                Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);
            }
        }

        private static void UpdateSchemaVersion(PluginData<ExternalAnswerWorkflowInput> data, Version targetVersion)
        {
            data.Version = targetVersion;
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
