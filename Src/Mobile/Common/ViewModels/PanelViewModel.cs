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

		public MvxRelayCommand NavigateToRatingPage
		{
			get
			{
				return new MvxRelayCommand(() =>
				                           {
					RequestNavigate<BookRatingViewModel>(new { orderId = "7BE53757-B6D0-4CD9-923A-F643F59454F5", canRate = true });
				});
			}
		}


	}
}

