// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WorkflowEngine.Json.Converters
{
    /// <summary>
    /// A converted to register a default implementation for interface types
    /// to allow disambugation at the time of deserialization
    /// </summary>
    public class DefaultInterfaceConverter<TInterface, TConcrete> : JsonConverter
        where TConcrete : TInterface
    {
        private const string TypePropertyName = "$type";

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var typeName = jObject[TypePropertyName]?.ToObject<string>();
            return !string.IsNullOrEmpty(typeName) ? jObject.ToObject(Type.GetType(typeName)) : jObject.ToObject<TConcrete>();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TInterface);
        }
    }
}
