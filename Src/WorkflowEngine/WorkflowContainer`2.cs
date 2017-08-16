// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Schemas;
using WorkflowEngine.Config;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    /// <summary>
    /// This class is the main interface to the Workflow Engine
    /// It is  completely self contained and responisble for maintaining its own context
    ///
    /// Created containers must be initialized using the Initialize() method prior to using
    /// </summary>
    public class WorkflowContainer<TWorkflow, TOutput> : IWorkflowContainer<TWorkflow, TOutput>
        where TWorkflow : Workflow, new()
        where TOutput : Schema, new()
    {
        private readonly IPluginServices _pluginServices;

        /// <summary>
        /// This context object should contain all the container's state
        /// </summary>
        private WorkflowContainerContext _containerContext;

        /// <summary>
        /// Flag to indicate whether this container has been properly initialized
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// Root workflow output
        /// </summary>
        private PluginData<TOutput> _rootWorkflowOutput;

        public WorkflowContainer(IPluginServices pluginServices)
        {
            _pluginServices = pluginServices;
        }

        public PluginData<T> CreateWorkflowInput<T>()
            where T : Schema, new()
        {
            EnsureContainerIsInitialized();
            return _pluginServices.CreatePluginData<T>();
        }

        public PluginData<T> GetWorkflowInput<T>(string inputName)
            where T : Schema, new()
        {
            EnsureContainerIsInitialized();
            if (_containerContext.ContainerInputs == null || !_containerContext.ContainerInputs.TryGetValue(inputName, out IPluginData<Schema> oldInput))
            {
                throw new WorkflowEngineException("Cannot get a non existing input parameter");
            }
            return _pluginServices.CreatePluginData(oldInput.Data as T, oldInput.Id);
        }

        public PluginData<TOutput> GetOutput()
        {
            return _rootWorkflowOutput;
        }

        async Task<WorkflowContainerExecutionResult> IWorkflowContainer<TWorkflow, TOutput>.Execute(PluginInputs workflowInputs)
        {
            EnsureContainerIsInitialized();
            if (_containerContext.Executed)
            {
                throw new WorkflowEngineException("Cannot execute workflow container more than once.");
            }

            var rootWorkflow = _pluginServices.LoadPlugin<TWorkflow>(_containerContext.RootPluginContext);

            _containerContext.Executed = true;
            _containerContext.ContainerInputs = workflowInputs ?? throw new WorkflowEngineException("Cannot execute container with null inputs");
            _containerContext.RootPluginContext = rootWorkflow.GetContext();
            _containerContext.ExecutionContext = _pluginServices.GetExecutionContext();

            _rootWorkflowOutput = await rootWorkflow.Execute<TOutput>(workflowInputs).ConfigureAwait(false);
            return _rootWorkflowOutput == null ? WorkflowContainerExecutionResult.PartiallyCompleted : WorkflowContainerExecutionResult.Completed;
        }

        public async Task<WorkflowContainerExecutionResult> ReExecute(PluginInputs newInputs = null)
        {
            EnsureContainerIsInitialized();
            if (!_containerContext.Executed)
            {
                throw new WorkflowEngineException("Cannot re-execute workfor container before executing it.");
            }
            var rootWorkflow = _pluginServices.LoadPlugin<TWorkflow>(_containerContext.RootPluginContext);
            UpdatePluginInputs(_containerContext.ContainerInputs, newInputs);
            try
            {
                _rootWorkflowOutput = await rootWorkflow.Execute<TOutput>(_containerContext.ContainerInputs).ConfigureAwait(false);
            }
            catch (VersionMismatchException)
            {
                return WorkflowContainerExecutionResult.NotExecuted;
            }
            return _rootWorkflowOutput == null ? WorkflowContainerExecutionResult.PartiallyCompleted : WorkflowContainerExecutionResult.Completed;
        }

        public bool RanToCompletion()
        {
            EnsureContainerIsInitialized();
            return _rootWorkflowOutput != null;
        }

        public void Initialize(WorkflowContainerContext context = null, IEnumerable<Tuple<string, string>> variants = null)
        {
            var variantConstarints = variants?.Select(v => new VariantConstraint(v.Item1, v.Item2)).ToList();
            if (context == null)
            {
                _pluginServices.Initialize(variants: variantConstarints);
                _containerContext = new WorkflowContainerContext
                {
                    ExecutionContext = _pluginServices.GetExecutionContext(),
                    Executed = false,
                    Variants = variantConstarints
                };
            }
            else
            {
                _containerContext = context;
                _pluginServices.Initialize(context.ExecutionContext, context.Variants);
            }
            _isInitialized = true;
        }

        public WorkflowContainerContext GetContext()
        {
            return _containerContext;
        }

        public bool TryLoad(string persistentState)
        {
            if (TryDeserializState(persistentState, out WorkflowContextState state))
            {
                var containerContext = new WorkflowContainerContext(state);
                Initialize(containerContext);
                return true;
            }

            return false;
        }

        public string Store()
        {
            return JsonConvert.SerializeObject(_containerContext.Store());
        }

        public Version WorkflowVersion()
        {
            return _containerContext.RootPluginContext.Version;
        }

        private static void UpdatePluginInputs(PluginInputs oldInputs, PluginInputs newInputs)
        {
            if (newInputs == null)
                return;

            foreach (var kv in newInputs)
            {
                var oldInput = oldInputs[kv.Key];
                var newInput = kv.Value;
                if (!Equals(oldInput.Id, newInput.Id))
                    throw new WorkflowEngineException("Attempting updating an input with a different one. Use GetWorkflowInput()");

                oldInputs[kv.Key] = kv.Value;
            }
        }

        private void EnsureContainerIsInitialized()
        {
            if (!_isInitialized)
            {
                throw new WorkflowEngineException("Workflow Container hasn't been initialized before usage");
            }
        }

        private bool TryDeserializState(string persistentState, out WorkflowContextState state)
        {
            try
            {
                state = JsonConvert.DeserializeObject<WorkflowContextState>(persistentState);
            }
            catch
            {
                state = null;
                _pluginServices.GetLogger().Warn("Failed to deserialize workflow state");
                return false;
            }

            return true;
        }
    }
}
