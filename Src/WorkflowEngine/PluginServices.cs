// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Schemas;
using WorkflowEngine.Config;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Logging.Interfaces;
using IServiceProvider = WorkflowEngine.Interfaces.IServiceProvider;
using ThreadingTask = System.Threading.Tasks.Task;

namespace WorkflowEngine
{
    /// <summary>
    /// A new Object of this type should be created for every execution
    /// and the object should not be re-used because it preservers state
    ///
    /// Please see the interface for documentation of public methods
    /// </summary>
    public class PluginServices : IPluginServices
    {
        /// <summary>
        /// Queue to queue work items
        /// </summary>
        private readonly IWorkQueue _workQueue;

        /// <summary>
        /// Plugin Configurations
        /// </summary>
        private readonly VariantConfigPluginConfig _pluginConfig;

        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Service provider
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Variant Constraints the Plugin Services were initialized with
        /// </summary>
        private IEnumerable<VariantConstraint> _variantConstraints;

        /// <summary>
        /// This execution context is the only state this class is allowed to have
        /// Any state should be contained within the execution context
        /// </summary>
        private ExecutionContext _executionContext;

        public PluginServices(
            IWorkQueue workQueue,
            IPluginConfig pluginConfig,
            ILogger logger,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _workQueue = workQueue;
            _pluginConfig = new VariantConfigPluginConfig(pluginConfig);
            _logger = logger;
        }

        public void Initialize(ExecutionContext executionContext = null, IList<VariantConstraint> variants = null)
        {
            _executionContext = executionContext ?? new ExecutionContext();
            _executionContext.SetPluginServices(this);
            _variantConstraints = variants;
            _pluginConfig.Initialize(variants);
        }

        public Task<PluginOutput<T>> PluginCompleted<T>(PluginData<T> data, string note = null)
            where T : Schema, new()
        {
            if (!string.IsNullOrEmpty(note))
                _executionContext.CurrentPluginContext.Note = note;
            return ThreadingTask.FromResult(new PluginOutput<T>(true, data));
        }

        public Task<PluginOutput<T>> PluginFailed<T>(string failureReason)
            where T : Schema, new()
        {
            _executionContext.CurrentPluginContext.Note = failureReason;
            return ThreadingTask.FromResult(new PluginOutput<T>(true, CreatePluginData<T>()));
        }

        public Task<PluginOutput<T>> PluginIncomplete<T>()
            where T : Schema, new()
        {
            return ThreadingTask.FromResult(new PluginOutput<T>(false, null));
        }

        public PluginData<T> CreatePluginData<T>()
            where T : Schema, new()
        {
            var data = new PluginData<T> { ExecutionContext = _executionContext };
            return data;
        }

        public PluginData<T> CreatePluginData<T>(T data, PluginDataId id = null)
            where T : Schema, new()
        {
            return id == null
                ? new PluginData<T>(data) { ExecutionContext = _executionContext }
                : new PluginData<T>(data, id) { ExecutionContext = _executionContext };
        }

        public PluginDataList<T> CreatePluginDataList<T>()
            where T : Schema, new()
        {
            var data = new PluginDataList<T> { ExecutionContext = _executionContext };
            return data;
        }

        public T GetOrCreatePlugin<T>(string pluginName = null, [CallerLineNumber]int callerLineNumber = -1)
            where T : Plugin, new()
        {
            var pluginIdentifier = GetPluginId(pluginName, callerLineNumber);
            return GetOrCreatePluginInternal<T>(pluginIdentifier);
        }

        public IEnumerable<T> GetOrCreatePlugins<T>(int numInstances, string pluginName = null, [CallerLineNumber]int callerLineNumber = -1)
            where T : Plugin, new()
        {
            var basePluginIdentifer = GetPluginId(pluginName, callerLineNumber);
            var plugins = new T[numInstances];
            for (var i = 0; i < numInstances; i++)
            {
                var pluginIdentifier = $"{basePluginIdentifer}_{i}";
                plugins[i] = GetOrCreatePluginInternal<T>(pluginIdentifier);
            }
            return plugins;
        }

        public T LoadPlugin<T>(PluginContext pluginContext)
            where T : Plugin, new()
        {
            var plugin = new T();
            plugin.Initialize(_executionContext, pluginContext);

            return plugin;
        }

        public bool AllPluginsExecuted()
        {
            return _executionContext.AllDependenciesFulFilled();
        }

        public bool PluginCancelled()
        {
            return _executionContext.IsCancelled() || _executionContext.CurrentPluginContext.Cancelled;
        }

        public void Cancel()
        {
            _executionContext.Cancel();
        }

        public ExecutionContext GetExecutionContext()
        {
            return _executionContext;
        }

        public void QueueWorkItem(WorkItem workItem)
        {
            _workQueue.QueueWorkItem(workItem);
        }

        public IPluginConfig GetConfig()
        {
            return _pluginConfig;
        }

        public object GetService(string serviceName)
        {
            return _serviceProvider.GetService(serviceName);
        }

        public IEnumerable<string> GetPluginNotes()
        {
            return _executionContext.GetPluginNotes();
        }

        public ILogger GetLogger()
        {
            return _logger;
        }

        public string GetPluginName()
        {
            return _executionContext?.CurrentPluginContext?.PluginName;
        }

        public IList<Tuple<string, string>> GetVariantConstraints()
        {
            return _variantConstraints?.Select(vc => Tuple.Create(vc.Key, vc.Value)).ToList();
        }

        private static string GetPluginId(string explicitId, int implicitId)
        {
            if (!string.IsNullOrEmpty(explicitId))
                return explicitId;

            if (implicitId == -1)
            {
                throw new WorkflowEngineException("No explicit Id provided and CallerInfo not being populated");
            }

            return implicitId.ToString(CultureInfo.InvariantCulture);
        }

        private T GetOrCreatePluginInternal<T>(string id)
            where T : Plugin, new()
        {
            var currentPluginContext = _executionContext.CurrentPluginContext;
            var currentedPluginNestedPlugins = currentPluginContext.Plugins;
            PluginContext pluginContext = null;
            if (currentedPluginNestedPlugins.ContainsKey(id))
            {
                var pluginId = currentedPluginNestedPlugins[id];
                pluginContext = _executionContext.GetPluginContextFromId(pluginId);
            }

            var plugin = new T();
            plugin.Initialize(_executionContext, pluginContext);
            currentedPluginNestedPlugins[id] = plugin.PluginId;

            return plugin;
        }
    }
}
