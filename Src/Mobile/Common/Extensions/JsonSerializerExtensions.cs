using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore.Platform;
using TinyIoC;

namespace apcurium.MK.Common.Extensions
{
    public static class JsonSerializerExtensions
    {
        private static JsonSerializer GetJsonConverter()
        {
            var serializer = new JsonSerializer
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                NullValueHandling = NullValueHandling.Include
            };

            return serializer;
        }

        public static string ToJson(this object source)
        {
            return source == null 
                ? string.Empty 
                : GetConverter().SerializeObject(source);
        }

        public static TResult FromJson<TResult>(this string source)
        {
            return source.HasValueTrimmed() 
                ? GetConverter().DeserializeObject<TResult>(source) 
                : default(TResult);
        }
    }
}