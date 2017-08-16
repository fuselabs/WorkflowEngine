using System;
using System.Threading.Tasks;
using Schemas;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Tasks
{
    class PrintToConsoleTask : Task
    {
        private static Task<PluginOutput<Schema>> ExecutePlugin(IPluginServices pluginServices, PluginData<IdentifyRequestTypeOutput> input)
        {
            Console.WriteLine(input.Data.Type.ToString());
            var data = pluginServices.CreatePluginData<Schema>();

            return pluginServices.PluginCompleted(data);
        }

    }
}
