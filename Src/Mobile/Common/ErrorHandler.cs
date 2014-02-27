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


        public void HandleError (Exception ex)
        {
			if (ex is WebServiceException && ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.Unauthorized) 
            {
				var mService = TinyIoCContainer.Current.Resolve<IMessageService> ();
				var localize = TinyIoCContainer.Current.Resolve<ILocalization>();
				mService.ShowMessage(localize["ServiceErrorCallTitle"], localize["ServiceErrorUnauthorized"], () =>
                {
					
					var dispatch = TinyIoCContainer.Current.Resolve<IMvxViewDispatcher> ();
					dispatch.ShowViewModel(new MvxViewModelRequest (typeof(LoginViewModel), null, null, MvxRequestedBy.UserAction));

					TinyIoCContainer.Current.Resolve<IOrderWorkflowService> ().PrepareForNewOrder ();
					TinyIoCContainer.Current.Resolve<IAccountService> ().SignOut ();
				});
			}
			else if (ex is WebException 
				&& (((WebException)ex).Status == WebExceptionStatus.ConnectFailure 
					|| ((WebException)ex).Status == WebExceptionStatus.NameResolutionFailure))
			{
				if(LastConnectError.Subtract(DateTime.Now).TotalSeconds < -5)
				{
					LastConnectError=DateTime.Now;
					var localize = TinyIoCContainer.Current.Resolve<ILocalization>();
					var title = localize["NoConnectionTitle"];
					var msg = localize["NoConnectionMessage"];
					var mService = TinyIoCContainer.Current.Resolve<IMessageService> ();
					mService.ShowMessage (title, msg);
				}
			}
        }
    }
}

