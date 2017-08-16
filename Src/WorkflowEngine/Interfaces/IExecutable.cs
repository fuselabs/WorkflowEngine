// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine.Interfaces
{
    /// <summary>
    /// An interface that represents a plugin's executable
    /// </summary>
    internal interface IExecutable
    {
        Execution Execute(object[] parameters, IState state, ExecutionContext executionContext);
    }
}
