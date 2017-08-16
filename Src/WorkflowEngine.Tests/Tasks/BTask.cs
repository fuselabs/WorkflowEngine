using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class BTask : Task
    {
        private static Task<PluginOutput<BOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AOutput> input)
        {
            var data = pluginServices.CreatePluginData<BOutput>();
            return pluginServices.PluginCompleted(data);
        }
    }
}
