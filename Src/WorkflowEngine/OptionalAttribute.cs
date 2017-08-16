// Copyright (c) Microsoft Corporation. All rights reserved.

using System;

namespace WorkflowEngine
{
    /// <summary>
    /// This attributes indicates that a plugin parameter is optional
    /// and that the plugin can safely execute with a null value for this parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class OptionalAttribute : Attribute
    {
    }
}
