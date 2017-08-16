// Copyright (c) Microsoft Corporation. All rights reserved.

using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class PluginInputs : PluginDataDictionary<string>
    {
        public PluginInputs() : base(str => str)
        {
        }

        public PluginInputs(IState persistentState, ExecutionContext executionContext) : base(persistentState, executionContext, str => str)
        {
        }
    }
}
