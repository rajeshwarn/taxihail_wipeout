using System;
using System.Reactive;
using System.Threading.Tasks;
using UIKit;
namespace apcurium.MK.Booking.Mobile.Client.Extensions
{
	public static class UIApplicationExtensions
	{
		public static async Task<T> InvokeOnMainThreadAsync<T>(this UIApplication sharedApplication, Func<Task<T>> asyncDelegateToExecute)
		{
			var tcs = new TaskCompletionSource<T>();


			sharedApplication.InvokeOnMainThread(async () =>
			{
				try
				{
					var result = await asyncDelegateToExecute();

					tcs.TrySetResult(result);

				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});

			return await tcs.Task.ConfigureAwait(false);
		}

		public static async Task<T> InvokeOnMainThreadAsync<T>(this UIApplication sharedApplication, Func<T> delegateToExecute)
		{
			var tcs = new TaskCompletionSource<T>();


			sharedApplication.InvokeOnMainThread(() =>
			{
				try
				{
					var result = delegateToExecute();

					tcs.TrySetResult(result);

				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});

			return await tcs.Task.ConfigureAwait(false);
		}

		public static async Task InvokeOnMainThreadAsync(this UIApplication sharedApplication, Func<Task> asyncDelegateToExecute)
		{
			var tcs = new TaskCompletionSource<Unit>();

			sharedApplication.InvokeOnMainThread(async () =>
			{
				try
				{
					await asyncDelegateToExecute();

					tcs.TrySetResult(Unit.Default);
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			});

			await tcs.Task.ConfigureAwait(false);
		}
	}
}
