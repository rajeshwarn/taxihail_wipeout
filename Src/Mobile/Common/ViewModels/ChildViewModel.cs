using System;
using Cirrious.MvvmCross.ViewModels;
using System.Runtime.CompilerServices;
using System.Reactive.Disposables;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class ChildViewModel: MvxViewModel, IDisposable
    {
		readonly CompositeDisposable _subscriptions = new CompositeDisposable();
		protected new void RaisePropertyChanged([CallerMemberName]string whichProperty = null)
		{
			base.RaisePropertyChanged(whichProperty);
		}
		
		protected void Observe<T>(IObservable<T> observable, Action<T> onNext)
		{
			observable
				.Subscribe(x => {
					InvokeOnMainThread(() => onNext(x));
				})
				.DisposeWith(_subscriptions);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected AsyncCommand GetCommand(Action action)
		{
			return new AsyncCommand(action);
		}

		protected AsyncCommand<T> GetCommand<T>(Action<T> action)
		{
			return new AsyncCommand<T>(action);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				_subscriptions.Dispose();
			}
		}

		protected TViewModel AddChild<TViewModel>(Func<TViewModel> builder)
			where TViewModel: ChildViewModel
		{
			var viewModel = builder.Invoke();
			viewModel.CallBundleMethods("Init", new MvxBundle());
			viewModel.DisposeWith(_subscriptions);
			return viewModel;
		}

		protected TViewModel AddChild<TViewModel>()
			where TViewModel: ChildViewModel
		{
			return AddChild<TViewModel>(() => Mvx.IocConstruct<TViewModel>());
		}
    }
}

