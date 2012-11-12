using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelViewModel : BaseViewModel
	{

		public PanelViewModel ()
		{
		}

		public void SignOut()
		{
            TinyIoCContainer.Current.Resolve<IAccountService>().SignOut();			
			InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new LogOutRequested(this)));
		}

		public MvxRelayCommand NavigateToOrderHistory
		{
			get
			{
				return new MvxRelayCommand(() =>
				                           {
					RequestNavigate<HistoryViewModel>();
				});
			}
		}


	}
}

