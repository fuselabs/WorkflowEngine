using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Schemas;
using Schemas.Framework;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Logging.Interfaces;
using WorkflowEngine.Tests.Mocks.Config;
using WorkflowEngine.Tests.Mocks.Logging;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Mocks.WorkQueue;
using WorkflowEngine.Tests.Workflows;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class JsonExternalWorkflowTests
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
        public async ThreadingTask JsonExternalWorkflow_BasicTest()
        {
            // Initialize Test String
            const string testInputData = "Do you have Kobe Meat?";
            var expectedJson = JsonConvert.SerializeObject(new StringData { String = "Sure, we do have Kobe meat!" });

            // Execute Test
            _container.RegisterType<
                IWorkflowContainer<JsonExternalWorkflow<StringData, StringData>, StringData>,
                WorkflowContainer<JsonExternalWorkflow<StringData, StringData>, StringData>>();
            
            _container.RegisterInstance(GetServiceProvider(expectedJson));
            var workflowContainer = _container.Resolve<IWorkflowContainer<JsonExternalWorkflow<StringData, StringData>, StringData>>();
            workflowContainer.Initialize();


            var input = workflowContainer.CreateWorkflowInput<JsonExternalWorkflowInput<StringData>>();
            var data = workflowContainer.CreateWorkflowInput<StringData>();
            data.Data.String = testInputData;
            input.Data.RequestObject = data.Data;
            input.Data.ExternalTaskType = ExternalTaskType.Test_JsonQAndA;
            var result = await workflowContainer.Execute(new PluginInputs { { "input", input } });
            var workflowOutput = workflowContainer.GetOutput();

            // First execution. External dependencies won't be resolved
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);


            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
            Assert.AreEqual(workflowOutput.Data.String, "Sure, we do have Kobe meat!");
        }

        private static IServiceProvider GetServiceProvider(string data)
        {
            var externalService = new ExternalService();
            externalService.SetResult(data, 1);
            var serviceProvider = new MockServiceProvider();
            serviceProvider.SetService("ExternalService", externalService);
            return serviceProvider;
        }
    }
}
