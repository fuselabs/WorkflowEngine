// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WorkflowEngine.Json.Converters
{
    public class DictionaryReadConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            // If the Type isn't of type IEnumerable, we just want the default serializer to handle it
            if (!IsIDictionary(objectType))
                return serializer.Deserialize(reader, objectType);

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException($"IDictionary Type: {objectType} must be serialized as a JSON object");

            var dictGenericType = typeof(Dictionary<,>);
            var dictType = dictGenericType.MakeGenericType(objectType.GetGenericArguments()[0], objectType.GetGenericArguments()[1]);
            return serializer.Deserialize(reader, dictType);
        }

        public override bool CanConvert(Type objectType)
        {
            return IsIDictionary(objectType);
        }

        internal static bool IsIDictionary(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }
    }
}
