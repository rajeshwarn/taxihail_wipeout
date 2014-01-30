using System;
using Cirrious.MvvmCross.ViewModels;
using System.Runtime.CompilerServices;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class ChildViewModel: MvxNavigatingObject, IDisposable
    {
		readonly CompositeDisposable _subscriptions = new CompositeDisposable();
		protected new void RaisePropertyChanged([CallerMemberName]string whichProperty = null)
		{
			base.RaisePropertyChanged(whichProperty);
		}

		protected void Observe<T>(IObservable<T> observable, Action<T> onNext)
		{
			observable
				.Subscribe(x => InvokeOnMainThread(() => onNext(x)))
				.DisposeWith(_subscriptions);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				_subscriptions.Dispose();
			}
		}
    }
}

