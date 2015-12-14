using System;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile
{
	public class StartCallboxNavigation: MvxNavigatingObject, IMvxAppStart
	{
		public async void Start(object hint)
		{
			var logger = Mvx.Resolve<ILogger>();
			try
			{
                logger.LogMessage("Startup with server {0}", Mvx.Resolve<IAppSettings>().GetServiceUrl());
                var accountService = Mvx.Resolve<IAccountService>();

                if (accountService.CurrentAccount == null)
				{
                    accountService.SignOut();
					ShowViewModel<CallboxLoginViewModel>();
				    return;
				}

                var activeOrderStatusDetails = await accountService.GetActiveOrdersStatus();

                var bookingService = Mvx.Resolve<IBookingService>();

                if (activeOrderStatusDetails != null && activeOrderStatusDetails.Any(c => bookingService.IsCallboxStatusActive(c.IBSStatusId)))
				{
					ShowViewModel<CallboxOrderListViewModel>();
				    return;
				}


			    ShowViewModel<CallboxCallTaxiViewModel>();
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