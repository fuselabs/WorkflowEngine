using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Schemas;
using Xunit;
using Assert = Xunit.Assert;
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class PluginVersioningTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public PluginVersioningTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async ThreadingTask CompatiblePluginsSameVersion()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.ExternalAnswerWorkflow, Workflows.ExternalTask.V1Prime.ExternalAnswerWorkflow>(true);
        }

        [Fact]
        public async ThreadingTask CompatiblePluginsDifferentMinorVersion()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.ExternalAnswerWorkflow, Workflows.ExternalTask.V15.ExternalAnswerWorkflow>(true);
        }


        [Fact]
        public async ThreadingTask InompatiblePluginsDifferentMajorVersion_FirstOlder()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V1.ExternalAnswerWorkflow, Workflows.ExternalTask.V2.ExternalAnswerWorkflow>(false);
        }

        [Fact]
        public async ThreadingTask InompatiblePluginsDifferentMajorVersion_FirstNewer()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V2.ExternalAnswerWorkflow, Workflows.ExternalTask.V1.ExternalAnswerWorkflow>(false);
        }

        [Fact]
        public async ThreadingTask IncompatiblePluginsDifferentMinorVersion_FirstNewer()
        {
            await RunPluginVersioningTest<Workflows.ExternalTask.V15.ExternalAnswerWorkflow, Workflows.ExternalTask.V1.ExternalAnswerWorkflow>(false);
        }


        private async ThreadingTask RunPluginVersioningTest<TFirstPlugin, TSecondPlugin>(bool shouldSucceed)
            where TSecondPlugin : Workflow, new()
            where TFirstPlugin : Workflow, new()
        {
            _fixture.Container.RegisterInstance(GetServiceProvider(), new PerThreadLifetimeManager());
            _fixture.Container.RegisterType<IWorkflowContainer<TFirstPlugin, ExternalDataHandlerTaskOutput>, WorkflowContainer<TFirstPlugin, ExternalDataHandlerTaskOutput>>();
            _fixture.Container.RegisterType<IWorkflowContainer<TSecondPlugin, ExternalDataHandlerTaskOutput>, WorkflowContainer<TSecondPlugin, ExternalDataHandlerTaskOutput>>();
            var firstPluginContainer = _fixture.Container.Resolve<IWorkflowContainer<TFirstPlugin, ExternalDataHandlerTaskOutput>>();
            firstPluginContainer.Initialize();

            var workflowInput = firstPluginContainer.CreateWorkflowInput<ExternalAnswerWorkflowInput>();
            workflowInput.Data.Question = "2+2";

            // First execution. External dependencies won't be resolved
            var result = await firstPluginContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = firstPluginContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = firstPluginContainer.Store();

            var secondPluginContainer = _fixture.Container.Resolve<IWorkflowContainer<TSecondPlugin, ExternalDataHandlerTaskOutput>>();
            var loadSucceeded = secondPluginContainer.TryLoad(serializedWorkflowContext);
            Assert.True(loadSucceeded);

            // Second execution. External dependencies will be resolved
            result = await secondPluginContainer.ReExecute();
            workflowOutput = secondPluginContainer.GetOutput();
            if (shouldSucceed)
            {
                Assert.NotNull(workflowOutput);
                Assert.True(result == WorkflowContainerExecutionResult.Completed);
            }
            else
            {
                Assert.Null(workflowOutput);
                Assert.True(result == WorkflowContainerExecutionResult.NotExecuted);
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

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
