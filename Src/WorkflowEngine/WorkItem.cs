// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine
{
    public enum WorkItemType
    {
        ReExecuteWorkflow
    }

    /// <summary>
    /// Work Item that can be queued into a work queue to be
    /// executed at a future point in time
    /// </summary>
    public class WorkItem
    {
        /// <summary>
        /// Gets or sets work Item Type
        /// </summary>
        public WorkItemType Type { get; set; }

        /// <summary>
        /// Gets or sets delay in milliseconds
        /// </summary>
        public long DelayInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets work Item Data
        /// </summary>
        public object Data { get; set; }
    }
}
