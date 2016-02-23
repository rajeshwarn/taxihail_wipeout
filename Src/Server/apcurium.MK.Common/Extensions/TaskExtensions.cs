using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Framework.Extensions
{
    public static class TaskExtensions
    {
        public static void Subscribe(this Task thisTask, Action action)
        {
            thisTask.ContinueWith(t =>
                {
                    action();
                });
        }
        public static void Subscribe<T>(this Task<T> thisTask, Action action)
        {
            thisTask.ContinueWith(t =>
            {
                action();
            });
        }
        public static void Subscribe<T>(this Task<T> thisTask, Action<T> action)
        {
            thisTask.ContinueWith(t =>
            {
                action(t.Result);
            });
        }
    }
}
