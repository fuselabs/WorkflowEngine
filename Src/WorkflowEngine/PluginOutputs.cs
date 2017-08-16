// Copyright (c) Microsoft Corporation. All rights reserved.

using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class PluginOutputs : PluginDataDictionary<PluginDataId>
    {
        public PluginOutputs() : base(id => id.Id)
        {
        }

        public PluginOutputs(IState persistentState, ExecutionContext executionContext) : base(persistentState, executionContext, id => id.Id)
        {
        }
    }
}
