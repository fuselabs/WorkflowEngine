using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows.ExternalTask;
using Xunit;
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class SchemaVersioningTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public SchemaVersioningTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async ThreadingTask BreakingSchemaChange()
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

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            // Simulate a breaking schema change by changing the type of the Question 
            // Property from String to a nested object
            serializedWorkflowContext = serializedWorkflowContext.Replace("\"2+2\"", "{\"QuestionString\":\"2+2\"}");

            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<ExternalAnswerWorkflow, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.False(loadSucceeded);
        }

        [Fact]
        public async ThreadingTask CompatibleSchemas_SameVersion()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV1.ExternalAnswerWorkflow>(new Version(1, 0), true);
        }

        [Fact]
        public async ThreadingTask CompatibleSchemas_DifferentMinorVersion_NewerSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV1.ExternalAnswerWorkflow>(new Version(1, 5), true);
        }

        [Fact]
        public async ThreadingTask IncompatibleSchemas_DifferentMinorVersion_OlderSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV15.ExternalAnswerWorkflow>(new Version(1, 0), false);
        }

        [Fact]
        public async ThreadingTask InompatibleSchemas_DifferentMajorVersion_NewerSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV15.ExternalAnswerWorkflow>(new Version(2, 0), false);
        }

        [Fact]
        public async ThreadingTask InompatibleSchemas_DifferentMajorVersion_OlderSchema()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.InputV2.ExternalAnswerWorkflow>(new Version(1, 0), false);
        }



        private async ThreadingTask RunPluginVersioningTest<TWorkflow>(Version targetVersion, bool shouldSucceed)
            where TWorkflow : Workflow, new()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>, WorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";
            UpdateSchemaVersion(workflowInput, targetVersion);

            // First execution. External dependencies won't be resolved
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();

            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<TWorkflow, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.True(loadSucceeded);

            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();

            if (shouldSucceed)
            {
                Assert.NotNull(workflowOutput);
                Assert.True(result == WorkflowContainerExecutionResult.Completed);
            }
            else
            {
                Assert.Null(workflowOutput);
                Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);
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

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
