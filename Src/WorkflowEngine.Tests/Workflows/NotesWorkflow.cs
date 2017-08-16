using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    internal class NotesWorkflow : Workflow
    {
        private static async Task<PluginOutput<AOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var aTask = pluginServices.GetOrCreatePlugin<SuccessfulTaskWithComment>();
            var aOutput = await aTask.Execute<AOutput>(new PluginInputs{{"input", input}});

            var bTask = pluginServices.GetOrCreatePlugin<FailingTaskWithComment>();
            var bOutput = await bTask.Execute<AOutput>(new PluginInputs { { "input", input } });                

            return await pluginServices.PluginCompleted(bOutput);

        }
    }
}
