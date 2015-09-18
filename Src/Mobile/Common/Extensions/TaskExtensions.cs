using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class TaskExtensions
    {
        public static Task HandleErrors(this Task task)
        {
            task.ContinueWith(t=>{

                var logger = TinyIoCContainer.Current.Resolve<ILogger>();

                logger.LogError(t.Exception);

            }, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

	    public static async Task<TValue> ShowProgress<TValue>(this Task<TValue> task)
	    {
		    var service = Mvx.Resolve<IMessageService>();

		    using (service.ShowProgress())
		    {
			    return await task;
		    }
	    }

        public static Task<T> HandleErrors<T>(this Task<T> task)
        {
            task.ContinueWith(t=>{
                
                var logger = TinyIoCContainer.Current.Resolve<ILogger>();

                t.Exception.Handle(x=>{
                    logger.LogError(x);
                    return true;
                });
                
            }, TaskContinuationOptions.OnlyOnFaulted);
            
            return task;
        }

	    public static async void FireAndForget(this Task task)
	    {
		    try
		    {
			    await task;
		    }
		    catch (OperationCanceledException)
		    {
			    // Operation Cancelled exception suppressed because we don't need to worry about a cancelled task.
		    }
		    catch (Exception ex)
		    {
				var logger = TinyIoCContainer.Current.Resolve<ILogger>();

				logger.LogError(ex);
		    }
	    }
    }
}

