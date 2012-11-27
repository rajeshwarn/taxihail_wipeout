using System;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;
using System.Threading.Tasks;
using System.Threading;

namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation
                : MvxApplicationObject
                    , IMvxStartNavigation
    {
        public void Start()
        {
            if (TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount == null)
            {
                RequestNavigate<LoginViewModel>();
            }
            else
            {
                if(TinyIoCContainer.Current.Resolve<IAppSettings>().IsCMT)
                   RequestNavigate<CmtBookViewModel>();

                if(!TinyIoCContainer.Current.Resolve<IAppSettings>().IsCMT)
                    RequestNavigate<BookViewModel>();
            }


            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl);
        }

       

        public bool ApplicationCanOpenBookmarks
        {
            get { return true; }
        }
    }

}

