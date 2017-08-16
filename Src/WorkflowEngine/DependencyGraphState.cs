// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    internal partial class DependencyGraph : IStateful
    {
        internal class DependencyGraphState : IState
        {
            public IDictionary<string, PluginId> CreatedBy { get; set; }

            public IDictionary<string, ICollection<PluginDataId>> Consumes { get; set; }

            public string Graph { get; set; }

            public ISet<PluginId> ExecutedPlugins { get; set; }
        }
    }
}
