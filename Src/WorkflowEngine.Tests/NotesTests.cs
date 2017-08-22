using System;
using System.Linq;
using Microsoft.Practices.Unity;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;
using WorkflowEngine.Tests.Workflows;
using Xunit;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class NotesTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public NotesTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async ThreadingTask NoteOnSuccess()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<SuccessfulTaskWithComment>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<AOutput>(new PluginInputs { { "input", workflowInput } });
            var notes = pluginServices.GetPluginNotes().ToList();

            Assert.NotNull(workflowOutput);
            Assert.Equal(1, notes.Count);
            Assert.Equal("Task Succeeded", notes[0]);

        }

        [Fact]
        public async ThreadingTask NoteOnFailure()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<FailingTaskWithComment>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<AOutput>(new PluginInputs { { "input", workflowInput } });
            var notes = pluginServices.GetPluginNotes().ToList();

            Assert.NotNull(workflowOutput);
            Assert.Equal(1, notes.Count);
            Assert.Equal("Task Failed", notes[0]);

        }

        [Fact]
        public async ThreadingTask MultipleNotes()
        {
            var pluginServices = _fixture.Container.Resolve<IPluginServices>();
            pluginServices.Initialize();
            var workflow = pluginServices.GetOrCreatePlugin<NotesWorkflow>();
            var workflowInput = pluginServices.CreatePluginData<AInput>();

            var workflowOutput = await workflow.Execute<AOutput>(new PluginInputs { { "input", workflowInput } });
            var notes = pluginServices.GetPluginNotes().ToList();

            Assert.NotNull(workflowOutput);
            Assert.Equal(2, notes.Count);
            Assert.True(notes.Contains("Task Failed"));
            Assert.True(notes.Contains("Task Succeeded"));

        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
