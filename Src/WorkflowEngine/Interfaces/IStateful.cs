// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine.Interfaces
{
    /// <summary>
    /// Interface for classes that have persistent state
    /// </summary>
    public interface IStateful
    {
        IState Store();
    }
}
