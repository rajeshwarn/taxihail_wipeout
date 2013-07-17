using System;
using System.Globalization;
using System.Net;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using Android.App;
using Android.Net;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
	public class ErrorHandler : IErrorHandler
	{
		public const string ACTION_SERVICE_ERROR = "Mk_Taxi.SERVICE_ERROR";
		public const string ACTION_EXTRA_ERROR = "Mk_Taxi.SERVICE_ERROR_Code";

        public static DateTime _lastConnectError = DateTime.MinValue;

        public void HandleError(Exception ex)
        {
			if (ex is WebServiceException) {
				var webServiceException = (WebServiceException)ex;

				var title = string.Empty;
				var message = string.Empty;
				if (webServiceException.StatusCode == 404) {
					title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionTitle");
					message = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NoConnectionMessage");
				} else {
					title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("ServiceErrorCallTitle");
					message = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("ServiceErrorDefaultMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);;
				}

				try {
					message = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("ServiceError" + webServiceException.ErrorCode);
				} catch {

				}
				TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (title, message);
			} else if (ex is WebException) {
				TinyIoCContainer.Current.Resolve<ILogger> ().LogError (ex);
				if (_lastConnectError.Subtract (DateTime.Now).TotalSeconds < -2) {
					_lastConnectError = DateTime.Now;
					var cm = (ConnectivityManager)Application.Context.GetSystemService (Context.ConnectivityService);
					if ((cm == null) || (cm.ActiveNetworkInfo == null) || (!cm.ActiveNetworkInfo.IsConnectedOrConnecting)) {
						var title = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NetworkErrorTitle");
						var message = TinyIoCContainer.Current.Resolve<IAppResource> ().GetString ("NetworkErrorMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);
						TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (title, message);
					}
				}
			} else {
				throw ex;
			}
        }
	}
}

