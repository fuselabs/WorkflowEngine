// Copyright (c) Microsoft Corporation. All rights reserved.

namespace WorkflowEngine.Logging.Interfaces
{
    public interface ILogger
    {
        void Error(string msg);

        void Warn(string msg);

        void Fatal(string msg);

        void Debug(string msg);

        void Info(string msg);
    }
}
