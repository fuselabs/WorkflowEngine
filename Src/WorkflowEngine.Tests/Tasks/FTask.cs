using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class FTask : Task
    {
        private static Task<PluginOutput<FOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AOutput> aOutput, [Optional]PluginData<BOutput> bOutput)
        {
            var data = pluginServices.CreatePluginData<FOutput>();
            return pluginServices.PluginCompleted(data);
        }
    }
}
