using System;
using System.Collections.Generic;
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
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    [TestClass]
    public class BasicAsyncTests
    {
        private static IUnityContainer _container;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            _container = new UnityContainer();
            _container.RegisterType<IPluginServices, PluginServices>();
            _container.RegisterType<IWorkQueue, MockWorkQueue>();
            _container.RegisterType<IPluginConfig, MockPluginConfig>();
            _container.RegisterType<ILogger, MockLogger>();

            JsonUtils.SetGlobalJsonNetSettings();
        }

        [TestMethod]
        public async ThreadingTask MicroTaskWorkflow_BasicTest()
        {
            _container.RegisterInstance(GetServiceProvider());
            var pluginServices = _container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ExternalAnswerWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            var workflowOutput = await workflow.Execute<ExternalDataHandlerTaskOutput>(new PluginInputs { { "input", workflowInput }});

            Assert.IsNull(workflowOutput);
            Assert.IsFalse(pluginServices.AllPluginsExecuted());

        }

        [TestMethod]
        public async ThreadingTask MicroTaskWorkflow_BasicTestWithContainer()
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

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);

        }

        
        [TestMethod]
        public async ThreadingTask MicroTaskWorkflow_AsyncWithContainer()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var workflowContext = workflowContainer.GetContext();
            workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize(workflowContext);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);

        }
        
        [TestMethod]
        public async ThreadingTask MicroTaskWorkflow_AsyncWithSerializedContainerContext()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.IsTrue(loadSucceeded);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);

        }

        [TestMethod]
        // Test to ensure that repeated executions of the workflow that don't
        // change state also don't lead to an increase in the size of the persisted workflow state
        public async ThreadingTask MicroTaskWorkflow_RepeatedExecution()
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

            workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.TryLoad(serializedWorkflowContext);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);


            serializedWorkflowContext = workflowContainer.Store();
            for (var i = 0; i <= 10; i++)
            {
                workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
                workflowContainer.TryLoad(serializedWorkflowContext);
                result = await workflowContainer.ReExecute();

                var newSerializedWorkflowContext = workflowContainer.Store();
                Assert.IsTrue(serializedWorkflowContext.Length == newSerializedWorkflowContext.Length);
                Assert.IsNotNull(workflowOutput);
                Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
                serializedWorkflowContext = newSerializedWorkflowContext;
            }

        }

        [TestMethod]
        // Test to ensure that repeated executions of the workflow that don't
        // change state also don't lead to an increase in the size of the persisted workflow state
        // Even when the inputs to the workflow container are changed
        public async ThreadingTask MicroTaskWorkflow_RepeatedExecution_UpdatedInputs()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.TryLoad(serializedWorkflowContext);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);


            serializedWorkflowContext = workflowContainer.Store();
            for (var i = 0; i <= 10; i++)
            {
                workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
                workflowContainer.TryLoad(serializedWorkflowContext);
                workflowInput = workflowContainer.GetWorkflowInput<ExternalAnswerWorkflowInput>("input");
                workflowInput.Data.Question = "2+2";
                result = await workflowContainer.ReExecute(new PluginInputs { { "input", workflowInput } });

                var newSerializedWorkflowContext = workflowContainer.Store();
                Assert.IsTrue(serializedWorkflowContext.Length == newSerializedWorkflowContext.Length);
                Assert.IsNotNull(workflowOutput);
                Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);
                serializedWorkflowContext = newSerializedWorkflowContext;
            }

        }

        [TestMethod]
        public async ThreadingTask MicroTaskWorkflow_UpdateInputs()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            workflowInput = workflowContainer.GetWorkflowInput<ExternalAnswerWorkflowInput>("input");
            workflowInput.Data.Question = "3+3";

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute(new PluginInputs { { "input", workflowInput }});
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);

        }


        [TestMethod]
        public async ThreadingTask MicroTaskWorkflow_VariantConstraints()
        {
            _container.RegisterInstance(GetServiceProvider());
            _container.RegisterInstance(new MockPluginConfig(
                new Dictionary<string, string>
                {
                    { "ExternalAnswerOverride", "OverridenNoFlight||flt::flt1==OverridenWithFlight" }
                }));
            _container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize(variants: new List<Tuple<string, string>> {Tuple.Create("flt", "flt1")});

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.IsNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            workflowInput = workflowContainer.GetWorkflowInput<ExternalAnswerWorkflowInput>("input");
            workflowInput.Data.Question = "3+3";

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute(new PluginInputs { { "input", workflowInput } });
            workflowOutput = workflowContainer.GetOutput();
            Assert.IsNotNull(workflowOutput);
            Assert.IsTrue(result == WorkflowContainerExecutionResult.Completed);

            // Ensure that the variant constraints were taken into account
            Assert.AreEqual(workflowOutput.Data.Answer, "OverridenWithFlight");

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
