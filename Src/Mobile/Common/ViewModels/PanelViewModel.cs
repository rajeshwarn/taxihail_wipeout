using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelViewModel : BaseViewModel
	{
		private IAppContext _appContext;

		public PanelViewModel (IAppContext appContext)
		{
			_appContext = appContext;
		}

		public void SignOut()
		{
			_appContext.SignOut ();
			InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new LogOutRequested(this)));
		}
	}
}

