// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Schemas;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public partial class ExecutionContext : IStateful
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// PluginServices object that created this execution context
        /// This allows all plugins to inject the pluginServices object into plugins/data created within this execution context
        /// </summary>
        internal IPluginServices PluginServices;
#pragma warning restore SA1401 // Fields must be private

        /// <summary>
        /// Structure that represents the plugin depdendency graph for this workflow execution context
        /// </summary>
        private readonly DependencyGraph _dependencyGraph = new DependencyGraph();

        /// <summary>
        /// Table of all created plugin contexts and the only location where we actually store
        /// the plugin context. All other plugin context pointers point here
        /// </summary>
        private readonly IDictionary<PluginId, PluginContext> _pluginContexts = new Dictionary<PluginId, PluginContext>();

        /// <summary>
        /// Table of all plugin outputs
        /// </summary>
        private readonly PluginOutputs _pluginOutputs = new PluginOutputs();

        /// <summary>
        /// Stack representing the current plugin runtime
        /// </summary>
        private readonly Stack<PluginId> _pluginRuntimeStack = new Stack<PluginId>();

        /// <summary>
        /// Indicates whether this execution has been cancelled
        /// </summary>
        private bool _cancelled;

        public ExecutionContext()
        {
            CreateRootContext();
        }

        public ExecutionContext(IState persistentState)
        {
            var state = persistentState as ExecutionContextState;
            if (state == null)
            {
                throw new WorkflowEngineException("ExecutionContext must be loaded with an object of type ExecutionContextState");
            }
            _pluginContexts = state.PluginContexts.ToDictionary(kv => new PluginId(kv.Key), kv => new PluginContext(kv.Value));
            _dependencyGraph = new DependencyGraph(state.DependencyGraph);
            _cancelled = state.Cancelled;
            _pluginOutputs = new PluginOutputs(state.PluginOutputs, this);
            CreateRootContext();
        }

        /// <summary>
        /// Gets the context of the currently executing plugin
        /// This is basically a pointer to the top of the plugin runtime stack
        /// </summary>
        public PluginContext CurrentPluginContext => _pluginContexts[_pluginRuntimeStack.Peek()];

        public void SetPluginServices(IPluginServices pluginServices)
        {
            PluginServices = pluginServices;
        }

        /// <summary>
        /// Signals that a plugin is going to execute now
        /// </summary>
        /// <param name="pluginContext">ExecutionContext of the plugin about to execute</param>
        public void StartPluginExecution(PluginContext pluginContext)
        {
            _pluginRuntimeStack.Push(pluginContext.PluginId);
        }

        /// <summary>
        /// Updates the consumers list for the inputs of this plugin to include the plugin
        /// </summary>
        public void UpdateInputConsumers(PluginContext pluginContext)
        {
            _dependencyGraph.UpdateInputsConsumers(pluginContext);
        }

        /// <summary>
        /// Signals that the last executed plugin is going to end now
        /// </summary>
        public void EndPluginExecution<T>(bool pluginSucceeded, PluginData<T> plugingOutput)
            where T : Schema, new()
        {
            if (pluginSucceeded)
            {
                CurrentPluginContext.PluginOutputId = plugingOutput?.Id;
                _dependencyGraph.UpdateExecutedPlugins(CurrentPluginContext.PluginId);
                _pluginOutputs[plugingOutput?.Id] = plugingOutput;
            }
            _pluginRuntimeStack.Pop();
        }

        public void CreatePluginData<T>(PluginData<T> pluginData)
            where T : Schema, new()
        {
            _dependencyGraph.UpdateCreatedBy(CurrentPluginContext, pluginData);
        }

        public bool CurrentPluginDependenciesFulfilled()
        {
            var pluginId = CurrentPluginContext.PluginId;

            var dependencies = _dependencyGraph.GetPluginDependencies(pluginId);
            return dependencies.Where(dependency => !_pluginRuntimeStack.Contains(dependency))
                                .All(dependency => _dependencyGraph.PluginHasExecuted(dependency));
        }

        public bool AllDependenciesFulFilled()
        {
            var allDependencies = _dependencyGraph.AllDependencies;
            return allDependencies.All(dependency => _dependencyGraph.PluginHasExecuted(dependency));
        }

        public bool PluginExecuted(PluginContext pluginContext)
        {
            return _dependencyGraph.PluginHasExecuted(pluginContext.PluginId);
        }

        public void AddPluginContext(PluginContext pluginContext)
        {
            _pluginContexts[pluginContext.PluginId] = pluginContext;
        }

        public PluginContext GetPluginContextFromId(PluginId id)
        {
            return _pluginContexts[id];
        }

        public PluginData<T> GetPluginOutputFromId<T>(PluginDataId id)
            where T : Schema, new()
        {
            return _pluginOutputs[id] as PluginData<T>;
        }

        public void Cancel()
        {
            _cancelled = true;
        }

        public bool IsCancelled()
        {
            return _cancelled;
        }

        public IEnumerable<string> GetPluginNotes()
        {
            return _pluginContexts.Values.Where(pc => !string.IsNullOrEmpty(pc.Note)).Select(pc => pc.Note);
        }

        public IState Store()
        {
            return new ExecutionContextState
            {
                DependencyGraph = _dependencyGraph.Store(),
                PluginContexts = _pluginContexts.ToDictionary(kv => kv.Key.Id, kv => kv.Value.Store()),
                Cancelled = _cancelled,
                PluginOutputs = _pluginOutputs?.Store()
            };
        }

        private void CreateRootContext()
        {
            // SetPluginServices Runtime Stack with a special "Root" Stack Frame
            var rootContext = PluginContext.CreateRootContext();
            _pluginRuntimeStack.Push(rootContext.PluginId);
            _pluginContexts[rootContext.PluginId] = rootContext;

            // Root is always considered 'executed'
            _dependencyGraph.UpdateExecutedPlugins(rootContext.PluginId);
        }
    }
}
