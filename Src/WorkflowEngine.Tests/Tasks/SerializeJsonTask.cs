using System.Threading.Tasks;
using Newtonsoft.Json;
using Schemas;
using Schemas.Framework;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine.Tests.Tasks
{
    public class SerializeJsonTask<T> : Task
        where T : Schema, new()
    {
        private static Task<PluginOutput<StringData>> ExecutePlugin(IPluginServices pluginServices, PluginData<T> input)
        {
            var output = pluginServices.CreatePluginData<StringData>();
            output.Data.String = JsonConvert.SerializeObject(input.Data);

            return pluginServices.PluginCompleted(output);
        }
    }
}
