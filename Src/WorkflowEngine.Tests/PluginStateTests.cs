using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class PluginStateTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public PluginStateTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public async ThreadingTask AsyncExecution()
        {
            _fixture.Container.RegisterType<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>, WorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<AInput>();

            // First execution. Workflow shouldn't resolve yet
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Second execution. Workflow shouldn't resolve yet
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Third execution. Workflow should resolve
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);
            Assert.True(workflowOutput.Data.CurrentValue == 3);

        }

        [Fact]
        public async ThreadingTask AsyncExecutionWithSerialization()
        {
            _fixture.Container.RegisterType<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>, WorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            workflowContainer.Initialize();

            var workflowInput = workflowContainer.CreateWorkflowInput<AInput>();

            // First execution. Workflow shouldn't resolve yet
            var result = await workflowContainer.Execute(new PluginInputs { { "input", workflowInput } });
            var workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            var serializedWorkflowContext = workflowContainer.Store();
            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            var loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.True(loadSucceeded);

            // Second execution. Workflow shouldn't resolve yet
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);

            // Store Context and recreate workflow container
            serializedWorkflowContext = workflowContainer.Store();
            workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<StatefulWorkflow, StatefulWorkflowState>>();
            loadSucceeded = workflowContainer.TryLoad(serializedWorkflowContext);
            Assert.True(loadSucceeded);

            // Third execution. Workflow should resolve
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);
            Assert.True(workflowOutput.Data.CurrentValue == 3);

        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
