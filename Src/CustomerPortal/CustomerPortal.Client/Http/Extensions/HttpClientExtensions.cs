using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CustomerPortal.Client.Exceptions;
using Newtonsoft.Json;

namespace CustomerPortal.Client.Http.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> Get(this HttpClient client, string relativeUrl)
        {
            return await client.Send(HttpMethod.Get, new Uri(relativeUrl, UriKind.Relative))
                .ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> Post<TRequest>(this HttpClient client, string relativeUrl, TRequest content)
        {
            return await client.Send(HttpMethod.Post, new Uri(relativeUrl, UriKind.Relative), JsonConvert.SerializeObject(content))
                .ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> Put<TRequest>(this HttpClient client, string relativeUrl, TRequest content)
        {
            return await client.Send(HttpMethod.Put, new Uri(relativeUrl, UriKind.Relative), JsonConvert.SerializeObject(content))
                .ConfigureAwait(false);
        }

        public static async Task<HttpResponseMessage> Delete(this HttpClient client, string relativeUrl)
        {
            return await client.Send(HttpMethod.Delete, new Uri(relativeUrl, UriKind.Relative))
                .ConfigureAwait(false);
        }

        private static async Task<HttpResponseMessage> Send(this HttpClient client, HttpMethod method, Uri requestUri, string jsonPayload = null)
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (jsonPayload != null)
            {
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new ServiceResponseException(response, responseBody);
            }

            return response;
        }
    }
}
