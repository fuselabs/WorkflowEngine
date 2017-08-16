// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Schemas;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class PluginDataState : IState
    {
        public PluginDataId Id { get; set; }

        public Schema Data { get; set; }

        public Version Version { get; set; }

        public bool IsList { get; set; }

        public IEnumerable<Schema> DataList { get; set; }
    }
}