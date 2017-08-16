// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public partial class PluginContext : IStateful
    {
        public PluginContext()
        {
            Plugins = new Dictionary<string, PluginId>();
        }

        public PluginContext(IState persistentState)
        {
            var state = persistentState as PluginContextState;
            if (state == null)
            {
                throw new WorkflowEngineException("PluginContext must be loaded with an object of type PluginContextState");
            }
            PluginId = state.PluginId;
            PluginName = state.PluginName;
            Cancelled = state.Cancelled;
            Plugins = state.Plugins;
            Version = state.Version;
            PluginInputIds = state.PluginInputIds;
            PluginOutputId = state.PluginOutputId;
            PluginState = state.PluginState;
            Note = state.Note;
        }

        /// <summary>
        /// Gets or sets id of the plugin
        /// </summary>
        public PluginId PluginId { get; set; }

        /// <summary>
        /// Gets or sets friendly Name of the plugin
        /// </summary>
        public string PluginName { get; set; }

        /// <summary>
        /// Gets or sets plugin name -> Plugin Data Id mapping
        /// </summary>
        public IDictionary<string, PluginDataId> PluginInputIds { get; set; }

        /// <summary>
        /// Gets or sets if the plugin has executed and returned an output, we store its ID here
        /// </summary>
        public PluginDataId PluginOutputId { get; set; }

        /// <summary>
        /// Gets or sets map unique identifier of a nested plugin to the plugin id
        /// </summary>
        public IDictionary<string, PluginId> Plugins { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates whether the plugin has been cancelled
        /// </summary>
        public bool Cancelled { get; set; }

        /// <summary>
        /// Gets or sets indicates the version of the plugin
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets note about the execution of a plugin (optional)
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets state of the plugin (instance variables) If this is a stateful plugin
        /// </summary>
        public IState PluginState { get; set; }

        private string DebuggerDisplay => $"PluginId: {PluginId}, PluginName: {PluginName}";

        /// <summary>
        /// Creates special Context that represents the "Root" of the toplevel workflow
        /// </summary>
        public static PluginContext CreateRootContext()
        {
            return new PluginContext { PluginName = "Root", PluginId = new PluginId("00000000000000000000000000000000") };
        }

        public IState Store()
        {
            return new PluginContextState
            {
                PluginId = PluginId,
                PluginName = PluginName,
                Plugins = Plugins,
                PluginInputIds = PluginInputIds,
                PluginOutputId = PluginOutputId,
                Cancelled = Cancelled,
                Version = Version,
                Note = Note,
                PluginState = PluginState
            };
        }
    }
}
