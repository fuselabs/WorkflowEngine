// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Reflection;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    internal partial class RuntimeCache
    {
        internal class CachedMethodInfo
        {
            public string PluginName { get; set; }

            public ParameterInfo[] ParameterInfo { get; set; }

            public IExecutable Executable { get; set; }

            public Type ReturnType { get; set; }

            public Version Version { get; set; }
        }
    }
}
