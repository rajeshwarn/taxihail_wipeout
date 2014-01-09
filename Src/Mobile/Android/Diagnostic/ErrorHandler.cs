using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.Net;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Diagnostic
{
    public class ErrorHandler : IErrorHandler
    {
        public static DateTime LastConnectError = DateTime.MinValue;

        public void HandleError(Exception ex)
        {
            if (ex is WebServiceException)
            {
                var webServiceException = (WebServiceException) ex;
                var title = TinyIoCContainer.Current.Resolve<ILocalization>()["ServiceErrorCallTitle"];
                var message = TinyIoCContainer.Current.Resolve<ILocalization>()["ServiceErrorDefaultMessage"];

                try
                {
                    message = TinyIoCContainer.Current.Resolve<ILocalization>()["ServiceError" + webServiceException.ErrorCode];
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }
                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
            }
            else if (ex is WebException)
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                if (LastConnectError.Subtract(DateTime.Now).TotalSeconds < -2)
                {
                    LastConnectError = DateTime.Now;
                    var cm = (ConnectivityManager) Application.Context.GetSystemService(Context.ConnectivityService);
                    if ((cm == null) || (cm.ActiveNetworkInfo == null) ||
                        (!cm.ActiveNetworkInfo.IsConnectedOrConnecting))
                    {
                        var title = TinyIoCContainer.Current.Resolve<ILocalization>()["NetworkErrorTitle"];
                        var message = TinyIoCContainer.Current.Resolve<ILocalization>()["NetworkErrorMessage"];
                        TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, message);
                    }
                }
            }
        }
    }
}