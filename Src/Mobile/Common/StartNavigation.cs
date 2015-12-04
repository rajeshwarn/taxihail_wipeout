using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation : MvxNavigatingObject, IMvxAppStart
    {
		public async void Start (object hint)
        {
			var @params = (Dictionary<string, string>)hint;

			JsConfig.DateHandler = JsonDateHandler.ISO8601; //MKTAXI-849 it's here because cache service use servicetacks deserialization so it needs it to correctly deserezialised expiration date...

		    var appSettings = Mvx.Resolve<IAppSettings>();
		    var accountService = Mvx.Resolve<IAccountService>();
		    var facebookService = Mvx.Resolve<IFacebookService>();
		    var metricsService = Mvx.Resolve<IMetricsService>();

            await appSettings.Load();

            if (appSettings.Data.FacebookEnabled)
            {
                facebookService.Init();
            }

            if (appSettings.Data.FacebookPublishEnabled)
            {
                facebookService.PublishInstall();
            }

			Mvx.Resolve<IAnalyticsService>().ReportConversion();

            if (accountService.CurrentAccount == null
                || (appSettings.Data.CreditCardIsMandatory
                    && accountService.CurrentAccount.DefaultCreditCard == null))
			{
                if (accountService.CurrentAccount != null)
				{
                    accountService.SignOut();
				}

                // Don't check the app version here since it's done in the LoginViewModel 
				// and HomeViewModel and causes problems on iOS 9+

				ShowViewModel<LoginViewModel>();
            }
			else if (@params.ContainsKey ("orderId"))
            {
				var orderId = Guid.Parse(@params["orderId"]);
                bool isPairingNotification;
                bool.TryParse(@params["isPairingNotification"], out isPairingNotification);

				// Make sure to reload notification/payment/network settings even if the user has killed the app
	            await Task.WhenAll(
						accountService.GetNotificationSettings(true).HandleErrors(),
						accountService.GetUserTaxiHailNetworkSettings(true).HandleErrors(),
						Mvx.Resolve<IPaymentService>().GetPaymentSettings().HandleErrors()
		            );

                try
                {
                    var orderStatus = await Mvx.Resolve<IBookingService>().GetOrderStatusAsync(orderId);
                    var order = await accountService.GetHistoryOrderAsync(orderId);

                    if (order != null && orderStatus != null)
                    {
						ShowViewModel<HomeViewModel>(new
						{
							locateUser = false,
							order = order.ToJson(),
							orderStatusDetail = orderStatus.ToJson()
						});
                    }
                }
                catch(Exception ex)
                {
					var logger = Mvx.Resolve<ILogger>();

					logger.LogMessage("An error occurred while handling notifications");
					logger.LogError(ex);

                    ShowViewModel<HomeViewModel>(new { locateUser = true });
                }
            }
            else
            {
				// Make sure to refresh notification/payment settings even if the user has killed the app
				await Task.WhenAll(
						accountService.GetNotificationSettings(true).HandleErrors(),
						Mvx.Resolve<IPaymentService>().GetPaymentSettings().HandleErrors()
					);

                // Log user session start
                metricsService.LogApplicationStartUp();

				var hasLastOrder = Mvx.Resolve<IBookingService>().HasLastOrder;
				if (hasLastOrder)
				{
					ShowViewModel<ExtendedSplashScreenViewModel>(new { preventShowViewAnimation = "NotUsed" });
				}
				else
				{
					ShowViewModel<HomeViewModel>(new { locateUser = true });
				}
            }

            Mvx.Resolve<ILogger>().LogMessage("Startup with server {0}", appSettings.Data.ServiceUrl);
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }
}

