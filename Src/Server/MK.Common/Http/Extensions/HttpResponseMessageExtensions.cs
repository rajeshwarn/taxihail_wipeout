using System.Net.Http;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using Newtonsoft.Json;

namespace apcurium.MK.Common.Http.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T> Deserialize<T>(this Task<HttpResponseMessage> task, JsonSerializerSettings serializerSettings = null)
        {
            var response = await task.ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false);

            return json.FromJsonSafe<T>();
        }
    }
}
