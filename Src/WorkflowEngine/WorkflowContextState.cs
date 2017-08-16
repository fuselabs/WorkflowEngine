// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using WorkflowEngine.Config;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class WorkflowContextState : IState
    {
        public IState ContainerInputs { get; set; }

        public IState RootPluginContext { get; set; }

        public IState ExecutionContext { get; set; }

        public bool Executed { get; set; }

        public IList<VariantConstraint> Variants { get; set; }
    }
}
