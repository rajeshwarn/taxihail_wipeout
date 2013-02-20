using System;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Extensions;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading.Tasks;
using System.Threading;
using apcurium.MK.Common.Configuration;
using System.Collections.Generic;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using ServiceStack.Text;


namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation
                : MvxApplicationObject,
                  IMvxStartNavigation,
                  IMvxServiceConsumer<IAccountService>,
                  IMvxServiceConsumer<IBookingService>
    {
        readonly IDictionary<string, string> _params;
        public StartNavigation (IDictionary<string, string> @params)
        {
            _params = @params ?? new Dictionary<string, string>();
        }

        public void Start ()
        {
            //if (TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount == null)
            //{
            //    RequestNavigate<LoginViewModel>();
            //}
            //else
            //{
            //    if(TinyIoCContainer.Current.Resolve<IAppSettings>().IsCMT)
                   RequestNavigate<CmtHomeViewModel>();

                //if(!TinyIoCContainer.Current.Resolve<IAppSettings>().IsCMT)
                   // RequestNavigate<BookViewModel>();
            if (TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService> ().CurrentAccount == null) {
                RequestNavigate<LoginViewModel> ();
            } 
            else if (_params.ContainsKey ("orderId"))
            {
                var orderId = Guid.Parse(_params["orderId"]);
                var accountService = this.GetService<IAccountService> ();
                var bookingService = this.GetService<IBookingService> ();
                var orderStatus = bookingService.GetOrderStatus (orderId);
                var order = accountService.GetHistoryOrder (orderId);
                
                if (order != null && orderStatus != null) {
                    
                    RequestNavigate<BookingStatusViewModel>(new Dictionary<string, string> {
                        {"order", order.ToJson()},
                        {"orderStatus", orderStatus.ToJson()},
                    },clearTop: true);

            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl);
        }

       

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }

}

