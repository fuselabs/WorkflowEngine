using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    internal class OptionalParamsWorkflow : Workflow
    {
        private static async Task<PluginOutput<FOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var aTask = pluginServices.GetOrCreatePlugin<ATask>();
            var aOutput = await aTask.Execute<AOutput>(new PluginInputs { { "input", input } });

            var fTask = pluginServices.GetOrCreatePlugin<FTask>();
            var fOutput = await fTask.Execute<FOutput>(new PluginInputs { { "aOutput", aOutput }, {"bOutput", null} });

            return await pluginServices.PluginCompleted(fOutput);
        }
    }
}
