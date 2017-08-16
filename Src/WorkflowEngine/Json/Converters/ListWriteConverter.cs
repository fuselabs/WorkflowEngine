// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WorkflowEngine.Json.Converters
{
    public class ListWriteConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (CanConvert(value.GetType()))
            {
                var list = value as IEnumerable;
                if (list == null)
                    return;

                writer.WriteStartArray();
                foreach (var elem in list)
                {
                    serializer.Serialize(writer, elem);
                }
                writer.WriteEndArray();
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
            return ImplementsIEnumerable(objectType) || IsArray(objectType);
        }

        internal static bool ImplementsIEnumerable(Type objectType)
        {
            return objectType.IsGenericType && objectType.GetGenericTypeDefinition().GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        internal static bool IsArray(Type objectType)
        {
            return objectType.IsArray;
        }
    }
}
