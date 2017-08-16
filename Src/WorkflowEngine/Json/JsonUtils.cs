// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;
using WorkflowEngine.Json.Converters;

namespace WorkflowEngine.Json
{
    public static class JsonUtils
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters = new List<JsonConverter>
            {
                // The order matters, it's important to register the dictionary converters first
                // Because in C#, a dictionary IS a list, so if we reverse the order, dictionaries
                // Will get serialized as lists
                new DictionaryReadConverter(),
                new DictionaryWriteConverter(),
                new ListReadConverter(),
                new ListWriteConverter()
            },
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        private static readonly ILog Log = LogManager.GetLogger(typeof(JsonUtils));

        public static bool TryDeserialize<T>(string serialized, out T deserialized)
          where T : class
        {
            try
            {
                deserialized = JsonConvert.DeserializeObject<T>(serialized, SerializerSettings);
                return true;
            }
            catch (Exception ex)
            {
                deserialized = null;
                Log.Error("Failed to deserialize object", ex);
                return false;
            }
        }

        public static void SetGlobalJsonNetSettings()
        {
            JsonConvert.DefaultSettings = () => SerializerSettings;
        }
    }
}
