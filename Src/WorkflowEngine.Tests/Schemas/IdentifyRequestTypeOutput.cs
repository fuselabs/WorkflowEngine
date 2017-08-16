using Schemas;

namespace WorkflowEngine.Tests.Schemas
{
    public enum RequestType
    {
        Voting,
        Unknown
    }
    public class IdentifyRequestTypeOutput : Schema
    {
        public RequestType Type { get; set; }
    }
}
