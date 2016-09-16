using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;

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

		public static IObservable<T> CatchAndLogError<T> (this IObservable<T> source, T defaultValue = default (T), [CallerMemberName] string memberName = "", [CallerLineNumber] int lineNumber = -1)
		{
			return source
				.Materialize ()
				.Select(notification => 
				{
					if (notification.Kind != NotificationKind.OnError) 
					{
						return notification;
					}

					TinyIoCContainer.Current.Resolve<ILogger> ().LogError (notification.Exception, memberName, lineNumber);

					return Notification.CreateOnNext (defaultValue);
				})
				.Dematerialize ();

		}
    }
}

