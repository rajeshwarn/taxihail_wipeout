using System;
using System.Net;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Common.Diagnostic;
using Android.Content;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using TinyIoC;
using Android.App;
using Android.Net;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Callbox.Mobile.Client.Diagnostic
{
	public class ErrorHandler : IErrorHandler
	{
		private readonly ILocalization _localize;
		private readonly ILogger _logger;
		private readonly IMessageService _messageService;
		public const string ACTION_SERVICE_ERROR = "Mk_Taxi.SERVICE_ERROR";
		public const string ACTION_EXTRA_ERROR = "Mk_Taxi.SERVICE_ERROR_Code";

        public static DateTime LastConnectError = DateTime.MinValue;

		public ErrorHandler(ILocalization localize, ILogger logger, IMessageService messageService)
		{
			_localize = localize;
			_logger = logger;
			_messageService = messageService;
		}


		public bool HandleError(Exception ex)
        {
			var webServiceException = ex as WebServiceException;
			if (webServiceException != null)
            {
                
				var title = _localize["ServiceErrorCallTitle"];
				var message = _localize["ServiceErrorDefaultMessage"];

                try
                {

					message = _localize["ServiceError" + webServiceException.ErrorCode];
                }
                catch(Exception exception)
                {
					Logger.LogError(exception);
                }

				_messageService.ShowMessage(title, message, () =>
				{
					var dispatch = Mvx.Resolve<IMvxViewDispatcher>();
					dispatch.ShowViewModel(new MvxViewModelRequest(typeof(CallboxLoginViewModel), null, null, MvxRequestedBy.UserAction));

					Mvx.Resolve<IOrderWorkflowService>().PrepareForNewOrder();
					Mvx.Resolve<IAccountService>().SignOut();
				});
            }
            else if (ex is WebException)
            {
				_logger.LogError(ex);
                if (LastConnectError.Subtract(DateTime.Now).TotalSeconds < -2)
                {
                    LastConnectError = DateTime.Now;
                    var cm = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
                    if ((cm == null) || (cm.ActiveNetworkInfo == null) || (!cm.ActiveNetworkInfo.IsConnectedOrConnecting))
                    {
						var title = _localize["NetworkErrorTitle"];
						var message = _localize["NetworkErrorMessage"];
						_messageService.ShowMessage(title, message);
                    }
                }
            }

	        return true;
        }
	}
}

