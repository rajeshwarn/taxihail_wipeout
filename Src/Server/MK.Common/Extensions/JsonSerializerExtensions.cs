using apcurium.MK.Common.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace apcurium.MK.Common.Extensions
{
    public static class JsonSerializerExtensions
    {
        private static NewtonsoftJsonSerializer GetJsonConverter(bool transformToCamelCase)
        {
            var serializer = new JsonSerializer
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ContractResolver = transformToCamelCase ? new CustomCamelCasePropertyNamesContractResolver() : new DefaultContractResolver()
            };

            return new NewtonsoftJsonSerializer(serializer);
        }

        public static string ToJson(this object source, bool transformToCamelCase = true)
        {
            return source == null 
                ? string.Empty 
                : GetJsonConverter(transformToCamelCase).SerializeObject(source);
        }

        public static TResult FromJson<TResult>(this string source, bool transformToCamelCase = true)
        {
            return source.HasValueTrimmed() 
                ? GetJsonConverter(transformToCamelCase).DeserializeObject<TResult>(source) 
                : default(TResult);
        }

        public static TResult FromJsonSafe<TResult>(this string source)
        {
            try
            {
                return FromJson<TResult>(source);
            }
            catch(Exception ex)
            {
                return default(TResult);
            }
        }
    }

    public class CustomCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}