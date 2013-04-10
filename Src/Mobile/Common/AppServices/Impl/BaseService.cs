using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class BaseService: IUseServiceClient
    {

        protected string UseServiceClient<T>(Action<T> action, Action<Exception> errorHandler = null) where T : class
        {
            return this.UseServiceClient<T>(null, action, errorHandler);
        }

        protected string UseServiceClient<T>( string name, Action<T> action, Action<Exception> errorHandler = null ) where T : class
        {
            try
            {

                var method =  TinyIoCContainer.Current.Resolve<ILogger>().GetStack(2);
                TinyIoCContainer.Current.Resolve<ILogger>().StartStopwatch("*************************************   UseServiceClient : " + method);                
				T service;
				if( name == null )
				{
					service = TinyIoCContainer.Current.Resolve<T>();
				}
				else
				{
					service = TinyIoCContainer.Current.Resolve<T>(name);
				}
                action(service);
                TinyIoCContainer.Current.Resolve<ILogger>().StopStopwatch("*************************************   UseServiceClient : " + method);
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
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
            });
            
        }



    }
}