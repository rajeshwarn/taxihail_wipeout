using System;
using System.Net;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using Android.Content;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
	public class ErrorHandler : IErrorHandler
	{
		public const string ACTION_EUC = "Mk_Taxi.ERROR_UNAUTHORIZED_CALL_RAISED";

		public ErrorHandler ()
		{
		}

		public void HandleError( Exception ex )
		{
			if( ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.Unauthorized )
			{
				AppContext.Current.App.SendBroadcast( new Intent( ACTION_EUC ) );
				AppContext.Current.SignOut ();
			}
		}
	}
}

