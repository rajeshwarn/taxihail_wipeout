using System;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Extensions
{
	public static class IUseServiceClientExtensions
	{
		public static string UseServiceClient<T>(this IUseServiceClient service, Action<T> action, Action<Exception> errorHandler = null) where T : class
		{
			return IUseServiceClientExtensions.UseServiceClient<T>(service, null, action, errorHandler);
		}

		public static string UseServiceClient<T>(this IUseServiceClient service, string name, Action<T> action, Action<Exception> errorHandler = null ) where T : class
		{
			try
			{
				TinyIoCContainer.Current.Resolve<ILogger>().StartStopwatch("UseServiceClient : " + typeof(T));
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
				TinyIoCContainer.Current.Resolve<ILogger>().StopStopwatch("UseServiceClient : " + typeof(T));
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

