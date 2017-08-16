// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WorkflowEngine.Exceptions;
using WorkflowEngine.Interfaces;

namespace WorkflowEngine
{
    internal class StatelessExecutable : IExecutable
    {
        /// <summary>
        /// In the case of stateless (static) executables, we cache the delegate as an optimization
        /// </summary>
        private readonly Delegate _exec;

        public StatelessExecutable(MethodInfo mi)
        {
            _exec = GetExecDelegate(mi);
        }

        public Execution Execute(object[] parameters, IState state, ExecutionContext executionContext)
        {
            if (state != null)
                throw new WorkflowEngineException("Stateless Executable is not allowed to have state");
            return new Execution(_exec.DynamicInvoke(parameters), null);    // Stateless Executable has no state
        }

        private static Delegate GetExecDelegate(MethodInfo methodInfo)
        {
            var delegateType = Expression.GetDelegateType((from parameter in methodInfo.GetParameters() select parameter.ParameterType)
                                            .Concat(new[] { methodInfo.ReturnType })
                                            .ToArray());

            return Delegate.CreateDelegate(delegateType, methodInfo);
        }
    }
}
