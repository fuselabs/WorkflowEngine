// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Reflection;
using Schemas;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    public class StatefulExecutable : IExecutable
    {
        private const BindingFlags FieldBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly Type _type;
        private readonly MethodInfo _methodInfo;

        public StatefulExecutable(MethodInfo mi, Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _methodInfo = mi ?? throw new ArgumentNullException(nameof(mi));
        }

        public Execution Execute(object[] parameters, IState state, ExecutionContext executionContext)
        {
            var instance = Activator.CreateInstance(_type);
            if (instance == null)
            {
                throw new WorkflowEngineException($"Unable to create an instance of Plugin Type: {_type}");
            }
            LoadState(instance, state, executionContext);

            // Since the plugins are async, we need to delay the state evaluation by capturing
            // it in a lambda till after the result has been awaited in Plugin::Execute() to
            // guarantee that the plugin state has been evaluated and avoid a race condition
            return new Execution(_methodInfo.Invoke(instance, parameters), () => GetState(instance));
        }

        private void LoadState(object instance, IState state, ExecutionContext executionContext)
        {
            var executionState = state as Execution.ExecutionState;

            // This means that plugin doesn't have state yet
            if (executionState == null)
                return;

            foreach (var pair in executionState.PluginFields)
            {
                var field = _type.GetField(pair.Key, FieldBindingFlags);

                if (field == null)
                    throw new WorkflowEngineException($"Unable to find state property with name: {pair.Key} on type: {_type}");

                var pluginDataState = pair.Value as PluginDataState;
                var fieldValue = PluginData<Schema>.CreatePluginData(pluginDataState, executionContext);

                try
                {
                    field.SetValue(instance, fieldValue);
                }
                catch (Exception ex)
                {
                    throw new WorkflowEngineException($"Unable to set state property with name:{pair.Key} on type: {_type}", ex);
                }
            }
        }

        private IState GetState(object instance)
        {
            var state = new Execution.ExecutionState();

            // We might want to force plugin developers to use private fields only later on
            var fields = _type.GetFields(FieldBindingFlags);

            foreach (var field in fields)
            {
                if (field == null)
                    throw new WorkflowEngineException($"One of the fields on the type: {_type} is null");

                if (field.FieldType.GetGenericTypeDefinition() != typeof(PluginData<>))
                    throw new WorkflowEngineException("Plugin Fields must all be of type PluginData<>");

                var fieldName = field.Name;
                var fieldValue = field.GetValue(instance);

                // If the field value is null, no need to store it
                if (fieldValue == null)
                    continue;

                var fieldSchemaValue = fieldValue as IPluginData<Schema>;

                if (fieldSchemaValue == null)
                    throw new WorkflowEngineException($"Unable to cast state property: {fieldName} in type: {_type} to IPluginData<Schema>");

                state.PluginFields[fieldName] = fieldSchemaValue.Store();
            }
            return state;
        }
    }
}
