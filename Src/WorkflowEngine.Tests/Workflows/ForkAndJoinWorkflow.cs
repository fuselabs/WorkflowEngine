using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;
using WorkflowEngine.Tests.Tasks;

namespace WorkflowEngine.Tests.Workflows
{
    public class ForkAndJoinWorkflow : Workflow
    {
        private static async Task<PluginOutput<EOutput>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            var aTasks = pluginServices.GetOrCreatePlugins<ATask>(2);

            var eTaskInput = pluginServices.CreatePluginDataList<AOutput>();
            foreach (var aTask in aTasks)
            {
                var aTaskOutput = await aTask.Execute<AOutput>(new PluginInputs { { "input", input } });
                eTaskInput.Add(aTaskOutput.Data);
            }

            var eTask = pluginServices.GetOrCreatePlugin<ETask>();
            var eOutput = await eTask.Execute<EOutput>(new PluginInputs { { "aOutputs", eTaskInput } });

            return await pluginServices.PluginCompleted(eOutput);

        }
    }
}
