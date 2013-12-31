using System;
using System.Net;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helper;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Diagnostic;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.UIKit;
using ServiceStack.ServiceClient.Web;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostics
{
    public class ErrorHandler : IErrorHandler
    {
        public static DateTime LastConnectError = DateTime.MinValue;


        public void HandleError (Exception ex)
        {
			if (ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.Unauthorized) 
            {
				LastConnectError=DateTime.Now;
				MessageHelper.Show (Resources.ServiceErrorCallTitle, Resources.ServiceErrorUnauthorized, () => {
					UIApplication.SharedApplication.InvokeOnMainThread (() => {
						var dispatch = TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider> ().Dispatcher;
						dispatch.RequestNavigate (new MvxShowViewModelRequest (typeof(LoginViewModel), null, true, MvxRequestedBy.UserAction));
					});

					TinyIoCContainer.Current.Resolve<IAccountService> ().SignOut ();
				});
			}
			else if (ex is WebException && ((WebException)ex).Status == WebExceptionStatus.ConnectFailure)
			{
				LastConnectError=DateTime.Now;
				var title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionTitle");
				var msg = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionMessage");
				var mService = TinyIoCContainer.Current.Resolve<IMessageService> ();
				mService.ShowMessage (title, msg);
			}
        }
    }
}

