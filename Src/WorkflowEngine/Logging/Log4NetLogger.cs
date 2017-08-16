// Copyright (c) Microsoft Corporation. All rights reserved.

using log4net;
using WorkflowEngine.Logging.Interfaces;

namespace WorkflowEngine.Logging
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        public Log4NetLogger(ILog log)
        {
            _log = log;
        }

        public void Error(string msg)
        {
            _log.Error(msg);
        }

        public void Warn(string msg)
        {
            _log.Warn(msg);
        }

        public void Fatal(string msg)
        {
            _log.Fatal(msg);
        }

        public void Debug(string msg)
        {
            _log.Debug(msg);
        }

        public void Info(string msg)
        {
            _log.Info(msg);
        }
    }
}
