// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Schemas;
using WorkflowEngine.Config;
using WorkflowEngine.Logging.Interfaces;

namespace WorkflowEngine.Interfaces
{
    public interface IPluginServices
    {
        Task<PluginOutput<T>> PluginCompleted<T>(PluginData<T> data, string note = null)
            where T : Schema, new();

        Task<PluginOutput<T>> PluginFailed<T>(string failureReason)
            where T : Schema, new();

        Task<PluginOutput<T>> PluginIncomplete<T>()
            where T : Schema, new();

        PluginData<T> CreatePluginData<T>()
            where T : Schema, new();

        PluginData<T> CreatePluginData<T>(T data, PluginDataId id = null)
            where T : Schema, new();

        PluginDataList<T> CreatePluginDataList<T>()
            where T : Schema, new();

        /// <summary>
        /// Creates a new plugin, the developer can optionally provide a plugin name,
        /// otherwise this method relies on source code line numbers which is fine since
        /// one we instantiate a workflow, we store the DLL version that we used
        /// and always maintain a snapshot of this DLL version in storage
        ///
        /// This method should NEVER be called inside a loop, the plural version
        /// GetOrCreatePlugins() of the method should be used when multiple instances
        /// are needed
        /// TODO: Add FxCop check to make sure that this method is never called in a loop
        /// TODO: Add FxCop check to make sure that an explicitly provided plugin name matches the name of the
        ///       a newly declared plugin variable to avoid plugin name collision
        /// </summary>
        T GetOrCreatePlugin<T>(string pluginName = null, [CallerLineNumber]int callerLineNumber = 0)
            where T : Plugin, new();

        IEnumerable<T> GetOrCreatePlugins<T>(int numInstances, string pluginName = null, [CallerLineNumber]int callerLineNumber = 0)
            where T : Plugin, new();

        /// <summary>
        /// Used to load a plugin with a pre-existing context
        /// Allows re-execution of incomplete workflows
        /// </summary>
        T LoadPlugin<T>(PluginContext pluginContext)
            where T : Plugin, new();

        /// <summary>
        /// Returns true if all the plugins whose output is consumed have executed
        /// If there are plugins that produce outputs that aren't consumed, these are
        /// not guaranteed to have run and might even be pruned completely by the engine
        /// </summary>
        bool AllPluginsExecuted();

        /// <summary>
        /// Returns true if the current plugin has been cancelled
        ///
        /// A plugin that has been cancelled will still run, it's only up for the
        /// plugin developer to decide if they want to change their logic in case
        /// of cancellation.
        ///
        /// In the future we can experiment with other approaches, e.g. Only run
        /// a cancellation handler of a cancelled plugin, or only run the plugin once
        /// </summary>
        bool PluginCancelled();

        /// <summary>
        /// Cancels the whole execution
        /// </summary>
        void Cancel();

        ExecutionContext GetExecutionContext();

        void Initialize(ExecutionContext executionContext = null, IList<VariantConstraint> variants = null);

        ILogger GetLogger();

        void QueueWorkItem(WorkItem workItem);

        IPluginConfig GetConfig();

        object GetService(string serviceName);

        IEnumerable<string> GetPluginNotes();

        string GetPluginName();

        IList<Tuple<string, string>> GetVariantConstraints();
    }
}