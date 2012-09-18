using System;
using System.Globalization;
using System.Net;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
	public class ErrorHandler : IErrorHandler
	{
		public const string ACTION_SERVICE_ERROR = "Mk_Taxi.SERVICE_ERROR";
		public const string ACTION_EXTRA_ERROR = "Mk_Taxi.SERVICE_ERROR_Code";
		
		public void HandleError( Exception ex )
		{
            var exception =  ex as WebServiceException;
			
			if(exception != null)
            {
                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServiceErrorCallTitle");
                var message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServiceErrorDefaultMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);
                
                try
                {

                    message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServiceError" + exception.ErrorCode);                    
                }
                catch
                {

                }
                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);



                //var errorIntent = new Intent(ACTION_SERVICE_ERROR);
                //errorIntent.PutExtra(ACTION_EXTRA_ERROR, exception.ErrorCode);

                //AppContext.Current.App.SendBroadcast(errorIntent);

                //if(exception.StatusCode == (int)HttpStatusCode.Unauthorized)
                //{
                //    AppContext.Current.SignOut();
                //}
            }
		}
	}
}

