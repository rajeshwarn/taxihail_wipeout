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

        public void HandleError(Exception ex)
        {


            if (ex is WebServiceException)
            {
                var webServiceException = (WebServiceException)ex;
                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServiceErrorCallTitle");
                var message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServiceErrorDefaultMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);

                try
                {

                    message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("ServiceError" + webServiceException.ErrorCode);
                }
                catch
                {

                }
                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
            }
            else if (ex is WebException)
            {
                var title = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("NetworkErrorTitle");
                var message = TinyIoCContainer.Current.Resolve<IAppResource>().GetString("NetworkErrorMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);
                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
            }


        }
	}
}

