// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class Execution : IStateful
    {
        private readonly object _result;
        private readonly Func<IState> _stateFunc;

        public Execution(object result, Func<IState> stateFunc)
        {
            _result = result;
            _stateFunc = stateFunc;
        }

        public IState Store()
        {
            return _stateFunc?.Invoke();
        }

        public object GetResult()
        {
            return _result;
        }

        public class ExecutionState : IState
        {
            public IDictionary<string, IState> PluginFields { get; } = new Dictionary<string, IState>();
        }
    }
}