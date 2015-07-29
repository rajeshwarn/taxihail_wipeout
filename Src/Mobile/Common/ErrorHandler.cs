using System;
using System.Net;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Diagnostic;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using ServiceStack.ServiceClient.Web;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile
{
    public class ErrorHandler : IErrorHandler
    {
        public static DateTime LastConnectError = DateTime.MinValue;
		public static DateTime LastUnauthorizedError = DateTime.MinValue;

        public bool HandleError (Exception ex)
        {
			var handled = false;
			if (ex is WebServiceException 
				&& ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.Unauthorized) 
            {
				if (LastUnauthorizedError.Subtract(DateTime.Now).TotalSeconds < -5)
				{
					LastUnauthorizedError = DateTime.Now;

					var localize = TinyIoCContainer.Current.Resolve<ILocalization>();
					TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage(
						localize["ServiceErrorCallTitle"], 
						localize["ServiceErrorUnauthorized"], 
						() => {					
							var dispatch = TinyIoCContainer.Current.Resolve<IMvxViewDispatcher> ();
							dispatch.ShowViewModel(new MvxViewModelRequest (typeof(LoginViewModel), null, null, MvxRequestedBy.UserAction));

							TinyIoCContainer.Current.Resolve<IOrderWorkflowService> ().PrepareForNewOrder ();
							TinyIoCContainer.Current.Resolve<IAccountService> ().SignOut ();
						});
				}

				handled = true;
			}
			else if (ex is WebException 
				&& (((WebException)ex).Status == WebExceptionStatus.ConnectFailure 
				 || ((WebException)ex).Status == WebExceptionStatus.NameResolutionFailure
                || ((WebException)ex).Status == WebExceptionStatus.SendFailure
                || ((WebException)ex).Status == WebExceptionStatus.ReceiveFailure
                || ((WebException)ex).Status == WebExceptionStatus.PipelineFailure
                || ((WebException)ex).Status == WebExceptionStatus.RequestCanceled
                || ((WebException)ex).Status == WebExceptionStatus.ConnectionClosed
                || ((WebException)ex).Status == WebExceptionStatus.SecureChannelFailure
                || ((WebException)ex).Status == WebExceptionStatus.KeepAliveFailure
                || ((WebException)ex).Status == WebExceptionStatus.Pending
                || ((WebException)ex).Status == WebExceptionStatus.Timeout
                || ((WebException)ex).Status == WebExceptionStatus.UnknownError
                || ((WebException)ex).Status == WebExceptionStatus.CacheEntryNotFound
                || ((WebException)ex).Status == WebExceptionStatus.TrustFailure
                ))
			{
				if(LastConnectError.Subtract(DateTime.Now).TotalSeconds < -5)
				{
					LastConnectError = DateTime.Now;

					var localize = TinyIoCContainer.Current.Resolve<ILocalization>();
					TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (
						localize["NoConnectionTitle"], 
						localize["NoConnectionMessage"]);
				}

				handled = true;
			}

			return handled;
        }
    }
}

