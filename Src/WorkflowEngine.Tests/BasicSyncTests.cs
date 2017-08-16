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
    public class BasicSyncTests
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
        // Task A -> Task B -> Task C
        // Workflow: A->B->C
        public async ThreadingTask WorkflowEngine_SequentialOrderedExecution()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<SequentialOrderedWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<COutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
        }

        [TestMethod]
        // Task A -> Task B -> Task C
        // Workflow: A->C->B
        public async ThreadingTask WorkflowEngine_SequentialUnorderedExecution()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<SequentialUnorderedWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var output = await workflow.Execute<COutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNull(output);
            Assert.IsFalse(pluginServices.AllPluginsExecuted());
        }

        [TestMethod]
        // Task A->B->D and A->D
        // Workflow: A->B->D
        public async ThreadingTask WorkflowEngine_ParallelOrderedExecution()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ParallelOrderedWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<DOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
        }

        [TestMethod]
        // Task A1 -> E and A2 -> E
        // Workflow A1 -> A2 -> E
        public async ThreadingTask WorkflowEngine_ListInputExecution()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ListInputWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<EOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
        }

        [TestMethod]
        // Task A1 -> E and A2 -> E
        // Workflow (A1 AND A2) using Fork -> E 
        public async ThreadingTask WorkflowEngine_ForkAndJoinExecution()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ForkAndJoinWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<EOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
        }

        [TestMethod]
        // Task A -> F and B -> (optional) F
        // Workflow (A AND B) -> F, B returns nulls
        public async ThreadingTask WorkflowEngine_OptionalParameters()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<OptionalParamsWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<FOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.IsNotNull(workflowOutput);
        }

    }

}
