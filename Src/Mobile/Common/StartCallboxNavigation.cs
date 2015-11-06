using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile
{
	public class StartCallboxNavigation: MvxNavigatingObject, IMvxAppStart
	{
		public async void Start(object hint)
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601; //MKTAXI-849 it's here because cache service use servicetacks deserialization so it needs it to correctly deserezialised expiration date...

			var logger = Mvx.Resolve<ILogger>();
			try
			{
				var accountService = Mvx.Resolve<IAccountService>();

				var activeOrderStatusDetails = await accountService.GetActiveOrdersStatus();

			    var bookingService = Mvx.Resolve<IBookingService>();

                if (accountService.CurrentAccount == null)
				{
					ShowViewModel<CallboxLoginViewModel>();
				}
				else if (activeOrderStatusDetails != null && activeOrderStatusDetails.Any(c => bookingService.IsCallboxStatusActive(c.IBSStatusId)))
				{
					ShowViewModel<CallboxOrderListViewModel>();
				}
				else
				{
					ShowViewModel<CallboxCallTaxiViewModel>();
				}

				logger.LogMessage("Startup with server {0}", Mvx.Resolve<IAppSettings>().Data.ServiceUrl);
			}
			catch (Exception ex)
			{
				logger.LogMessage("An error occurred while starting the app, attempting to go to Login screen");
				logger.LogError(ex);

				ShowViewModel<CallboxLoginViewModel>();
			}
			
		}

		public bool ApplicationCanOpenBookmarks
		{
			get { return true; }
		}
	}
}