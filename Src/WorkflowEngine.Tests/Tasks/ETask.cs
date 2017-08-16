using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class ETask : Task
    {
        private static Task<PluginOutput<EOutput>> ExecutePlugin(IPluginServices pluginServices, PluginDataList<AOutput> aOutputs)
        {
            var data = pluginServices.CreatePluginData<EOutput>();
            return pluginServices.PluginCompleted(data);
        }
    }
}
