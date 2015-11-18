using apcurium.MK.Common.Serializer;
using Newtonsoft.Json;

namespace apcurium.MK.Common.Extensions
{
    public static class JsonSerializerExtensions
    {
        private static NewtonsoftJsonSerializer GetJsonConverter()
        {
            var serializer = new JsonSerializer
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            return new NewtonsoftJsonSerializer(serializer);
        }

        public static string ToJson(this object source)
        {
            return source == null 
                ? string.Empty 
                : GetJsonConverter().SerializeObject(source);
        }

        public static TResult FromJson<TResult>(this string source)
        {
            return source.HasValueTrimmed() 
                ? GetJsonConverter().DeserializeObject<TResult>(source) 
                : default(TResult);
        }
    }
}