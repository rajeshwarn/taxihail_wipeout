using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using TinyMessenger;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class ViewModelServicesExtensions
    {
		public static ServicesExtensionPoint Services(this IMvxViewModel viewModel)
        {
            return new ServicesExtensionPoint();
        }

		public static ServicesExtensionPoint Services(this IMvxView view)
		{
			return new ServicesExtensionPoint();
		}
    }

    public class ServicesExtensionPoint
    {
        public ServicesExtensionPoint()
        {
        }

		public IConfigurationManager Config { get { return Mvx.Resolve<IConfigurationManager>(); } }

		public IMessageService Message { get { return Mvx.Resolve<IMessageService>(); } }

		public ILocalization Localize { get { return Mvx.Resolve<ILocalization>(); } }

		public ITinyMessengerHub MessengerHub { get { return Mvx.Resolve<ITinyMessengerHub>(); } }

		public IAppSettings Settings { get { return Mvx.Resolve<IAppSettings>(); } }

		public IPhoneService Phone { get { return Mvx.Resolve<IPhoneService>(); } }

		public AbstractLocationService Location { get { return Mvx.Resolve<AbstractLocationService>(); } }

		public IBookingService Booking { get { return Mvx.Resolve<IBookingService>(); } }

		public ICacheService Cache { get { return Mvx.Resolve<ICacheService>(); } }

		public ICacheService AppCache { get { return Mvx.Resolve<ICacheService>("AppCache"); } }

		public IAppSettings AppSettings { get { return Mvx.Resolve<IAppSettings>(); } }

		public IApplicationInfoService ApplicationInfo { get { return Mvx.Resolve<IApplicationInfoService>(); } }

		public IGeolocService Geoloc { get { return Mvx.Resolve<IGeolocService>(); } }

		public IAccountService Account { get { return Mvx.Resolve<IAccountService>(); } }

		public IPaymentService Payment { get { return Mvx.Resolve<IPaymentService>(); } }

		public ITutorialService Tutorial { get { return Mvx.Resolve<ITutorialService>(); } }

		public ITermsAndConditionsService Terms { get { return Mvx.Resolve<ITermsAndConditionsService>(); } }

		public IPushNotificationService PushNotification { get { return Mvx.Resolve<IPushNotificationService>(); } }

		public IPackageInfo PackageInfo { get { return Mvx.Resolve<IPackageInfo>(); } }

		public IVehicleService Vehicle { get { return Mvx.Resolve<IVehicleService>(); } }
    }
}