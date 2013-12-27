using System.Linq;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile
{
	public class StartCallboxNavigation : MvxApplicationObject, IMvxStartNavigation
	{
		public void Start()
		{
			JsConfig.DateHandler = JsonDateHandler.ISO8601; //MKTAXI-849 it's here because cache service use servicetacks deserialization so it needs it to correctly deserezialised expiration date...

			TinyIoCContainer.Current.Resolve<IConfigurationManager>().Reset();

			var activeOrderStatusDetails = TinyIoCContainer.Current.Resolve<IAccountService>().GetActiveOrdersStatus();

			if (TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount == null)
			{
				RequestNavigate<CallboxLoginViewModel>();
			}
			else if (activeOrderStatusDetails != null && activeOrderStatusDetails.Any(c => TinyIoCContainer.Current.Resolve<IBookingService>().IsCallboxStatusActive(c.IbsStatusId)))
			{
				RequestNavigate<CallboxOrderListViewModel>();
			}
			else
			{
				RequestNavigate<CallboxCallTaxiViewModel>();
			}

			TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Startup with server {0}", TinyIoCContainer.Current.Resolve<IAppSettings>().ServiceUrl);
		}

		public bool ApplicationCanOpenBookmarks
		{
			get { return true; }
		}
	}
}