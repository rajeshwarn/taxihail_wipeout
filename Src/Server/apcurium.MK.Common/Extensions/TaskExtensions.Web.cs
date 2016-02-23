using System.Threading.Tasks;
using apcurium.MK.Common.Diagnostic;
using Microsoft.Practices.ServiceLocation;

namespace apcurium.MK.Common.Extensions
{
    public static class TaskExtensions
    {
        public static Task<T> HandleErrors<T>(this Task<T> task)
        {
            task.ContinueWith(InnerHandleError, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

        public static Task HandleErrors(this Task task)
        {
            task.ContinueWith(InnerHandleError, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }

        private static void InnerHandleError(Task task)
        {
            var logger = ServiceLocator.Current.GetInstance<ILogger>();

            logger.LogError(task.Exception);
        }
    }
}
