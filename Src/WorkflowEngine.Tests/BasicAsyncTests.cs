using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Mocks.Config;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows.ExternalTask;
using Xunit;
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{

    public class BasicAsyncTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public BasicAsyncTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async ThreadingTask BasicTest()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ExternalAnswerWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            var workflowOutput = await workflow.Execute<ExternalDataHandlerTaskOutput>(new PluginInputs { { "input", workflowInput }});

            Assert.Null(workflowOutput);
            Assert.False(pluginServices.AllPluginsExecuted());

        }

        [Fact]
        public async ThreadingTask BasicTestWithContainer()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);

        }

        
        [Fact]
        public async ThreadingTask AsyncWithContainer()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var workflowContext = workflowContainer.GetContext();
            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize(workflowContext);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);

        }
        
        [Fact]
        public async ThreadingTask AsyncWithSerializedContainerContext()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.True(loadSucceeded);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);

        }

        [Fact]
        // Test to ensure that repeated executions of the workflow that don't
        // change state also don't lead to an increase in the size of the persisted workflow state
        public async ThreadingTask RepeatedExecution()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.TryLoad(serializedWorkflowContext);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);


            serializedWorkflowContext = workflowContainer.Store();
            for (var i = 0; i <= 10; i++)
            {
                workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
                workflowContainer.TryLoad(serializedWorkflowContext);
                result = await workflowContainer.ReExecute();

                var newSerializedWorkflowContext = workflowContainer.Store();
                Assert.True(serializedWorkflowContext.Length == newSerializedWorkflowContext.Length);
                Assert.NotNull(workflowOutput);
                Assert.True(result == WorkflowContainerExecutionResult.Completed);
                serializedWorkflowContext = newSerializedWorkflowContext;
            }

        }

        [Fact]
        // Test to ensure that repeated executions of the workflow that don't
        // change state also don't lead to an increase in the size of the persisted workflow state
        // Even when the inputs to the workflow container are changed
        public async ThreadingTask RepeatedExecution_UpdatedInputs()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.TryLoad(serializedWorkflowContext);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);


            serializedWorkflowContext = workflowContainer.Store();
            for (var i = 0; i <= 10; i++)
            {
                workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
                workflowContainer.TryLoad(serializedWorkflowContext);
                workflowInput = workflowContainer.GetWorkflowInput<ExternalAnswerWorkflowInput>("input");
                workflowInput.Data.Question = "2+2";
                result = await workflowContainer.ReExecute(new PluginInputs { { "input", workflowInput } });

                var newSerializedWorkflowContext = workflowContainer.Store();
                Assert.True(serializedWorkflowContext.Length == newSerializedWorkflowContext.Length);
                Assert.NotNull(workflowOutput);
                Assert.True(result == WorkflowContainerExecutionResult.Completed);
                serializedWorkflowContext = newSerializedWorkflowContext;
            }

        }

        [Fact]
        public async ThreadingTask UpdateInputs()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput }});
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            workflowInput = workflowContainer.GetWorkflowInput<ExternalAnswerWorkflowInput>("input");
            workflowInput.Data.Question = "3+3";

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute(new PluginInputs { { "input", workflowInput }});
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);

        }


        [Fact]
        public async ThreadingTask VariantConstraints()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterInstance(new MockPluginConfig(
                new Dictionary<string, string>
                {
                    { "ExternalAnswerOverride", "OverridenNoFlight||flt::flt1==OverridenWithFlight" }
                }), 
                new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize(variants: new List<Tuple<string, string>> {Tuple.Create("flt", "flt1")});

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            workflowInput = workflowContainer.GetWorkflowInput<ExternalAnswerWorkflowInput>("input");
            workflowInput.Data.Question = "3+3";

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute(new PluginInputs { { "input", workflowInput } });
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);

            // Ensure that the variant constraints were taken into account
            Assert.Equal(workflowOutput.Data.Answer, "OverridenWithFlight");

        }

        private static IServiceProvider GetServiceProvider()
        {
            var externalService = new ExternalService();
            externalService.SetResult("4", 1);
            var serviceProvider = new MockServiceProvider();
            serviceProvider.SetService("ExternalService", externalService);
            return serviceProvider;
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
