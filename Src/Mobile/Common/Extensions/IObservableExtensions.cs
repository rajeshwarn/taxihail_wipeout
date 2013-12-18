using System;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class IObservableExtensions
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

