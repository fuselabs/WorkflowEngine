using WorkflowEngine.Interfaces;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Tests.Mocks.WorkQueue
{
    public class MockWorkQueue : IWorkQueue
    {
        ThreadingTask IWorkQueue.QueueWorkItem(WorkItem workItem)
        {
            throw new System.NotImplementedException();
        }
    }
}
