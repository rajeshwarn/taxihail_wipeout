using System;
using Cirrious.CrossCore;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyIoC;

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
			var logger = Mvx.Resolve<ILogger>();
			try
			{
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
				logger.LogError(ex);
				if (errorHandler == null)
				{
					Mvx.Resolve<IErrorHandler>().HandleError(ex);
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

