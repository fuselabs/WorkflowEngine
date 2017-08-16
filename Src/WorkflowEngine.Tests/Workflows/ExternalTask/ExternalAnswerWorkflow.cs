using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows.ExternalTask
{
    internal class ExternalAnswerWorkflow : Workflow
    {
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

            if (externalData != null)
            {
                var configOverride = pluginServices.GetConfig().Get("ExternalAnswerOverride");
                if (!string.IsNullOrEmpty(configOverride))
                    externalData.Data.Answer = configOverride;
            }

            return await pluginServices.PluginCompleted(externalData);

        }
    }
}
