using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;
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

			var creditCardIsMandatory = TinyIoCContainer.Current.Resolve<IAppSettings>().Data.CreditCardIsMandatory;
			var currentAccount = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;

			if (currentAccount == null 
				|| (creditCardIsMandatory 
					&& !currentAccount.DefaultCreditCard.HasValue))
			{
				if (currentAccount != null)
				{
					TinyIoCContainer.Current.Resolve<IAccountService>().SignOut();
				}

				ShowViewModel<LoginViewModel>();
            }
			else if (@params.ContainsKey ("orderId"))
            {
				var orderId = Guid.Parse(@params["orderId"]);
                bool isPairingNotification;
                bool.TryParse(@params["isPairingNotification"], out isPairingNotification);

				var accountService = Mvx.Resolve<IAccountService> ();
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
                TinyIoCContainer.Current.Resolve<IAccountService>().LogApplicationStartUp();

				ShowViewModel<HomeViewModel>(new { locateUser =  true });
            }

			TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().Data.ServiceUrl);
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }
}

