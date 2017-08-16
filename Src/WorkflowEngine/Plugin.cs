// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using log4net;
using Schemas;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

[assembly: InternalsVisibleTo("WorkflowEngine.Tests")]
namespace WorkflowEngine
{
    /// <summary>
    /// Class representing a unit of work (e.g. Task or Workflow)
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ", nq}")]
    public abstract class Plugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Plugin));

        /// <summary>
        /// Execution context in which the plugin will run
        /// </summary>
        private ExecutionContext _executionContext;

        /// <summary>
        /// Flag to determine if the plugin has been properly initialized
        /// </summary>
        private bool _isInitialized;

        protected Plugin()
        {
            ExecInfo = RuntimeCache.GetExecInfo(GetType());
        }

        public PluginId PluginId => PluginContext.PluginId;

        /// <summary>
        /// Gets or sets plugin context, all the plugin's state that we need to persist should
        /// be contained in this context object
        /// </summary>
        private PluginContext PluginContext { get; set; }

        /// <summary>
        /// Gets runtime information about the plugin
        /// </summary>
        private RuntimeCache.CachedMethodInfo ExecInfo { get; }

        private string DebuggerDisplay => $"Id: {PluginContext.PluginId}, Name: {PluginContext.PluginName}";

        /// <summary>
        /// Uses reflection to safely execute a Typed "ExecutePlugin" method that is defined in a Task/Workflow
        /// </summary>
        /// <param name="actualParams">Key-Value pairs representing Typed variable and their names</param>
        public virtual async Task<PluginData<T>> Execute<T>(PluginInputs actualParams)
            where T : Schema, new()
        {
            if (actualParams == null)
            {
                throw new WorkflowEngineException("ContainerInputs passed to a plugin cannot be null");
            }

            PluginContext.PluginInputIds = actualParams.ToDictionary(kv => kv.Key, kv => kv.Value?.Id);
            return await ExecuteSafely<T>(actualParams).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancels the plugin.
        /// </summary>
        public void Cancel()
        {
            PluginContext.Cancelled = true;
        }

        internal PluginContext GetContext()
        {
            return PluginContext;
        }

        internal void Initialize(ExecutionContext executionContext, PluginContext pluginContext)
        {
            _executionContext = executionContext ?? throw new WorkflowEngineException("Cannot initalize plugin with a null execution context");

            if (pluginContext != null)
            {
                PluginContext = pluginContext;
            }
            else
            {
                PluginContext = new PluginContext
                {
                    PluginId = new PluginId(),
                    PluginName = ExecInfo.PluginName,
                    Version = ExecInfo.Version
                };
                _executionContext.AddPluginContext(PluginContext);
            }
            _isInitialized = true;
        }

        /// <summary>
        /// Returns true only if:
        /// 1. All single requird inputs are not null
        /// 2. All list required inputs are not null
        /// 3. Every item in non-null list inputs are not null
        /// </summary>
        private static bool RequiredInputsNotNull(PluginInputs actualParams, ParameterInfo[] expectedParams)
        {
            foreach (var ap in actualParams)
            {
                var pi = expectedParams.FirstOrDefault(ep => ep.Name == ap.Key);
                if (pi != null && pi.CustomAttributes.All(a => a.AttributeType != typeof(OptionalAttribute)) && ap.Value == null)
                    return false;
            }

            return !actualParams.Values.Any(param => param != null && param.IsList() && param.GetDataList().Any(item => item == null));
        }

        private static Version GetExpectedParamVersion(ParameterInfo expectedParamInfo)
        {
            if (expectedParamInfo.CustomAttributes == null)
                return Version.DefaultVersion;

            var versionAttributes = expectedParamInfo.GetCustomAttributes(typeof(Version), true);

            // For now we'll allow plugins to not specify a version for inputs for back compat, in the future we might enforce it
            if (versionAttributes.Length == 1)
            {
                return (Version)versionAttributes[0];
            }

            return Version.DefaultVersion;
        }

        /// <summary>
        /// Ensures that the plugin has been instantiated properly
        /// With a workflow ExecutionContext.
        /// TODO: Replace this with a Compile Time Check using FxCop
        /// </summary>
        private void EnsurePluginIsInitialized()
        {
            if (!_isInitialized)
            {
                throw new WorkflowEngineException("Plugin not initialized properly. Make sure to use PluginServices to create all Plugins");
            }
        }

        /// <summary>
        /// Ensures that all non-null plugin parameters have been instantiated properly
        /// TODO: Replace this with a Compile Time Check using FxCop
        /// </summary>
        private void EnsureParamsContextIsSet(PluginInputs actualParams)
        {
            if (actualParams == null)
                throw new WorkflowEngineException(nameof(actualParams));

            if (actualParams.Values.Any(param => param != null && param.ExecutionContext == null))
            {
                throw new WorkflowEngineException("Workflow ExecutionContext not set for this plugin data objects. Make sure to use PluginServices to GetFromId all Plugin String");
            }
        }

        private bool IsCompatiblePlugin()
        {
            var currentVersion = ExecInfo.Version;
            var contextVersion = PluginContext.Version;

            var compatible = ExecInfo.PluginName == PluginContext.PluginName && currentVersion.Compatible(contextVersion);
            if (!compatible)
            {
                Log.Warn($"Plugin {PluginContext.PluginName} has an incompatible version");
            }

            return compatible;
        }

        /// <summary>
        /// Executes Plugin's ExecutePlugin method safely taking care maintaining the right execution context
        /// </summary>
        /// <param name="actualParams">Key-Value pairs representing Typed variable and their names</param>
        /// <returns>PluginOutput object</returns>
        private async Task<PluginData<T>> ExecuteSafely<T>(PluginInputs actualParams)
            where T : Schema, new()
        {
            var pluginSucceeded = false;
            PluginData<T> pluginData = null;
            try
            {
                EnsurePluginIsInitialized();
                _executionContext.StartPluginExecution(PluginContext);
                if (AlreadyExecuted())
                {
                    pluginSucceeded = true;
                    pluginData = _executionContext.GetPluginOutputFromId<T>(PluginContext.PluginOutputId);
                }
                else if (RequiredInputsNotNull(actualParams, ExecInfo.ParameterInfo))
                {
                    if (!IsCompatiblePlugin())
                        throw new VersionMismatchException(PluginContext.PluginName);

                    _executionContext.UpdateInputConsumers(PluginContext);
                    if (AllDependenciesFulFilled())
                    {
                        if (ValidateParams(actualParams, ExecInfo.ParameterInfo, out object[] validatedParams))
                        {
                            var execution = ExecInfo.Executable.Execute(validatedParams, PluginContext.PluginState, _executionContext);
                            var output = await ValidateReturn<T>(execution.GetResult()).ConfigureAwait(false);
                            pluginSucceeded = output.Success && output.Data != null;
                            pluginData = output.Data;
                            PluginContext.PluginState = execution.Store();
                        }
                    }
                }
            }
            catch (VersionMismatchException)
            {
                // Rethrow VME's to allow outer layers to deal with it appropriately
                throw;
            }
            catch (Exception ex)
            {
                throw new WorkflowEngineException("An unexpected exception was thrown during executing a plugin, see inner exception for details", ex);
            }
            finally
            {
                _executionContext.EndPluginExecution(pluginSucceeded, pluginData);
            }
            return pluginData;
        }

        private bool AllDependenciesFulFilled()
        {
             return _executionContext.CurrentPluginDependenciesFulfilled();
        }

        /// <summary>
        /// Returns true only if the plugin hasn't been executed already
        /// </summary>
        private bool AlreadyExecuted()
        {
            return _executionContext.PluginExecuted(PluginContext);
        }

        /// <summary>
        /// Validates that the Type returned by the Plugin's Execute method is the Type expected by the Workflow Developer
        /// TODO: Replace this with a Compile Time Check using FxCop
        /// </summary>
        /// <typeparam name="T">Expected Return Type</typeparam>
        /// <param name="ret">Actual returned object</param>
        /// <returns>Typed version of the returned object if there was a type match</returns>
        private async Task<PluginOutput<T>> ValidateReturn<T>(object ret)
            where T : Schema, new()
        {
            if (ret == null)
            {
                throw new WorkflowEngineException("ExecutePlugin returned null");
            }

            var expecteType = typeof(T);
            var actualType = ExecInfo.ReturnType;

            if (!expecteType.IsAssignableFrom(actualType))
            {
                throw new WorkflowEngineException($"Plugin {PluginContext.PluginName} called with an  invalid expected type");
            }

            var typedRet = ret as Task<PluginOutput<T>>;
            if (typedRet == null)
            {
                throw new WorkflowEngineException("ExecutePlugin didn't return Plugins of the expected Type");
            }

            return await typedRet.ConfigureAwait(false);
        }

        /// <summary>
        /// Validates that the passed in parameters are of the right number, types and names at runtime
        /// TODO: Replace this with a Compile Time Check using FxCop
        /// </summary>
        /// <param name="actualParams">Passed in parameters</param>
        /// <param name="expectedParams">Expected parameters based on reflection</param>
        /// <param name="validatedParams">Paramters that have already been validated</param>
        private bool ValidateParams(PluginInputs actualParams, ParameterInfo[] expectedParams, out object[] validatedParams)
        {
            const int numberOfInjectedParams = 1;
            const int minNumberOfParams = 1;

            validatedParams = new object[expectedParams.Length];

            if (expectedParams.Length < numberOfInjectedParams + minNumberOfParams)
            {
                throw new WorkflowEngineException("There should be at least 2 parameters for the ExecutePlugin method. A PluginServices object and at least one Plugins object");
            }

            if (actualParams == null || actualParams.Count != expectedParams.Length - numberOfInjectedParams)
            {
                Log.Warn($"Number of passed parameters to plugin {PluginContext.PluginName} doesn't match what the ExecutePlugin method expects ");
                return false;
            }

            EnsureParamsContextIsSet(actualParams);

            var pluginServicesParamExpected = false;

            for (var i = 0; i < expectedParams.Length; i++)
            {
                var expectedParam = expectedParams[i];

                if (expectedParam.Name == "pluginServices" && expectedParam.ParameterType == typeof(IPluginServices))
                {
                    pluginServicesParamExpected = true;
                    validatedParams[i] = _executionContext.PluginServices;
                    continue;
                }

                if (!actualParams.ContainsKey(expectedParam.Name))
                {
                    Log.Warn($"Expected Param {expectedParam.Name} was not passed to the ExecutePlugin Method");
                    return false;
                }

                var actualParam = actualParams[expectedParam.Name];

                if (actualParam != null && expectedParam.ParameterType != actualParam.GetType())
                {
                    Log.Warn($"Passed Param {expectedParam.Name} has the wrong type to plugin {PluginContext.PluginName}");
                    return false;
                }

                if (actualParam != null && !actualParam.Version.Compatible(GetExpectedParamVersion(expectedParam)))
                {
                    Log.Warn($"Passed Param {expectedParam.Name} has the wrong version to plugin {PluginContext.PluginName}");
                    return false;
                }

                validatedParams[i] = actualParam;
            }

            if (!pluginServicesParamExpected)
            {
                throw new WorkflowEngineException("ExecutePlugin Method should accept a pluginServices parameter of Type PluginServices");
            }

            return true;
        }
    }
}
