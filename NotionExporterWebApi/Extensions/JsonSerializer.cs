using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NotionExporterWebApi.Extensions
{
    public static class JsonSerializer
    {
        public static string ToPrettyJson<T>(this T obj)
        {
            return SerializeObject(obj);
        }

        public static string SerializeObject<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented,
                new StringEnumConverterWithNullForUnknownValues());
        }

        public static T? DeserializeObject<T>(string str) where T : class
        {
            return JsonConvert.DeserializeObject<T>(str,
                new StringEnumConverterWithNullForUnknownValues());
        }
    }


    internal class StringEnumConverterWithNullForUnknownValues : StringEnumConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
            Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                return base.ReadJson(reader, objectType, existingValue, serializer);
            }
            catch (JsonSerializationException)
            {
                return null;
            }
        }
    }
}