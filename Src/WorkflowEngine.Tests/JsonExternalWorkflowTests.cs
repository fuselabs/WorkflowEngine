using System;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Schemas;
using Schemas.Framework;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Mocks.ServiceProvider;
using WorkflowEngine.Tests.Workflows;
using Xunit;
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests
{
    public class JsonExternalWorkflowTests : IClassFixture<UnityContainerFixture>, IDisposable
    {
        private readonly UnityContainerFixture _fixture;

        public JsonExternalWorkflowTests(UnityContainerFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public async ThreadingTask BasicTest()
        {
            // Initialize Test String
            const string testInputData = "Do you have Kobe Meat?";
            var expectedJson = JsonConvert.SerializeObject(new StringData { String = "Sure, we do have Kobe meat!" });

            // Execute Test
            _fixture.Container.RegisterType<
                IWorkflowContainer<JsonExternalWorkflow<StringData, StringData>, StringData>,
                WorkflowContainer<JsonExternalWorkflow<StringData, StringData>, StringData>>();

            _fixture.Container.RegisterInstance(GetServiceProvider(expectedJson));
            var workflowContainer = _fixture.Container.Resolve<IWorkflowContainer<JsonExternalWorkflow<StringData, StringData>, StringData>>();
            workflowContainer.Initialize();


            var input = workflowContainer.CreateWorkflowInput<JsonExternalWorkflowInput<StringData>>();
            var data = workflowContainer.CreateWorkflowInput<StringData>();
            data.Data.String = testInputData;
            input.Data.RequestObject = data.Data;
            input.Data.ExternalTaskType = ExternalTaskType.Test_JsonQAndA;
            var result = await workflowContainer.Execute(new PluginInputs { { "input", input } });
            var workflowOutput = workflowContainer.GetOutput();

            // First execution. External dependencies won't be resolved
            Assert.Null(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.PartiallyCompleted);


            // Second execution. External dependencies will be resolved
            result = await workflowContainer.ReExecute();
            workflowOutput = workflowContainer.GetOutput();
            Assert.NotNull(workflowOutput);
            Assert.True(result == WorkflowContainerExecutionResult.Completed);
            Assert.Equal(workflowOutput.Data.String, "Sure, we do have Kobe meat!");
        }

        private static IServiceProvider GetServiceProvider(string data)
        {
            var externalService = new ExternalService();
            externalService.SetResult(data, 1);
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
