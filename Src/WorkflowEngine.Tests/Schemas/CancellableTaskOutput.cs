using Schemas;

namespace WorkflowEngine.Tests.Schemas
{
    internal class CancellableTaskOutput : Schema
    {
        public bool Cancelled { set; get; }
    }
}
