// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Schemas
{
    public class JsonExternalWorkflowInput<T> : Schema
        where T : Schema
    {
        public T RequestObject { get; set; }

        public ExternalTaskType ExternalTaskType { get; set; }
    }
}
