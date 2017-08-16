// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using QuickGraph;
using QuickGraph.Serialization;
using Schemas;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;
using WorkflowEngine.Utils;

namespace WorkflowEngine
{
    internal partial class DependencyGraph
    {
        /// <summary>
        /// Used to Store Plugins -> Creator Plugin relationships
        /// at runtime for dependency graph construction
        /// </summary>
        private readonly IDictionary<PluginDataId, PluginId> _createdBy = new Dictionary<PluginDataId, PluginId>();

        /// <summary>
        /// Used to Store Consumer Plugin -> Plugins relationships
        /// at runtime for dependency graph construction
        /// </summary>
        private readonly IDictionary<PluginId, ICollection<PluginDataId>> _consumes = new Dictionary<PluginId, ICollection<PluginDataId>>();

        /// <summary>
        /// Graph String Structure where we maintain the dependency info
        /// </summary>
        private readonly BidirectionalGraph<PluginId, SEdge<PluginId>> _graph = new BidirectionalGraph<PluginId, SEdge<PluginId>>();

        /// <summary>
        /// Set of Plugins that have finished execution
        /// </summary>
        private readonly ISet<PluginId> _executedPlugins = new HashSet<PluginId>();

        public DependencyGraph() { }

        public DependencyGraph(IState persistentState)
        {
            var state = persistentState as DependencyGraphState;
            if (state == null)
            {
                throw new WorkflowEngineException(
                    "DependencyGraph must be loaded with an object of type DependencyGraphState");
            }

            _executedPlugins = state.ExecutedPlugins;
            _consumes = state.Consumes.ToDictionary(kv => new PluginId(kv.Key), kv => kv.Value);
            _createdBy = state.CreatedBy.ToDictionary(kv => new PluginDataId(kv.Key), kv => kv.Value);
            _graph = new BidirectionalGraph<PluginId, SEdge<PluginId>>();
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.Write(state.Graph);
                    streamWriter.Flush();
                    stream.Position = 0;
                    using (var xmlReader = XmlReader.Create(stream))
                    {
                        var vertexFactory = new IdentifiableVertexFactory<PluginId>(pluginId => new PluginId(pluginId));
                        var edgeFactory = new IdentifiableEdgeFactory<PluginId, SEdge<PluginId>>((source, target, id) => new SEdge<PluginId>(source, target));
                        _graph.DeserializeFromGraphML(xmlReader, vertexFactory, edgeFactory);
                    }
                }
            }
        }

        public IEnumerable<PluginId> AllDependencies => _graph.Vertices;

        public void UpdateCreatedBy<T>(PluginContext pluginContext, PluginData<T> pluginData)
            where T : Schema, new()
        {
            if (!_createdBy.ContainsKey(pluginData.Id))
                _createdBy.Add(pluginData.Id, pluginContext.PluginId);
        }

        /// <summary>
        /// Adds the plugin as a consumer of all its inputs
        /// </summary>
        public void UpdateInputsConsumers(PluginContext pluginContext)
        {
            var pluginId = pluginContext.PluginId;
            foreach (var pluginInputId in pluginContext.PluginInputIds)
            {
                if (pluginInputId.Value != null)
                    UpdateDataConsumer(pluginId, pluginInputId.Value);
            }
        }

        public void UpdateExecutedPlugins(PluginId pluginId)
        {
            _executedPlugins.Add(pluginId);
        }

        public bool PluginHasExecuted(PluginId pluginId)
        {
            return _executedPlugins.Contains(pluginId);
        }

        public IEnumerable<PluginId> GetPluginDependencies(PluginId pluginId)
        {
            _graph.TryGetInEdges(pluginId, out IEnumerable<SEdge<PluginId>> inEdges);

            return inEdges?.Select(edge => edge.Source);
        }

        public IState Store()
        {
            string graphXml;
            using (var stream = new MemoryStream())
            {
                using (var streamReader = new StreamReader(stream))
                {
                    using (var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
                    {
                        var vertexId = new VertexIdentity<PluginId>(id => id.Id.ToString());
                        var edgeId = new EdgeIdentity<PluginId, SEdge<PluginId>>(edge => edge.Target.ToString());
                        _graph.SerializeToGraphML(xmlWriter, vertexId, edgeId);
                        xmlWriter.Flush();
                        stream.Position = 0;
                        graphXml = streamReader.ReadToEnd();
                    }
                }
            }
            return new DependencyGraphState
            {
                CreatedBy = GetFinalCreatedBy().ToDictionary(kv => kv.Key.Id.ToString(), kv => kv.Value),
                Consumes = GetFinalConsumes().ToDictionary(kv => kv.Key.Id.ToString(), kv => kv.Value),
                Graph = graphXml,
                ExecutedPlugins = _executedPlugins,
            };
        }

        private void UpdateDataConsumer(PluginId pluginId, PluginDataId dataId)
        {
            _consumes.SafeAdd(pluginId, dataId);
            UpdateDependencyGraph(pluginId, dataId);
        }

        /// <summary>
        /// Updates the Plugin-Plugin Dependency Graph
        /// </summary>
        private void UpdateDependencyGraph(PluginId destinationPluginId, PluginDataId dataId)
        {
            if (!_createdBy.ContainsKey(dataId))
            {
                throw new WorkflowEngineException("Cannot resolve data dependency");
            }
            var sourcePlugin = _createdBy[dataId];

            // Add edge only if it doesn't exist
            if (!_graph.TryGetEdge(sourcePlugin, destinationPluginId, out SEdge<PluginId> existingEdge))
            {
                _graph.AddVerticesAndEdge(new SEdge<PluginId>(sourcePlugin, destinationPluginId));
            }
        }

        /// <summary>
        /// Returns plugin data -> plugin relations that won't change and can be persisted
        /// </summary>
        private IEnumerable<KeyValuePair<PluginDataId, PluginId>> GetFinalCreatedBy()
        {
            return _createdBy.
                Where(kv => _executedPlugins.Contains(kv.Value)).
                ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Returns plugin -> plugin data relations that won't change and can be persisted
        /// </summary>
        private IEnumerable<KeyValuePair<PluginId, ICollection<PluginDataId>>> GetFinalConsumes()
        {
            return _consumes.
                Select(kv =>
                    new KeyValuePair<PluginId, ICollection<PluginDataId>>(
                        kv.Key,
                        new HashSet<PluginDataId>(kv.Value.Where(pluginDataId => _executedPlugins.Contains(_createdBy[pluginDataId]))))).
                ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
