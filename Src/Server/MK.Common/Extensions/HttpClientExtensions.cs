using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.Extensions
{
	public static class HttpClientExtensions
    {
        public static Task<TResult> GetAsync<TResult>(this HttpClient client, string absoluteOrRelativeUrl, params object[] parameters)
        {
            return Task.Run(async () =>
            {
                var url = parameters != null
                    ? absoluteOrRelativeUrl.InvariantCultureFormat(parameters)
                    : absoluteOrRelativeUrl;

                var httpResponseMessage = await client.GetAsync(url);

                httpResponseMessage.EnsureSuccessStatusCode();

                var result = await httpResponseMessage.Content.ReadAsStringAsync();

                return result.FromJson<TResult>();
            });
        }

        public static Task<TResult> PostAsync<TResult>(this HttpClient client, string absoluteOrRelativeUrl, object content, params object[] parameters)
        {
            return Task.Run(async () =>
            {
                var url = parameters != null
                    ? absoluteOrRelativeUrl.InvariantCultureFormat(parameters)
                    : absoluteOrRelativeUrl;

                var httpContent = new StringContent(content.ToJson(), Encoding.UTF8, "application/json");

                var httpResponseMessage = await client.PostAsync(url, httpContent);

                httpResponseMessage.EnsureSuccessStatusCode();

                var result = await httpResponseMessage.Content.ReadAsStringAsync();

                return result.FromJson<TResult>();
            });
        }

        public static Task<TResult> PutAsync<TResult>(this HttpClient client, string absoluteOrRelativeUrl, object content, params object[] parameters)
        {
            return Task.Run(async () =>
            {
                var url = parameters != null
                    ? absoluteOrRelativeUrl.InvariantCultureFormat(parameters)
                    : absoluteOrRelativeUrl;

                var httpContent = new StringContent(content.ToJson(), Encoding.UTF8, "application/json");

                var httpResponseMessage = await client.PutAsync(url, httpContent);

                httpResponseMessage.EnsureSuccessStatusCode();

                var result = await httpResponseMessage.Content.ReadAsStringAsync();

                return result.FromJson<TResult>();
            });
        }

        public static Task<TResult> DeleteAsync<TResult>(this HttpClient client, string absoluteOrRelativeUrl, params object[] parameters)
        {
            return Task.Run(async () =>
            {
                var url = parameters != null
                    ? absoluteOrRelativeUrl.InvariantCultureFormat(parameters)
                    : absoluteOrRelativeUrl;

                var httpResponseMessage = await client.DeleteAsync(url);

                httpResponseMessage.EnsureSuccessStatusCode();

                var result = await httpResponseMessage.Content.ReadAsStringAsync();

                return result.FromJson<TResult>();
            });
        }
    }
}

