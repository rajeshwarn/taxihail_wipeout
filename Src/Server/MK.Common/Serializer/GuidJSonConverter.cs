using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MK.Common.Android.Serializer
{
    public class GuidJsonConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Guid) || objectType == typeof(Guid?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token == null || token.Type == JTokenType.Null || (token.Type != JTokenType.Guid && token.Type != JTokenType.String))
            {
                return objectType == typeof(Guid?)
                    ? (Guid?)null 
                    : Guid.Empty;
            }

            if (token.Type == JTokenType.Guid)
            {
                return token.ToObject<Guid>();
            }

            var value = token.ToObject<string>();
            
            return new Guid(value);
        }
    }
}