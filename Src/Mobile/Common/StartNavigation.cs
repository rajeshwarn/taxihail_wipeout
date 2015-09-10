using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices.Social;

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

			await TinyIoC.TinyIoCContainer.Current.Resolve<IApplicationInfoService>().CheckVersionAsync(VersionCheck.CheckMinimumSupportedVersion);

			Mvx.Resolve<IAnalyticsService>().ReportConversion();

            if (accountService.CurrentAccount == null
                || (appSettings.Data.CreditCardIsMandatory
                    && accountService.CurrentAccount.DefaultCreditCard == null))
			{
                if (accountService.CurrentAccount != null)
				{
                    accountService.SignOut();
				}

				ShowViewModel<LoginViewModel>();
            }
			else if (@params.ContainsKey ("orderId"))
            {
				var orderId = Guid.Parse(@params["orderId"]);
                bool isPairingNotification;
                bool.TryParse(@params["isPairingNotification"], out isPairingNotification);

				// Make sure to reload notification/payment/network settings even if the user has killed the app
                await accountService.GetNotificationSettings(true);
                await accountService.GetUserTaxiHailNetworkSettings(true);
				await Mvx.Resolve<IPaymentService>().GetPaymentSettings();
                
                try
                {
                    var orderStatus = await Mvx.Resolve<IBookingService>().GetOrderStatusAsync(orderId);
                    var order = await accountService.GetHistoryOrderAsync(orderId);

                    if (order != null && orderStatus != null)
                    {
                        ShowViewModel<BookingStatusViewModel>(new Dictionary<string, string> {
						    {"order", order.ToJson()},
                            {"orderStatus", orderStatus.ToJson()}
                        });
                    }
                }
                catch(Exception)
                {
                    ShowViewModel<HomeViewModel>(new { locateUser = true });
                }
            }
            else
            {
                Task.Run(() =>
                {
                    // Make sure to refresh notification/payment settings even if the user has killed the app
                    accountService.GetNotificationSettings(true);
                    Mvx.Resolve<IPaymentService>().GetPaymentSettings();
                });

                // Log user session start
                metricsService.LogApplicationStartUp();

                ShowViewModel<HomeViewModel>(new { locateUser = true });
            }

            Mvx.Resolve<ILogger>().LogMessage("Startup with server {0}", appSettings.Data.ServiceUrl);
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }
}

