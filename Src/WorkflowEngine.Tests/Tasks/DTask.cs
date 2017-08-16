using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class DTask : Task
    {
        private static Task<PluginOutput<DOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AOutput> aOutput, PluginData<BOutput> bOutput)
        {
            var data = pluginServices.CreatePluginData<DOutput>();
            return pluginServices.PluginCompleted(data);
        }
    }
}
