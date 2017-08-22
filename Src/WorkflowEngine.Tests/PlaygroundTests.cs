using System;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Workflows;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class PlaygroundTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public PlaygroundTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async ThreadingTask SmokeTest()
        {

            const string email = "Hi, can you please help me find out if Tom and Dan prefer Green or Blue?";
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();

            var input = pluginServices.CreatePluginData<IdentifyRequestTypeInput>();
            input.Data.Email = email;

            var identifyWorkflow = pluginServices.GetOrCreatePlugin<IdentifyRequestTypeWorkflow>();

            var output = await identifyWorkflow.Execute<IdentifyRequestTypeOutput>(new PluginInputs { { "input", input } });
            Assert.NotNull(output);
            Assert.True(output.Data.Type == RequestType.Voting);
            Assert.True(pluginServices.AllPluginsExecuted());
        }

        [Fact]
        public async ThreadingTask WorkflowEngine_SmokeTestWithContainer()
        {
            const string email = "Hi, can you please help me find out if Tom and Dan prefer Green or Blue?";
            _fixture.Container.RegisterType<IWorkflowContainer<IdentifyRequestTypeWorkflow, IdentifyRequestTypeOutput>, WorkflowContainer<IdentifyRequestTypeWorkflow, IdentifyRequestTypeOutput>>();

            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<IdentifyRequestTypeWorkflow, IdentifyRequestTypeOutput>>();
            workflowContainer.Initialize();

            var input = workflowContainer.CreateWorkflowInput<IdentifyRequestTypeInput>();
            input.Data.Email = email;

            var result = await workflowContainer.Execute(new PluginInputs { { "input", input } });
            var output = workflowContainer.GetOutput();
            Assert.NotNull(output);
            Assert.True(output.Data.Type == RequestType.Voting);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
