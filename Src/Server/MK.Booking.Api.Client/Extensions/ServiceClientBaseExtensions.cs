using System;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;

namespace ServiceStack.ServiceClient.Web
{
	public static class ServiceClientBaseExtensions
    {
		public static Task<TResponse> PostAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
		{
			var tcs = new TaskCompletionSource<TResponse>();

			client.PostAsync<TResponse>(request,
				tcs.SetResult,
				(result, error) => tcs.SetException(error));

			return tcs.Task;
		}

		public static Task<TResponse> PostAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl, object request)
		{
			var tcs = new TaskCompletionSource<TResponse>();

			client.PostAsync<TResponse>(relativeOrAbsoluteUrl,
				request,
				tcs.SetResult,
				(result, error) => tcs.SetException(error));

			return tcs.Task;
		}
    }
}

