using System;
using System.Threading.Tasks;
using ServiceStack.Common.Web;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace CMTPayment.Extensions
{
	public static class ServiceClientBaseExtensions
    {        

        public static Task<TResponse> GetAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.GetAsync<TResponse>(relativeOrAbsoluteUrl,
                tcs.SetResult,
                (result, error) => tcs.SetException(FixWebServiceException(error)));

            return tcs.Task;
        }

        public static Task<TResponse> GetAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.GetAsync(request,
                tcs.SetResult,
                (result, error) => tcs.SetException(FixWebServiceException(error)));

            return tcs.Task;
        }

#if !CLIENT
		private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ServiceClientBaseExtensions));
		public static Task<TResponse> PostAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
		{
			var tcs = new TaskCompletionSource<TResponse>();

            client.PostAsync<string>(request.ToUrl(HttpMethods.Post, client.Format), 
                request, 
                result =>
                {
                    //TResponse dto = result.To<TResponse>();
                    var dto = Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(result);
                    tcs.SetResult(dto);

                }, 
                (response, exception) =>
                {
                    Log.Debug("CMT Response Body : " + response);
                    tcs.SetException(FixWebServiceException(exception));
                }
            );

			return tcs.Task;
		}
#else

		public static Task<TResponse> PostAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
		{
			var tcs = new TaskCompletionSource<TResponse>();

			client.PostAsync<TResponse>(request,
				tcs.SetResult,
                (result, error) => tcs.SetException(FixWebServiceException(error)));

			return tcs.Task;
		}
#endif

		public static Task<TResponse> PostAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl, object request)
		{
			var tcs = new TaskCompletionSource<TResponse>();

			client.PostAsync<TResponse>(relativeOrAbsoluteUrl,
				request,
				tcs.SetResult,
				(result, error) => tcs.SetException(FixWebServiceException(error)));

			return tcs.Task;
		}

        public static Task<TResponse> PutAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl, object request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.PutAsync<TResponse>(relativeOrAbsoluteUrl,
                request,
                tcs.SetResult,
                (result, error) => tcs.SetException(FixWebServiceException(error)));

            return tcs.Task;
        }

        public static Task<TResponse> DeleteAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.DeleteAsync<TResponse>(relativeOrAbsoluteUrl,
                tcs.SetResult,
                (result, error) => tcs.SetException(FixWebServiceException(error)));

            return tcs.Task;
        }

        public static Task<TResponse> DeleteAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.DeleteAsync(request,
                tcs.SetResult,
                (result, error) => tcs.SetException(FixWebServiceException(error)));

            return tcs.Task;
        }

        public static Exception FixWebServiceException(Exception e)
        {
            var wse = e as WebServiceException;
            if (wse != null && wse.StatusDescription == null)
            {
                // Fix ServiceStack bug.
                // WebServiceException.StatusDescription is null when using AsyncServiceClient
                wse.StatusDescription = e.Message;
            }
            return e;
        }
    }
}

