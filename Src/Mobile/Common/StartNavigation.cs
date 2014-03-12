using System;
using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels;

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
					&& currentAccount != null 
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
				var accountService = Mvx.Resolve<IAccountService> ();
				var bookingService = Mvx.Resolve<IBookingService> ();
                
                var orderStatus = bookingService.GetOrderStatus (orderId);
				var order = accountService.GetHistoryOrder(orderId);
                
				if (order != null && orderStatus != null) 
                {
					ShowViewModel<BookingStatusViewModel>(new Dictionary<string, string> {
						{"order", order.ToJson()},
                        {"orderStatus", orderStatus.ToJson()},
                    });
                }
            }
            else
            {
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

