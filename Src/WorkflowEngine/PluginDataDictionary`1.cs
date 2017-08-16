// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Schemas;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class PluginDataDictionary<T> : Dictionary<string, IPluginData<Schema>>, IStateful
    {
        private readonly Func<T, string> _keyFactory;

        public PluginDataDictionary(Func<T, string> keyFactory)
        {
            _keyFactory = keyFactory;
        }

        public PluginDataDictionary(IState persistentState, ExecutionContext executionContext, Func<T, string> keyFactory) : this(keyFactory)
        {
            var state = persistentState as PluginDataDictionaryState;
            if (state == null)
                return;
            foreach (var pair in state.Inputs)
            {
                var pluginDataState = pair.Value as PluginDataState;
                this[pair.Key] = PluginData<Schema>.CreatePluginData(pluginDataState, executionContext);
            }
        }

        public IPluginData<Schema> this[T key]
        {
            get => this[_keyFactory(key)];
            set => this[_keyFactory(key)] = value;
        }

        public IState Store()
        {
            IDictionary<string, IState> pluginInputs = new Dictionary<string, IState>();
            foreach (var pair in this)
            {
                pluginInputs[pair.Key] = pair.Value?.Store();
            }
            return new PluginDataDictionaryState
            {
                Inputs = pluginInputs
            };
        }

        public class PluginDataDictionaryState : IState
        {
            public IDictionary<string, IState> Inputs { get; set; }
        }
    }
}
