using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class ViewModelServicesExtensions
    {
        public static ServicesExtensionPoint Services(this BaseViewModel viewModel)
        {
            return new ServicesExtensionPoint(viewModel);
        }
    }

    public class ServicesExtensionPoint
    {
        private readonly BaseViewModel _viewModel;

        public ServicesExtensionPoint(BaseViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public IConfigurationManager Config { get { return _viewModel.Container.Resolve<IConfigurationManager>(); } }

        public IMessageService Message { get { return _viewModel.Container.Resolve<IMessageService>(); } }

        public IAppResource Resources { get { return _viewModel.Container.Resolve<IAppResource>(); } }

        public ITinyMessengerHub MessengerHub { get { return _viewModel.Container.Resolve<ITinyMessengerHub>(); } }

        public IAppSettings Settings { get { return _viewModel.Container.Resolve<IAppSettings>(); } }

        public IPhoneService Phone { get { return _viewModel.Container.Resolve<IPhoneService>(); } }

        public AbstractLocationService Location { get { return _viewModel.Container.Resolve<AbstractLocationService>(); } }

        public IBookingService Booking { get { return _viewModel.Container.Resolve<IBookingService>(); } }

        public ICacheService Cache { get { return _viewModel.Container.Resolve<ICacheService>(); } }

        public IAppCacheService AppCache { get { return _viewModel.Container.Resolve<IAppCacheService>(); } }

        public IAppSettings AppSettings { get { return _viewModel.Container.Resolve<IAppSettings>(); } }

        public IApplicationInfoService ApplicationInfo { get { return _viewModel.Container.Resolve<IApplicationInfoService>(); } }

        public IGeolocService Geoloc { get { return _viewModel.Container.Resolve<IGeolocService>(); } }

        public IAccountService Account { get { return _viewModel.Container.Resolve<IAccountService>(); } }

        public IPaymentService Payment { get { return _viewModel.Container.Resolve<IPaymentService>(); } }
        //todo to remove
        public IVehicleClient VehicleClient { get { return _viewModel.Container.Resolve<IVehicleClient>(); } }

        public ITutorialService Tutorial { get { return _viewModel.Container.Resolve<ITutorialService>(); } }

        public ITermsAndConditionsService Terms { get { return _viewModel.Container.Resolve<ITermsAndConditionsService>(); } }

        public IPushNotificationService PushNotification { get { return _viewModel.Container.Resolve<IPushNotificationService>(); } }

        public IPackageInfo PackageInfo { get { return _viewModel.Container.Resolve<IPackageInfo>(); } }
    }
}