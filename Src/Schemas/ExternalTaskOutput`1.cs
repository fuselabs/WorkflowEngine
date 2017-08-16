// Copyright (c) Microsoft Corporation. All rights reserved.

namespace Schemas
{
    public class ExternalTaskOutput<T> : Schema
        where T : Schema, new()
    {
        public T OutputData { get; set; }

        public bool Succeeded { get; set; }

        public string EscalationReason { get; set; }
    }
}
