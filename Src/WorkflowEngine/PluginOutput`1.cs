// Copyright (c) Microsoft Corporation. All rights reserved.

using Schemas;

namespace WorkflowEngine
{
    public class PluginOutput<T>
        where T : Schema, new()
    {
        public PluginOutput(bool success, PluginData<T> data)
        {
            Data = data;
            Success = success;
        }

        public bool Success { get; set; }

        public PluginData<T> Data { get; set; }
    }
}
