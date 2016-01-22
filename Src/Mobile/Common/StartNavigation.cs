using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation : MvxNavigatingObject, IMvxAppStart
    {
        public async void Start (object hint)
        {
			var @params = (Dictionary<string, string>)hint;

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

            if (accountService.CurrentAccount == null)
			{
                HandleAccountNotInValidState(accountService);
			}
			else
			{
                // Make sure to reload notification/payment/network settings even if the user has killed the app
                await Task.WhenAll(
                    accountService.GetNotificationSettings(true).HandleErrors(), 
                    accountService.GetUserTaxiHailNetworkSettings(true).HandleErrors(), 
                    Mvx.Resolve<IPaymentService>().GetPaymentSettings(true).HandleErrors()
                );

                // payment settings here are cached so not an issue if we refetch them, at least we are
                // sure we have the latest settings and calling it here will not throw an Unauthorized exception
                var paymentSettings = await Mvx.Resolve<IPaymentService>().GetPaymentSettings();
                if (paymentSettings.CreditCardIsMandatory
                   && accountService.CurrentAccount.DefaultCreditCard == null)
                {
                    HandleAccountNotInValidState(accountService);
                    return;
                }

                // Don't include it in the previous Task.WhenAll since we need to make sure PaymentSettings are refreshed before calling this
                await Mvx.Resolve<IDeviceCollectorService>().GenerateNewSessionIdAndCollect().HandleErrors();

                // Log user session start
                metricsService.LogApplicationStartUp();

				if (@params.ContainsKey("orderId"))
				{
					var orderId = Guid.Parse(@params["orderId"]);
					bool isPairingNotification;
					bool.TryParse(@params["isPairingNotification"], out isPairingNotification);
					
					try
					{
						var orderStatus = await Mvx.Resolve<IBookingService>().GetOrderStatusAsync(orderId);
						var order = await accountService.GetHistoryOrderAsync(orderId);
						if (order != null && orderStatus != null)
						{
							ShowViewModel<HomeViewModel>(new {
								locateUser = false,
								order = order.ToJson(),
								orderStatusDetail = orderStatus.ToJson()
							});
						}
					}
					catch (Exception ex)
					{
						var logger = Mvx.Resolve<ILogger>();
						logger.LogMessage("An error occurred while handling notifications");
						logger.LogError(ex);
						ShowViewModel<HomeViewModel>(new { locateUser = true });
					}
				}
				else
				{
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
			}

			Mvx.Resolve<ILogger>().LogMessage("Startup with server {0}", appSettings.GetServiceUrl());
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }

        private void HandleAccountNotInValidState(IAccountService accountService)
        {
            if (accountService.CurrentAccount != null)
            {
                accountService.SignOut();
            }

            // Don't check the app version here since it's done in the LoginViewModel 
            // and HomeViewModel and causes problems on iOS 9+

            ShowViewModel<LoginViewModel>();
        }
    }
}

