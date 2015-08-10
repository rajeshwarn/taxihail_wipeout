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
        public static DateTime LastGeneralError = DateTime.MinValue;
		public static DateTime LastUnauthorizedError = DateTime.MinValue;

		private static bool _isErrorMessageDisplayed;

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

							TinyIoCContainer.Current.Resolve<IOrderWorkflowService>().PrepareForNewOrder ();
							TinyIoCContainer.Current.Resolve<IAccountService>().SignOut ();
						});
				}

				handled = true;
			}
			else if ((ex is WebException 
				&& (((WebException)ex).Status == WebExceptionStatus.ConnectFailure 
					|| ((WebException)ex).Status == WebExceptionStatus.NameResolutionFailure))
				|| (ex is WebServiceException
					&& ((WebServiceException)ex).StatusCode == (int)HttpStatusCode.ServiceUnavailable))
			{
				if(LastConnectError.Subtract(DateTime.Now).TotalSeconds < -5)
				{
					LastConnectError = DateTime.Now;

					if (!_isErrorMessageDisplayed)
					{
						_isErrorMessageDisplayed = true;

						var localize = TinyIoCContainer.Current.Resolve<ILocalization>();
						TinyIoCContainer.Current.Resolve<IMessageService> ().ShowMessage (
							localize["NoConnectionTitle"], 
							localize["NoConnectionMessage"],
							() =>
							{
								_isErrorMessageDisplayed = false;
							});
					}

                    LogError(ex);
				}

				handled = true;
			}
			else
            {
                // Handle all other generic exceptions

                if (LastGeneralError.Subtract(DateTime.Now).TotalSeconds < -5)
                {
                    LastGeneralError = DateTime.Now;

                    LogError(ex);
                }
            }

			return handled;
        }

        private void LogError(Exception ex)
        {
            try
            {
                TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
            }
            catch
            {
                // Do nothing
            }
        }
    }
}

