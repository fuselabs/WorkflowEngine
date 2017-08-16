using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    internal class SequentialUnorderedWorkflow : Workflow
    {
        private static async Task<PluginOutput<COutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var aTask = pluginServices.GetOrCreatePlugin<ATask>();
            var aOutput = await aTask.Execute<AOutput>(new PluginInputs { { "input", input } });

            var cTask = pluginServices.GetOrCreatePlugin<CTask>();
            var cOutput = await cTask.Execute<COutput>(new PluginInputs { { "input", null } });

            var bTask = pluginServices.GetOrCreatePlugin<BTask>();
            var bOutput = await bTask.Execute<BOutput>(new PluginInputs { { "input", aOutput } });

            return cOutput != null ?
                await pluginServices.PluginCompleted(cOutput) :
                await pluginServices.PluginIncomplete <COutput>();

        }
    }
}
