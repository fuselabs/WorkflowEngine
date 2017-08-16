// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace WorkflowEngine
{
    /// <summary>
    /// Class representing UUID's for PluginDatas
    /// </summary>
    public class PluginDataId : ObjectId
    {
        public PluginDataId()
        {
        }

        public PluginDataId(string id) : base(new Guid(id)) { }
    }
}