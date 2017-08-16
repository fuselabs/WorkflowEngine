using Schemas;

namespace WorkflowEngine.Tests.Schemas
{
    internal class StatefulWorkflowState : Schema
    {
        public int CurrentValue { set; get; }
    }
}
