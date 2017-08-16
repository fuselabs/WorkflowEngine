using System.Threading.Tasks;
using Schemas;
using Schemas.Framework;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    public class ExternalDataHandlerTask : Task
    {
        private static Task<PluginOutput<ExternalDataHandlerTaskOutput>> ExecutePlugin(
            IPluginServices pluginServices, 
            PluginData<ExternalTaskOutput<StringData>> input)
        {
            var output = pluginServices.CreatePluginData<ExternalDataHandlerTaskOutput>();
            output.Data.Answer = input.Data.OutputData.String;
            return pluginServices.PluginCompleted(output);
        }
    }
}
