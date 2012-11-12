using System;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile
{
    public class StartNavigation
                : MvxApplicationObject
                    , IMvxStartNavigation
    {
        public void Start()
        {
            bool isUpToDate;
            try
            {
                var app = TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetAppInfo();

                 isUpToDate = app.Version.StartsWith("1.1.");
            }
            catch(Exception e)
            {
                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("NoConnectionTitle");
                var msg = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("NoConnectionMessage");
                var mService = TinyIoCContainer.Current.Resolve<IMessageService>();
                mService.ShowMessage(title, msg);

                isUpToDate = true;
            }

            if (!isUpToDate)
            {

                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateTitle");
                var msg = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("AppNeedUpdateMessage");
                var mService = TinyIoCContainer.Current.Resolve<IMessageService>();
                mService.ShowMessage(title, msg);
                RequestNavigate<LoginViewModel>(true);

            }
            else if (TinyIoC.TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount == null)
            {
                RequestNavigate<LoginViewModel>();


            }
            else
            {
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

