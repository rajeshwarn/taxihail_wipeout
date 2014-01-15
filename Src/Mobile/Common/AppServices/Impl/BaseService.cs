using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {
        protected string UseServiceClient<T>(Action<T> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where T : class
        {
			var service = TinyIoCContainer.Current.Resolve<T>();
            return UseServiceClient(service, action, errorHandler, method);
        }

        protected string UseServiceClient<T>( string name, Action<T> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where T : class
        {
			var service = TinyIoCContainer.Current.Resolve<T>(name);
			return UseServiceClient(service, action, errorHandler, method);
        }

        private string UseServiceClient<T>(T service, Action<T> action, Action<Exception> errorHandler, string method) where T : class
		{
			try
			{
                using(Logger.StartStopwatch("*************************************   UseServiceClient : " + method))
                {
				    action(service);
                }
				return "";
			}
			catch (Exception ex)
            {   
				Logger.LogError (ex);
			    if (errorHandler == null)
			    {
					TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
			    }
			    else
			    {
                   errorHandler (ex);
                }                 
				return ex.Message;

			}
		}

        protected async Task<TResult> UseServiceClient<TService, TResult>(Func<TService, Task<TResult>> action, [CallerMemberName] string method = "") where TResult : class  where TService : class
        {
            var service = TinyIoCContainer.Current.Resolve<TService>();

            try
            {
                using(Logger.StartStopwatch("*************************************   UseServiceClient : " + method))
                {
                    return await action(service);
                }
            }
            catch (Exception ex)
            {                    
                Logger.LogError(ex);
                TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(ex);
            }
            return default(TResult);
        }

        protected TResult UseServiceClientAsync<TService, TResult>(Func<TService, Task<TResult>> action, [CallerMemberName] string method = "")
            where TResult : class
            where TService : class
        {
            var service = TinyIoCContainer.Current.Resolve<TService>();

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

        protected void QueueCommand<T>(Action<T> action) where T : class
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    UseServiceClient(action);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            });
            
        }

        private ILogger _logger;
        protected ILogger Logger
        {
            get
            {
				return _logger ?? (_logger = Mvx.Resolve<ILogger>());
            }
        }

        protected ICacheService Cache
        {
            get
            {
                return TinyIoCContainer.Current.Resolve<ICacheService>("AppCache");
            }
        }

    }
}