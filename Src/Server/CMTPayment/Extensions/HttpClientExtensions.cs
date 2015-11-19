using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using Android.Hardware;

namespace CMTPayment.Extensions
{
    public static class HttpClientExtensions
    {
        public static void SetOAuthHeader(this HttpClient client, string url, string method, string consumerKey, string consumerSecretKey)
        {
            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(consumerKey,
                consumerSecretKey,
                "",
                "",
                method,
                new Uri(url),
                null);


            var authHeader = new AuthenticationHeaderValue(oauthHeader);

            client.DefaultRequestHeaders.Authorization = authHeader;
        }

        public static Task<TResult> GetAsync<TResult>(this HttpClient client,
            string url,
            Action<HttpResponseMessage> onSuccess, 
            Action<HttpResponseMessage> onError,
            Action<HttpResponseMessage> onComplete)
        {
            return Task.Run(() => client.GetAsync(url).HandleResult<TResult>(onSuccess, onError, onComplete));
        }



        private static async Task<TResult> HandleResult<TResult>(this Task<HttpResponseMessage> response, 
            Action<HttpResponseMessage> onSuccess,
            Action<HttpResponseMessage> onError,
            Action<HttpResponseMessage> onComplete)
        {
            HttpResponseMessage result = null;
            try
            {
                result = await response;

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
            finally
            {
                if (onComplete != null && result != null)
                {
                    onComplete(result);
                }
            }
            
        }
    }
}