using System.Threading.Tasks;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

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

    }
}

