using System.Threading.Tasks;
using Newtonsoft.Json;
using Schemas;
using Schemas.Framework;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine.Tests.Tasks
{

    public class DeserializeJsonTask<T> : Task
        where T : Schema, new()
    {
        private static Task<PluginOutput<T>> ExecutePlugin(IPluginServices pluginServices, PluginData<StringData> input)
        {
            var deserialized = JsonConvert.DeserializeObject<T>(input.Data.String);
            var output = pluginServices.CreatePluginData(deserialized);

            return pluginServices.PluginCompleted(output);
        }
    }
}
