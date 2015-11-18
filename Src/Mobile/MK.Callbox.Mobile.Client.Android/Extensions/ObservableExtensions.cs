using System;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;

namespace apcurium.MK.Callbox.Mobile.Client.Extensions
{
    public static class ObservableExtensions
    {
	    public static IDisposable SubscribeAndLogErrors<T>(this IObservable<T> source, Action<T> onNext)
	    {
		    return source.Subscribe(onNext, Mvx.Resolve<ILogger>().LogError);
	    }
    }
}