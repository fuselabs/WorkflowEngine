// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace WorkflowEngine.Exceptions
{
    /// <summary>
    /// Exception used throughout the Workflow Engine to indicate an unexpected error
    /// This Exception Type should be only caught with caution (better to leave the application crash)
    /// Since it indicates serious runtime problems that probably indicate a bug
    /// </summary>
    public class WorkflowEngineException : Exception
    {
        public WorkflowEngineException(string message) : base(message) { }

        public WorkflowEngineException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
