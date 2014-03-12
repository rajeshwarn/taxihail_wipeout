using System.Linq;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.MvvmCross.ViewModels;
using ServiceStack.Text;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile
{
	public class StartCallboxNavigation:
			MvxNavigatingObject,
			IMvxAppStart
	{
		public void Start(object hint)
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601; //MKTAXI-849 it's here because cache service use servicetacks deserialization so it needs it to correctly deserezialised expiration date...

			var activeOrderStatusDetails = TinyIoCContainer.Current.Resolve<IAccountService>().GetActiveOrdersStatus();

			if (TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount == null)
			{
				ShowViewModel<CallboxLoginViewModel>();
			}
			else if (activeOrderStatusDetails != null && activeOrderStatusDetails.Any(c => TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IBSStatusId)))
			{
				ShowViewModel<CallboxOrderListViewModel>();
			}
			else
			{
				ShowViewModel<CallboxCallTaxiViewModel>();
			}

            TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().Data.ServiceUrl);
		}

		public bool ApplicationCanOpenBookmarks
		{
			get { return true; }
		}
	}
}