using System;
using System.Net;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class ErrorHandler : IErrorHandler
	{
		public ErrorHandler ()
		{
		}

		public void HandleError( Exception ex )
		{
			if( ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.Unauthorized )
			{
				MessageHelper.Show( Resources.UnAuthorizedCallTitle, Resources.UnAuthorizedCallMessage, () => {
					AppContext.Current.Controller.InvokeOnMainThread( () => {
						AppContext.Current.Controller.PresentModalViewController( new LoginView(), true );
					});
					AppContext.Current.SignOut ();
				});
			}
		}
	}
}

