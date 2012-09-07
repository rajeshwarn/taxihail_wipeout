using System;
using System.Globalization;
using System.Net;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using Android.Content;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
	public class ErrorHandler : IErrorHandler
	{
		public const string ACTION_SERVICE_ERROR = "Mk_Taxi.SERVICE_ERROR";
		public const string ACTION_EXTRA_ERROR = "Mk_Taxi.SERVICE_ERROR_Code";
		
		public void HandleError( Exception ex )
		{
            var exception =  ex as WebServiceException;
			int erroCode;

			if(exception != null
                && int.TryParse(exception.ErrorCode, out erroCode))
            {
                var errorIntent = new Intent(ACTION_SERVICE_ERROR);
                errorIntent.PutExtra(ACTION_EXTRA_ERROR, erroCode.ToString(CultureInfo.InvariantCulture));

                AppContext.Current.App.SendBroadcast(errorIntent);

                if(exception.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    AppContext.Current.SignOut();
                }
            }
		}
	}
}

