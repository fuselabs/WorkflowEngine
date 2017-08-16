// Copyright (c) Microsoft Corporation. All rights reserved.

using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine.Interfaces
{
    /// <summary>
    /// Work queue for various workflow engine plugins to queue
    /// work that needs to happen at some point in the future
    /// e.g. A plugin might want some "nagging" logic after a timeout value
    ///      Or we might want to queue some incoming data while a worker is busy
    ///
    /// This allows us to use whatever mechanism we want for queuing work item
    /// e.g. queuing in memory using timers, or queuing in some form of persistent
    ///      storage like ServiceBus or Table Storage
    /// </summary>
    public interface IWorkQueue
    {
        ThreadingTask QueueWorkItem(WorkItem workItem);
    }
}
