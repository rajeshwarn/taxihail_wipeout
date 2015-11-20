using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Common.Extensions
{
	public static class HttpClientExtensions
    {
        public static Task<T> DeleteAsync<T>(this HttpClient client, IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();
            return client.DeleteAsync<T>(url);
        }

        public static Task<T> PostAsync<T>(this HttpClient client, IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            return client.PostAsync<T>(url, request);
        }

        public static Task<T> GetAsync<T>(this HttpClient client, IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            return client.GetAsync<T>(url);
        }

        public static Task<TResult> GetAsync<TResult>(this HttpClient client,
            string url,
            Action<HttpResponseMessage> onSuccess = null,
            Action<HttpResponseMessage> onError = null)
        {
            return Task.Run(() => client.GetAsync(url).HandleResult<TResult>(onSuccess, onError));
        }

        public static Task<T> DeleteAsync<T>(this HttpClient client,
            string url,
            Action<HttpResponseMessage> onSuccess = null,
            Action<HttpResponseMessage> onError = null)
        {
            return Task.Run(() => client.DeleteAsync(url).HandleResult<T>(onSuccess, onError));
        }

        public static Task<TResult> PostAsync<TResult>(this HttpClient client,
            string url,
            object content,
            Action<HttpResponseMessage> onSuccess = null,
            Action<HttpResponseMessage> onError = null)
        {
            return Task.Run(async () =>
            {
                var body = new StringContent(content.ToJson(), Encoding.UTF8, "application/json");

                return await client.PostAsync(url, body).HandleResult<TResult>(onSuccess, onError);
            });
        }

        public static Task<TResult> PutAsync<TResult>(this HttpClient client,
            string url,
            object content,
            Action<HttpResponseMessage> onSuccess = null,
            Action<HttpResponseMessage> onError = null)
        {
            return Task.Run(async () =>
            {
                var body = new StringContent(content.ToJson(), Encoding.UTF8, "application/json");

                return await client.PutAsync(url, body).HandleResult<TResult>(onSuccess, onError);
            });
        }

        public static async Task HandleResult(this Task<HttpResponseMessage> response,
            Action<HttpResponseMessage> onSuccess,
            Action<HttpResponseMessage> onError)
        {
            var result = await response;

            if (!result.IsSuccessStatusCode && onError != null)
            {
                onError(result);

                result.EnsureSuccessStatusCode();
            }

            if (onSuccess != null)
            {
                onSuccess(result);
            }
        }

        private static async Task<TResult> HandleResult<TResult>(this Task<HttpResponseMessage> response,
            Action<HttpResponseMessage> onSuccess,
            Action<HttpResponseMessage> onError)
        {
            var result = await response;

            if (!result.IsSuccessStatusCode && onError != null)
            {
                onError(result);

                result.EnsureSuccessStatusCode();
            }

            if (onSuccess != null)
            {
                onSuccess(result);
            }

            var jsonContent = await result.Content.ReadAsStringAsync();

            return jsonContent.FromJson<TResult>();

        }
    }
}

