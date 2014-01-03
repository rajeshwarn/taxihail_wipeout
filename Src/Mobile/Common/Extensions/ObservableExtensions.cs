using System;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    internal static class ObservableExtensions
    {
        public static T FirstOrDefaultCatchAll<T>(this IObservable<T> thisObservable)
        {
            try{
                return thisObservable.FirstOrDefault();
            }
            catch{
                return default(T);
            }
        }
    }
}

