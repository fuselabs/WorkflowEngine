// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine.Interfaces
{
    public interface IPluginConfig
    {
        string Get(string configKey);
    }
}