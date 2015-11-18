using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.MapDataProvider.Extensions
{
    public static class ServiceClientBaseExtensions
    {
        public static Task<TResponse> GetAsync<TResponse>(this HttpClient client, string relativeOrAbsoluteUrl)
        {
            return Task.Run(async () =>
            {
                var httpMessageResponse = await client.GetAsync(relativeOrAbsoluteUrl)
                    .ConfigureAwait(false);

                var result = await httpMessageResponse.EnsureSuccessStatusCode().Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                return result.FromJson<TResponse>();
            });
        }

        public static Task<TResponse> PostAsync<TResponse>(this HttpClient client, string relativeOrAbsoluteUrl, object request)
        {
            return Task.Run(async () =>
            {
                var httpContent = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");

                var httpMessageResponse = await client.PostAsync(relativeOrAbsoluteUrl, httpContent)
                    .ConfigureAwait(false);

                var result = await httpMessageResponse.EnsureSuccessStatusCode().Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                return result.FromJson<TResponse>();
            });
        }

        public static Task<TResponse> PutAsync<TResponse>(this HttpClient client, string relativeOrAbsoluteUrl, object request)
        {
            return Task.Run(async () =>
            {
                var httpContent = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");

                var httpMessageResponse = await client.PutAsync(relativeOrAbsoluteUrl, httpContent)
                    .ConfigureAwait(false);

                var result = await httpMessageResponse.EnsureSuccessStatusCode().Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                return result.FromJson<TResponse>();
            });
        }

        public static Task<TResponse> DeleteAsync<TResponse>(this HttpClient client, string relativeOrAbsoluteUrl)
        {
            return Task.Run(async () =>
            {
                var httpMessageResponse = await client.DeleteAsync(relativeOrAbsoluteUrl)
                    .ConfigureAwait(false);

                var result = await httpMessageResponse.EnsureSuccessStatusCode().Content
                    .ReadAsStringAsync()
                    .ConfigureAwait(false);

                return result.FromJson<TResponse>();
            });
        }
    }
}