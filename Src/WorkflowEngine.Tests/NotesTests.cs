using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Logging.Interfaces;
using WorkflowEngine.Tests.Mocks.Config;
using WorkflowEngine.Tests.Mocks.Logging;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Mocks.WorkQueue;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;
using WorkflowEngine.Tests.Workflows;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class NotesTests
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
        public async ThreadingTask WorkflowEngine_NoteOnSuccess()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<SuccessfulTaskWithComment>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<AOutput>(new PluginInputs { { "input", workflowInput } });
            var notes = pluginServices.GetPluginNotes().ToList();

            Assert.IsNotNull(workflowOutput);
            Assert.AreEqual(1, notes.Count);
            Assert.AreEqual("Task Succeeded", notes[0]);

        }

        [TestMethod]
        public async ThreadingTask WorkflowEngine_NoteOnFailure()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<FailingTaskWithComment>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<AOutput>(new PluginInputs { { "input", workflowInput } });
            var notes = pluginServices.GetPluginNotes().ToList();

            Assert.IsNotNull(workflowOutput);
            Assert.AreEqual(1, notes.Count);
            Assert.AreEqual("Task Failed", notes[0]);

        }

        [TestMethod]
        public async ThreadingTask WorkflowEngine_MultipleNotes()
        {
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<NotesWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<AOutput>(new PluginInputs { { "input", workflowInput } });
            var notes = pluginServices.GetPluginNotes().ToList();

            Assert.IsNotNull(workflowOutput);
            Assert.AreEqual(2, notes.Count);
            Assert.IsTrue(notes.Contains("Task Failed"));
            Assert.IsTrue(notes.Contains("Task Succeeded"));

        }
    }
}
