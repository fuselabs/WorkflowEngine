using Schemas;

namespace WorkflowEngine.Tests.Schemas
{
    internal class CancellationWorkflowInput : Schema
    {
        public bool CancelPlugin { set; get; }
        public bool CancelExecution { set; get; }
    }
}
