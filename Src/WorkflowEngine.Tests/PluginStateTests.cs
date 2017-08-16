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
using WorkflowEngine.Tests.Workflows;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class PluginStateTests
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
        public async ThreadingTask PluginState_AsyncExecution()
        {
            _container.RegisterType<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>, WorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<AInput>();

            // First execution. Workflow shouldn't resolve yet
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Second execution. Workflow shouldn't resolve yet
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Third execution. Workflow should resolve
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
            Assert.IsTrue(workflowOutput.Data.CurrentValue == 3);

        }

        [TestMethod]
        public async ThreadingTask PluginState_AsyncExecutionWithSerialization()
        {
            _container.RegisterType<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>, WorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<AInput>();

            // First execution. Workflow shouldn't resolve yet
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();
            workflowContainer = _container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.IsTrue(loadSucceeded);

            // Second execution. Workflow shouldn't resolve yet
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            serializedWorkflowContext = workflowContainer.Store();
            workflowContainer = _container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.IsTrue(loadSucceeded);

            // Third execution. Workflow should resolve
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
            Assert.IsTrue(workflowOutput.Data.CurrentValue == 3);

        }
    }
}
