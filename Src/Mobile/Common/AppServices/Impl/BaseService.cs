using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Runtime.CompilerServices;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {
        protected string UseServiceClient<T>(Action<T> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where T : class
        {
			var service = TinyIoCContainer.Current.Resolve<T>();
            return this.UseServiceClient<T>(service, action, errorHandler, method);
        }

        protected string UseServiceClient<T>( string name, Action<T> action, Action<Exception> errorHandler = null, [CallerMemberName] string method = "") where T : class
        {
			var service = TinyIoCContainer.Current.Resolve<T>(name);
			return this.UseServiceClient<T>(service, action, errorHandler, method);
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
                ILogger logger = null;
                if (TinyIoCContainer.Current.TryResolve<ILogger> ( out logger)) {
                    TinyIoCContainer.Current.Resolve<ILogger> ().LogError (ex);
				if (errorHandler == null)
				{
                        TinyIoCContainer.Current.Resolve<IErrorHandler> ().HandleError (ex);
				}
				else
				{
                        errorHandler (ex);
                    }                 
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
                return _logger ?? (_logger = TinyIoCContainer.Current.Resolve<ILogger>());
            }
        }
        protected IAppCacheService Cache
        {
            get
            {
                return TinyIoCContainer.Current.Resolve<IAppCacheService>();
            }
        }



    }
}