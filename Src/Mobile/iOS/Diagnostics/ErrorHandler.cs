using System;
using System.Net;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using TinyIoC;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class ErrorHandler : IErrorHandler
    {
        public static DateTime _lastConnectError = DateTime.MinValue;

        public ErrorHandler ()
        {
        }

        public void HandleError (Exception ex)
        {
			if (ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.Unauthorized) 
            {
				_lastConnectError=DateTime.Now;
				MessageHelper.Show (Resources.ServiceErrorCallTitle, Resources.ServiceErrorUnauthorized, () => {
					UIApplication.SharedApplication.InvokeOnMainThread (() => {
						var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider> ().Dispatcher;
						dispatch.RequestNavigate (new MvxShowViewModelRequest (typeof(LoginViewModel), null, true, MvxRequestedBy.UserAction));
					});

					TinyIoCContainer.Current.Resolve<IAccountService> ().SignOut ();
				});
			}
			else if (ex is WebException && ((WebException)ex).Status == WebExceptionStatus.ConnectFailure)
			{
				_lastConnectError=DateTime.Now;
				var title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionTitle");
				var msg = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionMessage");
				var mService = TinyIoCContainer.Current.Resolve<IMessageService> ();
				mService.ShowMessage (title, msg);
			}
        }
    }
}

