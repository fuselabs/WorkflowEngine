using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class BasicSyncTests : IClassFixture<UnityContainerFixture>, IDisposable
    {

        private readonly UnityContainerFixture _fixture;

        public BasicSyncTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        // Task A -> Task B -> Task C
        // Workflow: A->B->C
        public async ThreadingTask SequentialOrderedExecution()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<SequentialOrderedWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<COutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
        }

        [Fact]
        // Task A -> Task B -> Task C
        // Workflow: A->C->B
        public async ThreadingTask SequentialUnorderedExecution()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<SequentialUnorderedWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var output = await workflow.Execute<COutput>(new PluginInputs { { "input", workflowInput } });

            Assert.Null(output);
            Assert.False(pluginServices.AllPluginsExecuted());
        }

        [Fact]
        // Task A->B->D and A->D
        // Workflow: A->B->D
        public async ThreadingTask ParallelOrderedExecution()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ParallelOrderedWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<DOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
        }

        [Fact]
        // Task A1 -> E and A2 -> E
        // Workflow A1 -> A2 -> E
        public async ThreadingTask ListInputExecution()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ListInputWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<EOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
        }

        [Fact]
        // Task A1 -> E and A2 -> E
        // Workflow (A1 AND A2) using Fork -> E 
        public async ThreadingTask ForkAndJoinExecution()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<ForkAndJoinWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<EOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
        }

        [Fact]
        // Task A -> F and B -> (optional) F
        // Workflow (A AND B) -> F, B returns nulls
        public async ThreadingTask OptionalParameters()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<OptionalParamsWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<FOutput>(new PluginInputs { { "input", workflowInput } });

            Assert.NotNull(workflowOutput);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }

}
