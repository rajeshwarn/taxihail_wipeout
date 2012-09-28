using System;
using System.Net;
using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceClient.Web;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;

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
                        var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
                        dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(LoginViewModel), null, true, MvxRequestedBy.UserAction));
					});
					AppContext.Current.SignOut ();
				});
			}
		}
	}
}

