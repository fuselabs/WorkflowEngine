using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    internal class ParallelOrderedWorkflow : Workflow
    {
        private static async Task<PluginOutput<DOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var aTask = pluginServices.GetOrCreatePlugin<ATask>();
            var aOutput = await aTask.Execute<AOutput>(new PluginInputs { { "input", input } });

            var bTask = pluginServices.GetOrCreatePlugin<BTask>();
            var bOutput = await bTask.Execute<BOutput>(new PluginInputs { { "input", aOutput } });

            var dTask = pluginServices.GetOrCreatePlugin<DTask>();
            var dOutput = await dTask.Execute<DOutput>(new PluginInputs { { "aOutput", aOutput }, {"bOutput", bOutput} });

            return await pluginServices.PluginCompleted(dOutput);
        }
    }
}
