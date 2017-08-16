// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net.Appender;
using log4net.Core;

namespace WorkflowEngine.Logging
{
    /// <summary>
    /// Trace Appender to log into Azure Table Storage with the right level
    /// </summary>
    public class AzureTraceAppender : TraceAppender
    {
        private static readonly int FatalLevel = Level.Fatal.Value;
        private static readonly int ErrorLevel = Level.Error.Value;
        private static readonly int WarnLevel = Level.Warn.Value;
        private static readonly int InfoLevel = Level.Info.Value;

        private static readonly IDictionary<int, Action<string>> LevelMapping = new Dictionary<int, Action<string>>
        {
            { FatalLevel, s => Trace.Fail(s) },              // Level 1
            { ErrorLevel, s => Trace.TraceError(s) },        // Level 2
            { WarnLevel, s => Trace.TraceWarning(s) },       // Level 3
            { InfoLevel, s => Trace.TraceInformation(s) },   // Level 4
        };

        protected override void Append(LoggingEvent loggingEvent)
        {
            var level = loggingEvent.Level.Value;
            var message = RenderLoggingEvent(loggingEvent);

            if (LevelMapping.ContainsKey(level))
            {
                var traceAction = LevelMapping[level];
                traceAction(message);
            }
            else
            {
                Trace.Write(message);          // Level 5
            }

            if (ImmediateFlush)
                Trace.Flush();
        }
    }
}
