using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {

		protected async Task<TResult> UseServiceClientAsync<TService, TResult>(Func<TService, Task<TResult>> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where TResult : class  where TService : class
        {
            var service = TinyIoCContainer.Current.Resolve<TService>();

            try
            {
                using(Logger.StartStopwatch("*************************************   UseServiceClient : " + method))
                {
                    return await action(service)
                            .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {                    
                Logger.LogError(ex);
				if (errorHandler == null)
				{
					TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
				}
				else
				{
					errorHandler (ex);
				} 
                throw;
            }
        }

		protected async Task UseServiceClientAsync<TService>(Func<TService, Task> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where TService : class
		{
			var service = TinyIoCContainer.Current.Resolve<TService>();

			try
			{
				using(Logger.StartStopwatch("*************************************   UseServiceClient : " + method))
				{
					await action(service).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{                    
				Logger.LogError(ex);
				if (errorHandler == null)
				{
					TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
				}
				else
				{
					errorHandler (ex);
				} 
				throw;
			}
		}

		[Obsolete("Use Async Version. This one is for legacy code not yet migrated to async / await")]
		protected TResult UseServiceClientTask<TService, TResult>(Func<TService, Task<TResult>> action,string serviceName = null,[CallerMemberName] string method = "")
            where TResult : class
            where TService : class
        {
			var service = serviceName == null ? 
			              	TinyIoCContainer.Current.Resolve<TService>()
			              : TinyIoCContainer.Current.Resolve<TService>(serviceName);

            try
            {
                using (Logger.StartStopwatch("*************************************   UseServiceClient : " + method))
                {
                     var task = action(service);
                     task.Wait();
                     return task.Result;
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(x =>
                {
                    Logger.LogError(x);
                    TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(x);
                    return true;
                });
                
            }
            return default(TResult);
        }

		[Obsolete("Use Async Version. This one is for legacy code not yet migrated to async / await")]
		protected void UseServiceClientTask<TService>(Func<TService, Task> action, [CallerMemberName] string method = "")
			where TService : class
		{
			var service = TinyIoCContainer.Current.Resolve<TService>();

			try
			{
				using (Logger.StartStopwatch("*************************************   UseServiceClient : " + method))
				{
					var task = action(service);
					task.Wait();
				}
			}
			catch (AggregateException ex)
			{
				ex.Handle(x =>
					{
						Logger.LogError(x);
						TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(x);
						return true;
					});

			}
		}

        private ILogger _logger;
        protected ILogger Logger
        {
            get
            {
				return _logger ?? (_logger = Mvx.Resolve<ILogger>());
            }
        }

		protected ICacheService UserCache
        {
            get
            {
                return TinyIoCContainer.Current.Resolve<ICacheService>("UserAppCache");
            }
        }

    }
}