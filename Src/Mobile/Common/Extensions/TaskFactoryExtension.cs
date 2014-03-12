using System;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class TaskFactoryExtension
    {
        public static Task SafeStartNew(this TaskFactory taskFactory, Action action)
        {
            var t = taskFactory.StartNew( o=> action(), null, TaskCreationOptions.None);
            t.HandleErrors();
            return t;
        }

        public static Task<TResult> SafeStartNew<TResult>(this TaskFactory taskFactory, Func<TResult> action, CancellationToken token )
        {
            var t = taskFactory.StartNew( action , token);
            t.HandleErrors ();
            return t;
        }

        public static Task<TResult> SafeStartNew<TResult>(this TaskFactory<TResult> taskFactory, Func<TResult> action )
        {
            Task<TResult> t = taskFactory.StartNew( action );
            t.HandleErrors ();
            return t;
        }
    }
}

