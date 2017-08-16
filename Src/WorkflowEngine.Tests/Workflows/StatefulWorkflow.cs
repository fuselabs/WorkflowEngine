using System.Threading.Tasks;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Tests.Schemas;

namespace WorkflowEngine.Tests.Workflows
{
    /// <summary>
    /// This task doesn't complete until it has been called 3 times
    /// </summary>
    public class StatefulWorkflow : Workflow
    {
        private PluginData<StatefulWorkflowState> _currentValue;
        private Task<PluginOutput<StatefulWorkflowState>> ExecutePlugin(IPluginServices pluginServices, PluginData<AInput> input)
        {
            if (_currentValue == null)
                _currentValue = pluginServices.CreatePluginData<StatefulWorkflowState>();

            return ++_currentValue.Data.CurrentValue == 3 ? pluginServices.PluginCompleted(_currentValue) : pluginServices.PluginIncomplete<StatefulWorkflowState>();
        }
    }
}
