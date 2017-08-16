using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows.ExternalTask.V1
{
    internal class ExternalAnswerWorkflow : Workflow
    {
        [Version(1, 0)]
        private static async Task<PluginOutput<ExternalDataHandlerTaskOutput>> ExecutePlugin(
            IPluginServices pluginServices, 
            PluginData<ExternalAnswerWorkflowInput> input)
        {
            var externalService = pluginServices.GetService("ExternalService") as ExternalService;
            if(externalService == null)
                throw new System.Exception("Failed to resolve service");
            
            var result = externalService.GetResult(input.Data.Question);
            var externalOutput = result == null ? null : pluginServices.CreatePluginData(result);
            var externalDataHandlerTask = pluginServices.GetOrCreatePlugin<ExternalDataHandlerTask>();
            var externalData = await externalDataHandlerTask.Execute<ExternalDataHandlerTaskOutput>(new PluginInputs { { "input", externalOutput } });

            return await pluginServices.PluginCompleted(externalData);

        }
    }
}
