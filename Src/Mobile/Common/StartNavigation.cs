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
using TinyIoC;

namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation:
            MvxNavigatingObject,
            IMvxAppStart
    {
		public void Start (object hint)
        {
			var @params = (Dictionary<string, string>)hint;

			JsConfig.DateHandler = JsonDateHandler.ISO8601; //MKTAXI-849 it's here because cache service use servicetacks deserialization so it needs it to correctly deserezialised expiration date...

			var accountService = Mvx.Resolve<IAccountService> ();
			var paymentService = Mvx.Resolve<IPaymentService> ();

			// Make sure to reload notification/payment settings even if the user has killed the app
			accountService.GetNotificationSettings(true, true);
			paymentService.GetPaymentSettings(true);

			if (accountService.CurrentAccount == null 
				|| (Mvx.Resolve<IAppSettings> ().Data.CreditCardIsMandatory 
					&& !accountService.CurrentAccount.DefaultCreditCard.HasValue))
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

				var bookingService = Mvx.Resolve<IBookingService> ();
                
                var orderStatus = bookingService.GetOrderStatus (orderId);
				var order = accountService.GetHistoryOrder(orderId);
                
				if (order != null && orderStatus != null) 
                {
                    if (isPairingNotification)
                    {
                        ShowViewModel<ConfirmPairViewModel>(new
                        {
                            order = order.ToJson(),
                            orderStatus = orderStatus.ToJson()
                        }.ToStringDictionary());
                    }
                    else
                    {
                        ShowViewModel<BookingStatusViewModel>(new Dictionary<string, string> {
						    {"order", order.ToJson()},
                            {"orderStatus", orderStatus.ToJson()},
                        });
                    }
                }
            }
            else
            {
                // Log user session start
				accountService.LogApplicationStartUp();

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

