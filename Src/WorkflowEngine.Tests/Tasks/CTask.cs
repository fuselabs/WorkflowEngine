using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class CTask : Task
    {
        private static Task<PluginOutput<COutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<BOutput> input)
        {
            var data = pluginServices.CreatePluginData<COutput>();
            return pluginServices.PluginCompleted(data);
        }
    }
}
