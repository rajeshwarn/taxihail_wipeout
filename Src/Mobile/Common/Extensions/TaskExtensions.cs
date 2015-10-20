using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class TaskExtensions
    {
		public static Task HandleErrors(this Task task, [CallerMemberName] string callerName = "Unknown method", [CallerLineNumber] int callerLineNumber = 0)
        {
            task.ContinueWith(t=>
			{

				var logger = Mvx.Resolve<ILogger>();

				t.Exception.Handle(x =>
				{
					logger.LogMessage("An error occurred while executing a task under {0} at line {1}", callerName, callerLineNumber);
					logger.LogError(x);
					return true;
				});

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

        public static Task<T> HandleErrors<T>(this Task<T> task, [CallerMemberName] string callerName = "Unknown method", [CallerLineNumber] int callerLineNumber = 0)
        {
            task.ContinueWith(t =>
			{    
                var logger = Mvx.Resolve<ILogger>();

                t.Exception.Handle(x=>
				{
					logger.LogMessage("An error occurred while executing a task under {0} at line {1}", callerName, callerLineNumber);
                    logger.LogError(x);
                    return true;
                });

				return Task.FromResult(default(T));
			}, TaskContinuationOptions.OnlyOnFaulted);
            
            return task;
        }

		public static async void FireAndForget(this Task task, [CallerMemberName] string callerName = "Unknown method", [CallerLineNumber] int callerLineNumber = 0)
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
				var logger = Mvx.Resolve<ILogger>();

				logger.LogMessage("An error occurred while executing a task under {0} at line {1}", callerName, callerLineNumber);
				logger.LogError(ex);
		    }
	    }
    }
}

