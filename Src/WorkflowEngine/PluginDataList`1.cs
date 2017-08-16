// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using Schemas;

namespace WorkflowEngine
{
    public class PluginDataList<T> : PluginData<T>
        where T : Schema, new()
    {
        public PluginDataList() { }

        public PluginDataList(IEnumerable<Schema> data, PluginDataId id)
        {
            Id = id;
            PopulateData(data);
        }

        public PluginDataList(IEnumerable<Schema> data, PluginDataId id, Version version)
        {
            Id = id;
            Version = version;
            PopulateData(data);
        }

        #region Persistent State
        public new IList<T> Data { get; } = new List<T>();
        #endregion

        public void Add(T item)
        {
            Data.Add(item);
        }

        public override bool IsList()
        {
            return true;
        }

        public override IEnumerable<T> GetDataList()
        {
            return Data;
        }

        private void PopulateData(IEnumerable<Schema> data)
        {
            if (data == null)
                return;

            foreach (var element in data)
            {
                Data.Add(element as T);
            }
        }
    }
}
