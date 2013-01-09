using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile
{
    public class StartCallboxNavigation : MvxApplicationObject
                    , IMvxStartNavigation
    {
        public void Start()
        {


            TinyIoCContainer.Current.Resolve<IConfigurationManager>().Reset();
            
            if (TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount == null)
            {
                RequestNavigate<CallboxLoginViewModel>();
            }
            else if (TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().GetActiveOrdersStatus().Any(c => TinyIoC.TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IBSStatusId)))
            {
                RequestNavigate<CallboxOrderListViewModel>();
            }
            else
            {
                RequestNavigate<CallboxCallTaxiViewModel>();
            }

            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl);
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }

}

