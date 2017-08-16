using System.Threading.Tasks;
using Schemas;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{

    public class IdentifyRequestTypeWorkflow : Workflow
    {
        private static async Task<PluginOutput<IdentifyRequestTypeOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<IdentifyRequestTypeInput> input)
        {
            // First Plugin "Identify Request Type Task"
            var identifyTask = pluginServices.GetOrCreatePlugin<IdentifyRequestTypeTask>();
            var identifyTaskOutput = await identifyTask.Execute<IdentifyRequestTypeOutput>(new PluginInputs { { "input", input } });

            // Second Plugin "Print Request Type to Console"
            var printTask = pluginServices.GetOrCreatePlugin<PrintToConsoleTask>();
            await printTask.Execute<Schema>(new PluginInputs { { "input", identifyTaskOutput } });

            return await pluginServices.PluginCompleted(identifyTaskOutput);
        }
    }
}
