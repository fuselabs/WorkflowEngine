using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    internal class CancellableTask : Task
    {
        private static Task<PluginOutput<CancellableTaskOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<CancellationWorkflowInput> input)
        {
            var data = pluginServices.CreatePluginData<CancellableTaskOutput>();
            data.Data.Cancelled = pluginServices.PluginCancelled();

            return pluginServices.PluginCompleted(data);
        }
    }
}
