// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Schemas;

namespace WorkflowEngine.Interfaces
{
    public interface IPluginData<out T> : IStateful
        where T : Schema
    {
        /// <summary>
        /// Gets or sets a unique identifier for this data object
        /// </summary>
        PluginDataId Id { get; set; }

        ExecutionContext ExecutionContext { get; set; }

        T Data { get; }

        Version Version { get; }

        bool IsList();

        IEnumerable<T> GetDataList();
    }
}
