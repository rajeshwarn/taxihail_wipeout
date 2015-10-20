using System;
using System.Net;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Diagnostic;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;
using Android.App;
using Android.Net;

namespace apcurium.MK.Callbox.Mobile.Client.Diagnostic
{
	public class ErrorHandler : IErrorHandler
	{
		public const string ACTION_SERVICE_ERROR = "Mk_Taxi.SERVICE_ERROR";
		public const string ACTION_EXTRA_ERROR = "Mk_Taxi.SERVICE_ERROR_Code";

        public static DateTime LastConnectError = DateTime.MinValue;


        public bool HandleError(Exception ex)
        {
			var localize = TinyIoCContainer.Current.Resolve<ILocalization>();

			var webServiceException = ex as WebServiceException;
			if (webServiceException != null)
            {
                
				var title = localize["ServiceErrorCallTitle"];
				var message = localize["ServiceErrorDefaultMessage"];

                try
                {

					message = localize["ServiceError" + webServiceException.ErrorCode];
                }
                catch(Exception exception)
                {
					Logger.LogError(exception);
                }
                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
            }
            else if (ex is WebException)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                if (LastConnectError.Subtract(DateTime.Now).TotalSeconds < -2)
                {
                    LastConnectError = DateTime.Now;
                    var cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
                    if ((cm == null) || (cm.ActiveNetworkInfo == null) || (!cm.ActiveNetworkInfo.IsConnectedOrConnecting))
                    {
						var title = localize["NetworkErrorTitle"];
						var message = localize["NetworkErrorMessage"];
                        TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
                    }
                }
            }

	        return true;
        }
	}
}

