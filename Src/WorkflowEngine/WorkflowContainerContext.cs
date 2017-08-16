// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using WorkflowEngine.Config;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    /// <summary>
    /// Contains all persistent context for a workflow container
    /// </summary>
    public class WorkflowContainerContext : IStateful
    {
        public WorkflowContainerContext() { }

        public WorkflowContainerContext(IState persistentState)
        {
            var state = persistentState as WorkflowContextState;
            if (state == null)
            {
                throw new WorkflowEngineException("WorkflowContainerContext must be loaded with an object of type WorkflowContextState");
            }
            ExecutionContext = new ExecutionContext(state.ExecutionContext);
            ContainerInputs = new PluginInputs(state.ContainerInputs, ExecutionContext);
            RootPluginContext = new PluginContext(state.RootPluginContext);

            // Sync Root Plugin Context with Execution Context (Pointer Swizzling)
            RootPluginContext = ExecutionContext.GetPluginContextFromId(RootPluginContext.PluginId);
            Executed = state.Executed;
            Variants = state.Variants;
        }

        public ExecutionContext ExecutionContext { get; set; }

        public PluginContext RootPluginContext { get; set; }

        public PluginInputs ContainerInputs { get; set; }

        public IList<VariantConstraint> Variants { get; set; }

        public bool Executed { get; set; }

        public IState Store()
        {
            return new WorkflowContextState
            {
                ContainerInputs = ContainerInputs.Store(),
                RootPluginContext = RootPluginContext.Store(),
                ExecutionContext = ExecutionContext.Store(),
                Executed = Executed,
                Variants = Variants
            };
        }
    }
}
