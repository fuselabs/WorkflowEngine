using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    public class IdentifyRequestTypeTask : Task
    {
        private static Task<PluginOutput<IdentifyRequestTypeOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<IdentifyRequestTypeInput> input)
        {
            var data = pluginServices.CreatePluginData<IdentifyRequestTypeOutput>();

            data.Data.Type = input != null && input.Data.Email.Contains("prefer")
                                    ? RequestType.Voting
                                    : RequestType.Unknown;

            return pluginServices.PluginCompleted(data);
        }
    }
}
