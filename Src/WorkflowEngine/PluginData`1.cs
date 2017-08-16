// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Schemas;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class PluginData<T> : IPluginData<T>
        where T : Schema, new()
    {
        private ExecutionContext _executionContext;

        public PluginData() : this(new T())
        {
        }

        public PluginData(T data) : this(data, new PluginDataId())
        {
        }

        public PluginData(T data, PluginDataId id) : this(data, id, GetSchemaVersion(data))
        {
        }

        public PluginData(T data, PluginDataId id, Version version)
        {
            Id = id;
            Data = data;
            Version = version;
        }

        #region Persistent State
        public PluginDataId Id { get; set; }

        public T Data { get; private set; }

        public Version Version { get; set; }
        #endregion

        /// <summary>
        /// Gets or sets workflow context in which this data object is created/used
        /// </summary>
        [JsonIgnore]
        public ExecutionContext ExecutionContext
        {
            get => _executionContext;

            set
            {
                _executionContext = value;
                _executionContext.CreatePluginData(this);
            }
        }

        public static dynamic CreatePluginData(PluginDataState state, ExecutionContext executionContext)
        {
            if (state == null)
                return null;

            if (!state.IsList)
            {
                var type = typeof(PluginData<>).MakeGenericType(state.Data.GetType());
                var data = Activator.CreateInstance(type, state.Data, state.Id, state.Version);
                var execContext = type.GetField("_executionContext", BindingFlags.Instance | BindingFlags.NonPublic);
                execContext?.SetValue(data, executionContext);
                return data;
            }
            else
            {
                var type = typeof(PluginDataList<>).MakeGenericType(state.Data.GetType());
                var data = Activator.CreateInstance(type, state.DataList, state.Id, state.Version);
                var execContext = type.BaseType.GetField("_executionContext", BindingFlags.Instance | BindingFlags.NonPublic);
                execContext?.SetValue(data, executionContext);
                return data;
            }
        }

        public virtual bool IsList()
        {
            return false;
        }

        public virtual IEnumerable<T> GetDataList()
        {
            throw new WorkflowEngineException("Not List Type");
        }

        public virtual IState Store()
        {
            return new PluginDataState
            {
                Id = Id,
                Data = Data,
                Version = Version,
                IsList = IsList(),
                DataList = IsList() ? GetDataList() : null
            };
        }

        private static Version GetSchemaVersion(T data)
        {
            var schemaType = data.GetType();
            if (schemaType.CustomAttributes == null)
                return Version.DefaultVersion;

            var versionAttributes = schemaType.GetCustomAttributes(typeof(Version), true);

            // For now we'll allow schemas to not specify a version for inputs for back compat, in the future we might enforce it
            if (versionAttributes.Length == 1)
            {
                return (Version)versionAttributes[0];
            }

            return Version.DefaultVersion;
        }
    }
}
