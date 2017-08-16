using System.Threading.Tasks;
using Schemas;
using Schemas.Framework;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    /// <summary>
    /// This is a workflow that allows executing an External Task using JSON
    /// for input and output
    /// </summary>
    public class JsonExternalWorkflow<TRequest, TResponse> : Workflow
        where TRequest : Schema, new()
        where TResponse : Schema, new()
    {
        private static async Task<PluginOutput<TResponse>> ExecutePlugin(
            IPluginServices pluginServices,
            PluginData<JsonExternalWorkflowInput<TRequest>> input)
        {
            var serializeTask = pluginServices.GetOrCreatePlugin<SerializeJsonTask<TRequest>>();
            var serializeTaskInput = pluginServices.CreatePluginData<TRequest>(input.Data.RequestObject);
            var serializeOutput = await serializeTask.Execute<StringData>(new PluginInputs { { "input", serializeTaskInput } });

            var externalService = pluginServices.GetService("ExternalService") as ExternalService;
            if (externalService == null)
                throw new System.Exception("Failed to resolve service");

            var result = externalService.GetResult(serializeOutput.Data.String);
            if (result == null)
                return await pluginServices.PluginIncomplete<TResponse>();

            var externalOutput =  pluginServices.CreatePluginData(result);

            var deserializeTask = pluginServices.GetOrCreatePlugin<DeserializeJsonTask<TResponse>>();
            var deserializeInput = pluginServices.CreatePluginData(externalOutput.Data.OutputData);
            var deserializeOutput = await deserializeTask.Execute<TResponse>(new PluginInputs { { "input", deserializeInput } });

            return await pluginServices.PluginCompleted(deserializeOutput);
        }
    }
}
