using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using TinyMessenger;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;
using TinyIoC;

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
        readonly TinyIoCContainer _container;
        public ServicesExtensionPoint()
        {
            _container = TinyIoCContainer.Current;
        }

        public IConfigurationManager Config { get { return _container.Resolve<IConfigurationManager>(); } }

        public IMessageService Message { get { return _container.Resolve<IMessageService>(); } }

        public ILocalization Localize { get { return _container.Resolve<ILocalization>(); } }

        public ITinyMessengerHub MessengerHub { get { return _container.Resolve<ITinyMessengerHub>(); } }

        public IAppSettings Settings { get { return _container.Resolve<IAppSettings>(); } }

        public IPhoneService Phone { get { return _container.Resolve<IPhoneService>(); } }

        public AbstractLocationService Location { get { return _container.Resolve<AbstractLocationService>(); } }

        public IBookingService Booking { get { return _container.Resolve<IBookingService>(); } }

        public ICacheService Cache { get { return _container.Resolve<ICacheService>(); } }

        public ICacheService AppCache { get { return _container.Resolve<ICacheService>("AppCache"); } }

        public IApplicationInfoService ApplicationInfo { get { return _container.Resolve<IApplicationInfoService>(); } }

        public IGeolocService Geoloc { get { return _container.Resolve<IGeolocService>(); } }

        public IAccountService Account { get { return _container.Resolve<IAccountService>(); } }

        public IPaymentService Payment { get { return _container.Resolve<IPaymentService>(); } }

        public ITutorialService Tutorial { get { return _container.Resolve<ITutorialService>(); } }

        public ITermsAndConditionsService Terms { get { return _container.Resolve<ITermsAndConditionsService>(); } }

        public IPushNotificationService PushNotification { get { return _container.Resolve<IPushNotificationService>(); } }

        public IPackageInfo PackageInfo { get { return _container.Resolve<IPackageInfo>(); } }

        public IVehicleService Vehicle { get { return _container.Resolve<IVehicleService>(); } }
    }
}