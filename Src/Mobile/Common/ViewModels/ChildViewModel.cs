using System;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MK.Common.Configuration;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;

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
		}

		protected TinyIoCContainer Container
		{
			get { return TinyIoCContainer.Current; }
		}

		protected ILogger Logger { get { return Container.Resolve<ILogger>(); } }

		public TaxiHailSetting Settings { get { return Container.Resolve<IAppSettings>().Data; } }

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

