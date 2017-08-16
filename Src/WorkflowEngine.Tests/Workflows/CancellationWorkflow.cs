using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    internal class CancellationWorkflow : Workflow
    {
        private static async Task<PluginOutput<CancellableTaskOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<CancellationWorkflowInput> input)
        {

            var cancellableTask = pluginServices.GetOrCreatePlugin<CancellableTask>();
            if (input.Data.CancelExecution)
            {
                pluginServices.Cancel();
            }
            if (input.Data.CancelPlugin)
            {
                cancellableTask.Cancel();
            }

            var cancellableTaskOutput = await cancellableTask.Execute<CancellableTaskOutput>(new PluginInputs { { "input", input } });                
            return await pluginServices.PluginCompleted(cancellableTaskOutput);

        }
    }
}
