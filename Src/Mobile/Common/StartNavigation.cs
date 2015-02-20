using System;
using System.Collections.Generic;
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
    public class StartNavigation:
            MvxNavigatingObject,
            IMvxAppStart
    {
		public async void Start (object hint)
        {
			var @params = (Dictionary<string, string>)hint;

			JsConfig.DateHandler = JsonDateHandler.ISO8601; //MKTAXI-849 it's here because cache service use servicetacks deserialization so it needs it to correctly deserezialised expiration date...

			await Mvx.Resolve<IAppSettings>().Load();

			if (Mvx.Resolve<IAppSettings>().Data.FacebookEnabled)
			{
				Mvx.Resolve<IFacebookService> ().Init ();
			}
			if (Mvx.Resolve<IAppSettings>().Data.FacebookPublishEnabled) 
			{
				Mvx.Resolve<IFacebookService>().PublishInstall();
			}

			Mvx.Resolve<IAnalyticsService>().ReportConversion();

			if (Mvx.Resolve<IAccountService>().CurrentAccount == null 
				|| (Mvx.Resolve<IAppSettings> ().Data.CreditCardIsMandatory 
					&& !Mvx.Resolve<IAccountService>().CurrentAccount.DefaultCreditCard.HasValue))
			{
				if (Mvx.Resolve<IAccountService>().CurrentAccount != null)
				{
					Mvx.Resolve<IAccountService>().SignOut();
				}

				ShowViewModel<LoginViewModel>();
            }
			else if (@params.ContainsKey ("orderId"))
            {
				var orderId = Guid.Parse(@params["orderId"]);
                bool isPairingNotification;
                bool.TryParse(@params["isPairingNotification"], out isPairingNotification);

				// Make sure to reload notification/payment/network settings even if the user has killed the app
				await Mvx.Resolve<IAccountService>().GetNotificationSettings(true, true);
				await Mvx.Resolve<IPaymentService>().GetPaymentSettings(true);
                await Mvx.Resolve<IAccountService>().GetUserTaxiHailNetworkSettings(true);

                try
                {
                    var orderStatus = await Mvx.Resolve<IBookingService>().GetOrderStatusAsync(orderId);
                    var order = await Mvx.Resolve<IAccountService>().GetHistoryOrderAsync(orderId);

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
				// Make sure to reload notification/payment settings even if the user has killed the app
				await Mvx.Resolve<IAccountService>().GetNotificationSettings(true, true);
				await Mvx.Resolve<IPaymentService>().GetPaymentSettings(true);

                // Log user session start
				Mvx.Resolve<IAccountService>().LogApplicationStartUp();

				ShowViewModel<HomeViewModel>(new { locateUser =  true });
            }

			Mvx.Resolve<ILogger>().LogMessage("Startup with server {0}", Mvx.Resolve<IAppSettings>().Data.ServiceUrl);
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }
}

