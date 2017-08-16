using Schemas;
using Schemas.Framework;

namespace WorkflowEngine.Tests
{
    public class ExternalService
    {
        private string _result = string.Empty;
        private int _availableAfter;
        public void SetResult(string result, int availableAfter = 0)
        {
            _result = result;
            _availableAfter = availableAfter;
        }
        public ExternalTaskOutput<StringData> GetResult(string input)
        {
            if (_availableAfter-- > 0)
                return null;

            return new ExternalTaskOutput<StringData>
            {
                OutputData = new StringData
                {
                    String = _result
                }
            };
        }
    }
}
