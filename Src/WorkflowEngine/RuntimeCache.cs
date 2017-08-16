// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    /// <summary>
    /// This class is an optimization. Its purpose is to cache all runtime information that we get
    /// via reflection to reduce the perf impact of using reflection
    ///
    /// This class is thread safe
    /// </summary>
    internal partial class RuntimeCache
    {
        /// <summary>
        /// Cached All Reflection Information for improved performance
        /// </summary>
        private static readonly IDictionary<string, CachedMethodInfo> MethodInfoCache = new ConcurrentDictionary<string, CachedMethodInfo>();

        /// <summary>
        /// Uses Reflection to get MethodInfo for the Typed ExecutePlugin method. Also caches
        /// All reflection results to improve performance
        /// </summary>
        internal static CachedMethodInfo GetExecInfo(Type type)
        {
            var typeName = UniqueTypeName(type);
            if (MethodInfoCache.ContainsKey(typeName))
            {
                return MethodInfoCache[typeName];
            }

            var methodInfo = type.GetMethod("ExecutePlugin", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new WorkflowEngineException("Plugin Doesn't have an ExecutePlugin method Definition");
            }

            var cachedInfo = new CachedMethodInfo
            {
                ParameterInfo = GetParamterInfo(methodInfo),
                Executable = GetExecutable(methodInfo, type),
                PluginName = UniqueTypeName(type, false),
                ReturnType = GetReturnType(methodInfo),
                Version = GetVersion(methodInfo)
            };

            MethodInfoCache[typeName] = cachedInfo;
            return cachedInfo;
        }

        private static ParameterInfo[] GetParamterInfo(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters();
        }

        private static Type GetReturnType(MethodInfo methodInfo)
        {
            var returnType = methodInfo.ReturnType;

            if (!returnType.IsGenericType ||
                returnType.GetGenericTypeDefinition() != typeof(Task<>) ||
                returnType.GenericTypeArguments[0].GetGenericTypeDefinition() != typeof(PluginOutput<>))
            {
                throw new WorkflowEngineException("ExecutePlugin method must return Task<PluginOutput<T>>");
            }

            return returnType.GenericTypeArguments[0].GenericTypeArguments[0];
        }

        private static Version GetVersion(MethodInfo methodInfo)
        {
            if (methodInfo.CustomAttributes == null)
                return Version.DefaultVersion;

            var versionAttributes = methodInfo.GetCustomAttributes(typeof(Version), true);

            // For now we'll allow plugins to not have a version for back compat, in the future we might enforce it
            if (versionAttributes.Length == 1)
            {
                return (Version)versionAttributes[0];
            }

            return Version.DefaultVersion;
        }

        // Returns a unique type name including for same generics with different types
        private static string UniqueTypeName(Type type, bool fullName = true)
        {
            var sb = new StringBuilder();
            sb.Append(fullName ? type.FullName : type.Name);

            if (!type.IsGenericType)
                return sb.ToString();

            var genericParameters = type.GenericTypeArguments.Select(typeArg => typeArg.Name);
            var genericArgsNameExtention = string.Join("_", genericParameters);
            sb.Append("_");
            sb.Append(genericArgsNameExtention);
            return sb.ToString();
        }

        private static IExecutable GetExecutable(MethodInfo mi, Type type)
        {
            return mi.IsStatic ? (IExecutable)new StatelessExecutable(mi) : new StatefulExecutable(mi, type);
        }
    }
}
