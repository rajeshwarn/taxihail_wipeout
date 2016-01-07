using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MK.Common.Exceptions;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

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
            return Task.Run(() => client.GetAsync(client.GetForEndpointIfNeeded(url)).HandleResult<TResult>(onSuccess, onError));
        }


	    private static string GetForEndpointIfNeeded(this HttpClient client, string url)
	    {
	        if (url.StartsWith("http") || client.BaseAddress == null)
	        {
	            return url;
	        }

            var currentRelativeUrl = client.BaseAddress.LocalPath;
           
	        return url.StartsWith("/") || currentRelativeUrl.EndsWith("/")
                ? currentRelativeUrl + url
                : "{0}/{1}".InvariantCultureFormat(currentRelativeUrl, url);
	    }

        public static Task<T> DeleteAsync<T>(this HttpClient client,
            string url,
            Action<HttpResponseMessage> onSuccess = null,
            Action<HttpResponseMessage> onError = null)
        {
            return Task.Run(() => client.DeleteAsync(client.GetForEndpointIfNeeded(url)).HandleResult<T>(onSuccess, onError));
        }

        public static Task<TResult> PostAsync<TResult>(this HttpClient client,
            string url,
            object content,
            Action<HttpResponseMessage> onSuccess = null,
            Action<HttpResponseMessage> onError = null)
        {
            return Task.Run(() => InnerPostAsync<TResult>(client, url, content, onSuccess, onError));
        }

	    private static async Task<TResult> InnerPostAsync<TResult>(HttpClient client, string url, object content, Action<HttpResponseMessage> onSuccess, Action<HttpResponseMessage> onError)
	    {
	        var body = new StringContent(content.ToJson(), Encoding.UTF8, "application/json");

	        var relativeUrl = client.GetForEndpointIfNeeded(url);

	        return await client.PostAsync(relativeUrl, body).HandleResult<TResult>(onSuccess, onError);
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

                return await client.PutAsync(client.GetForEndpointIfNeeded(url), body).HandleResult<TResult>(onSuccess, onError);
            });
        }

        public static async Task HandleResult(this Task<HttpResponseMessage> response, Action<HttpResponseMessage> onSuccess, Action<HttpResponseMessage> onError)
        {
            var result = await response;

            await result.HandleResultInternal(onSuccess, onError);
        }

        private static async Task<TResult> HandleResult<TResult>(this Task<HttpResponseMessage> response, Action<HttpResponseMessage> onSuccess, Action<HttpResponseMessage> onError)
        {
            var result = await response;

            await result.HandleResultInternal(onSuccess, onError);

            var jsonContent = await result.Content.ReadAsStringAsync();
            return jsonContent.FromJson<TResult>();
        }

        private static async Task HandleResultInternal(this HttpResponseMessage result, Action<HttpResponseMessage> onSuccess, Action<HttpResponseMessage> onError)
        {
            if (!result.IsSuccessStatusCode)
            {
                if (onError != null)
                {
                    onError(result);
                }

                var body = await result.Content.ReadAsStringAsync();
                var errorResponse = body.FromJson<ErrorResponse>();

                if (errorResponse != null)
                {
                    throw new WebServiceException(errorResponse.ResponseStatus.ErrorCode) 
                    {
                        StatusCode = (int)result.StatusCode,
                        StatusDescription = result.ReasonPhrase,
                        ResponseBody = body
                    };
                }
                else
                {
                    throw new WebServiceException(result.ReasonPhrase) 
                    {
                        StatusCode = (int)result.StatusCode,
                        StatusDescription = result.ReasonPhrase,
                        ResponseBody = body
                    };
                }
            }

            if (onSuccess != null)
            {
                onSuccess(result);
            }
        }
    }
}

