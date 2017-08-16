// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public partial class ExecutionContext : IStateful
    {
        public class ExecutionContextState : IState
        {
            public IState DependencyGraph { get; set; }

            public IDictionary<string, IState> PluginContexts { get; set; }

            public bool Cancelled { get; set; }

            public IState PluginOutputs { get; set; }
        }
    }
}
