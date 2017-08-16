// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public partial class PluginContext
    {
        public class PluginContextState : IState
        {
            public PluginId PluginId { get; set; }

            public string PluginName { get; set; }

            public IDictionary<string, PluginId> Plugins { get; set; }

            public IDictionary<string, PluginDataId> PluginInputIds { get;  set; }

            public PluginDataId PluginOutputId { get; set; }

            public bool Cancelled { get; set; }

            public Version Version { get; set; }

            public string Note { get; set; }

            public IState PluginState { get; set; }
        }
    }
}
