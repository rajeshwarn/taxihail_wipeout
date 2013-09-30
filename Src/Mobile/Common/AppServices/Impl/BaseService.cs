using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {
        protected string UseServiceClient<T>(Action<T> action, Action<Exception> errorHandler = null) where T : class
        {
			var service = TinyIoCContainer.Current.Resolve<T>();
            return this.UseServiceClient<T>(service, action, errorHandler);
        }

        protected string UseServiceClient<T>( string name, Action<T> action, Action<Exception> errorHandler = null ) where T : class
        {
			var service = TinyIoCContainer.Current.Resolve<T>(name);
			return this.UseServiceClient<T>(service, action, errorHandler);
        }

		private string UseServiceClient<T>(T service, Action<T> action, Action<Exception> errorHandler) where T : class
		{
			try
			{
                var method =  Logger.GetStack(3);
                Logger.StartStopwatch("*************************************   UseServiceClient : " + method);                
				action(service);
                Logger.StopStopwatch("*************************************   UseServiceClient : " + method);
				return "";
			}
			catch (Exception ex)
			{                    
				TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
				if (errorHandler == null)
				{
					TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(ex);
				}
				else
				{
					errorHandler(ex);
				}
				return ex.Message;

			}
		}

        protected Task<TResult> UseServiceClient<TService, TResult>(Func<TService, TResult> action) where TResult : class  where TService : class
        {
            var service = TinyIoCContainer.Current.Resolve<TService>();
            var method =  Logger.GetStack(2);

            return Task.Run(() => {
                try
                {
                    Logger.StartStopwatch("*************************************   UseServiceClient : " + method);
                    Thread.Sleep(10000);
                    var result =  action(service);
                    Logger.StopStopwatch("*************************************   UseServiceClient : " + method);
                    return result;
                }
                catch (Exception ex)
                {                    
                    Logger.LogError(ex);
                    TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError(ex);
                }
                return default(TResult);
            });
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



    }
}