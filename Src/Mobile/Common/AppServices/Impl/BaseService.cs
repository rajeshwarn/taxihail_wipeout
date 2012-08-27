using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BaseService
    {

        protected string UseServiceClient<T>(Action<T> action) where T : class
        {
			return UseServiceClient<T>( null, action );
//            try
//            {
//                TinyIoCContainer.Current.Resolve<ILogger>().StartStopwatch("UseServiceClient : " + typeof(T));
//                var service = TinyIoCContainer.Current.Resolve<T>();
//                action(service);
//                TinyIoCContainer.Current.Resolve<ILogger>().StopStopwatch("UseServiceClient : " + typeof(T));
//                return "";
//            }
//            catch (Exception ex)
//            {
//                    
//                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
//				TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError( ex );
//                return ex.Message;
//            
//            }
        }

        protected string UseServiceClient<T>( string name, Action<T> action) where T : class
        {
            try
            {
                TinyIoCContainer.Current.Resolve<ILogger>().StartStopwatch("UseServiceClient : " + typeof(T));
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
                TinyIoCContainer.Current.Resolve<ILogger>().StopStopwatch("UseServiceClient : " + typeof(T));
                return "";
            }
            catch (Exception ex)
            {
                    
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
				TinyIoCContainer.Current.Resolve<IErrorHandler>().HandleError( ex );
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