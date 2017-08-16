// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WorkflowEngine.Json.Converters
{
    public class ListReadConverter : JsonConverter
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
            if (!CanConvert(objectType))
                return serializer.Deserialize(reader, objectType);

            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException($"IEnumerable Type: {objectType} must be serialized as a JSON arras");

            var listGenericType = typeof(List<>);
            var listType = listGenericType.MakeGenericType(objectType.GetGenericArguments().FirstOrDefault());
            return serializer.Deserialize(reader, listType);
        }

        public override bool CanConvert(Type objectType)
        {
            return IsIEnumerable(objectType);
        }

        private static bool IsIEnumerable(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}
