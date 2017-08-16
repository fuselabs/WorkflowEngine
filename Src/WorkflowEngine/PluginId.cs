// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace WorkflowEngine
{
    /// <summary>
    /// Class representing UUID's for Plugins
    /// </summary>
    public class PluginId : ObjectId
    {
        public PluginId() { }

        public PluginId(string id) : base(new Guid(id)) { }
    }
}