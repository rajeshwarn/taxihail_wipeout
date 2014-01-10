using System.Threading.Tasks;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Client.Extensions
{
	public static class ServiceClientBaseExtensions
    {
        public static Task<TResponse> GetAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.GetAsync<TResponse>(relativeOrAbsoluteUrl,
                tcs.SetResult,
                (result, error) => tcs.SetException(error));

            return tcs.Task;
        }

        public static Task<TResponse> GetAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.GetAsync(request,
                tcs.SetResult,
                (result, error) => tcs.SetException(error));

            return tcs.Task;
        }

		public static Task<TResponse> PostAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
		{
			var tcs = new TaskCompletionSource<TResponse>();

			client.PostAsync(request,
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

        public static Task<TResponse> PutAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl, object request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.PostAsync<TResponse>(relativeOrAbsoluteUrl,
                request,
                tcs.SetResult,
                (result, error) => tcs.SetException(error));

            return tcs.Task;
        }

        public static Task<TResponse> DeleteAsync<TResponse>(this ServiceClientBase client, string relativeOrAbsoluteUrl)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.GetAsync<TResponse>(relativeOrAbsoluteUrl,
                tcs.SetResult,
                (result, error) => tcs.SetException(error));

            return tcs.Task;
        }

        public static Task<TResponse> DeleteAsync<TResponse>(this ServiceClientBase client, IReturn<TResponse> request)
        {
            var tcs = new TaskCompletionSource<TResponse>();

            client.GetAsync(request,
                tcs.SetResult,
                (result, error) => tcs.SetException(error));

            return tcs.Task;
        }
    }
}

