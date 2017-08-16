// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Newtonsoft.Json;

namespace Schemas
{
    /// <summary>
    /// Base class for all schemas
    /// </summary>
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]

    [Serializable]
    public class Schema
    {
    }
}
