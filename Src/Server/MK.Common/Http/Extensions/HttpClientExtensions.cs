using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Http.Exceptions;
using Newtonsoft.Json;

namespace apcurium.MK.Common.Http.Extensions
{
    public static class HttpClientExtensions
    {
#if !CLIENT
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(HttpClientExtensions));
#endif
        public static Task<HttpResponseMessage> Get(this HttpClient client, string relativeUrl)
        {
            return client.Send(HttpMethod.Get, new Uri(relativeUrl, UriKind.Relative));
        }

        public static Task<HttpResponseMessage> Post<TRequest>(this HttpClient client, string relativeUrl, TRequest content)
        {
            return client.Send(HttpMethod.Post, new Uri(relativeUrl, UriKind.Relative), JsonConvert.SerializeObject(content));
        }

        public static Task<HttpResponseMessage> Put<TRequest>(this HttpClient client, string relativeUrl, TRequest content)
        {
            return client.Send(HttpMethod.Put, new Uri(relativeUrl, UriKind.Relative), JsonConvert.SerializeObject(content));
        }

        public static Task<HttpResponseMessage> Delete(this HttpClient client, string relativeUrl)
        {
            return client.Send(HttpMethod.Delete, new Uri(relativeUrl, UriKind.Relative));
        }

        private static async Task<HttpResponseMessage> Send(this HttpClient client, HttpMethod method, Uri requestUri, string jsonPayload = null)
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (jsonPayload != null)
            {
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }
#if !CLIENT
            Log.Debug("Request : " + request);
            Log.Debug("Request Content : " + jsonPayload);
#endif

            var response = await client.SendAsync(request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
#if !CLIENT
                Log.Debug("Response : " + response.StatusCode);
                Log.Debug("ResponseBody : " + responseBody);
#endif
                throw new ServiceResponseException(response, responseBody);
            }

            return response;
        }
    }
}
