using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class FailingTaskWithComment : Task
    {
        private static Task<PluginOutput<AOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var data = pluginServices.CreatePluginData<AOutput>();
            return pluginServices.PluginCompleted(data, "Task Failed");
        }
    }
}
