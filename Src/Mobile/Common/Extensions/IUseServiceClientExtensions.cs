using System;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Extensions
{
	public static class UseServiceClientExtensions
	{
		public static string UseServiceClient<T>(this IUseServiceClient service, Action<T> action, Action<Exception> errorHandler = null) where T : class
		{
			return UseServiceClient(service, null, action, errorHandler);
		}

		public static string UseServiceClient<T>(this IUseServiceClient service, string name, Action<T> action, Action<Exception> errorHandler = null ) where T : class
		{
			try
			{
                var logger = TinyIoCContainer.Current.Resolve<ILogger>();
				using(logger.StartStopwatch("UseServiceClient : " + typeof(T)))
                {
    				T client;
    				if( name == null )
    				{
    					client = TinyIoCContainer.Current.Resolve<T>();
    				}
    				else
    				{
    					client = TinyIoCContainer.Current.Resolve<T>(name);
    				}
    				action(client);
                }
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
	}
}

