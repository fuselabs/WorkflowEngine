using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class CancellationTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public CancellationTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async ThreadingTask NoCancellation()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<CancellationWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<CancellationWorkflowInput>();

            var workflowOutput = await workflow.Execute<CancellableTaskOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
            Assert.False(workflowOutput.Data.Cancelled);
        }

        [Fact]
        public async ThreadingTask PluginCancellation()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<CancellationWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<CancellationWorkflowInput>();
            workflowInput.Data.CancelPlugin = true;

            var workflowOutput = await workflow.Execute<CancellableTaskOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
            Assert.True(workflowOutput.Data.Cancelled);
        }

        [Fact]
        public async ThreadingTask ExecutionCancellation()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<CancellationWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<CancellationWorkflowInput>();
            workflowInput.Data.CancelExecution = true;

            var workflowOutput = await workflow.Execute<CancellableTaskOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
            Assert.True(workflowOutput.Data.Cancelled);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
