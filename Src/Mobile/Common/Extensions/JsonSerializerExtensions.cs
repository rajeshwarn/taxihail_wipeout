using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore.Platform;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class JsonSerializerExtensions
    {
        private static IMvxJsonConverter GetConverter()
        {
            return TinyIoCContainer.Current.Resolve<IMvxJsonConverter>("Newtonsoft");
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