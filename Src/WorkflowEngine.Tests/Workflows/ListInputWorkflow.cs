using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    public class ListInputWorkflow : Workflow
    {
        private static async Task<PluginOutput<EOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var aTask1 = pluginServices.GetOrCreatePlugin<ATask>();
            var aOutput1 = await aTask1.Execute<AOutput>(new PluginInputs { { "input", input } });

            var aTask2 = pluginServices.GetOrCreatePlugin<ATask>();
            var aOutput2 = await aTask2.Execute<AOutput>(new PluginInputs { { "input", input } });

            var eTaskInput = pluginServices.CreatePluginDataList<AOutput>();
            eTaskInput.Add(aOutput1.Data);
            eTaskInput.Add(aOutput2.Data);
            
            var eTask = pluginServices.GetOrCreatePlugin<ETask>();
            var eOutput = await eTask.Execute<EOutput>(new PluginInputs { { "aOutputs", eTaskInput } });

            return await pluginServices.PluginCompleted(eOutput);

        }
    }
}
