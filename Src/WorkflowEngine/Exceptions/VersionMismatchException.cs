// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace WorkflowEngine.Exceptions
{
    public class VersionMismatchException : WorkflowEngineException
    {
        public VersionMismatchException(string message) : base(message)
        {
        }

        public VersionMismatchException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
