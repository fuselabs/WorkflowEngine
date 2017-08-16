// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine.Interfaces
{
    /// <summary>
    /// Interface that workflow implementers can implement to provide services
    /// for their plugins
    /// </summary>
    public interface IServiceProvider
    {
        object GetService(string serviceName);
    }
}
