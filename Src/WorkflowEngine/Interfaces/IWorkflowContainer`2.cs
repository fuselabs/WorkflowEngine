// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Schemas;

namespace WorkflowEngine.Interfaces
{
    public enum WorkflowContainerExecutionResult
    {
        NotExecuted,
        PartiallyCompleted,
        Completed
    }

    public interface IWorkflowContainer<TWorkflow, TOutput>
        where TWorkflow : Workflow, new()
        where TOutput : Schema, new()
    {
        Task<WorkflowContainerExecutionResult> Execute(PluginInputs workflowInputs);

        Task<WorkflowContainerExecutionResult> ReExecute(PluginInputs newInputs = null);

        PluginData<T> CreateWorkflowInput<T>()
            where T : Schema, new();

        PluginData<T> GetWorkflowInput<T>(string inputName)
            where T : Schema, new();

        PluginData<TOutput> GetOutput();

        void Initialize(WorkflowContainerContext executionContext = null, IEnumerable<Tuple<string, string>> variants = null);

        WorkflowContainerContext GetContext();

        bool TryLoad(string persistentState);

        string Store();

        Version WorkflowVersion();
    }
}
