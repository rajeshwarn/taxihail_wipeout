using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.ViewModels.Payment.Cmt;

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

            Task.Factory.SafeStartNew( () => TinyIoCContainer.Current.Resolve<ICacheService>().Set<string>( "Client.NumberOfCharInRefineAddress", TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSetting( "Client.NumberOfCharInRefineAddress" )));

            if (TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount == null)
			{
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
				ShowViewModel<HomeViewModel>();
            }

            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl);
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }
}

