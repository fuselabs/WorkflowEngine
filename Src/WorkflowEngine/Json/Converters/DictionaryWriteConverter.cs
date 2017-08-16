// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WorkflowEngine.Json.Converters
{
    public class DictionaryWriteConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (CanConvert(value.GetType()))
            {
                var dict = value as IDictionary;
                if (dict == null)
                    return;
                writer.WriteStartObject();
                foreach (var elem in dict)
                {
                    var kv = (DictionaryEntry)elem;
                    writer.WritePropertyName(kv.Key.ToString());
                    serializer.Serialize(writer, kv.Value);
                }
                writer.WriteEndObject();
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return ImplementsIDictionary(objectType);
        }

        internal static bool ImplementsIDictionary(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition().GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }
    }
}
