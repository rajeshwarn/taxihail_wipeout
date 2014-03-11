using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Configuration;
using MK.Common.Configuration;
using TinyMessenger;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class ViewModelServicesExtensions
    {
		public static ServicesExtensionPoint Services(this IMvxView view)
		{
			return new ServicesExtensionPoint();
		}

		public static ServicesExtensionPoint Services(this MvxNavigatingObject navigatingObject)
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

        public IMessageService Message { get { return _container.Resolve<IMessageService>(); } }

        public ILocalization Localize { get { return _container.Resolve<ILocalization>(); } }

        public ITinyMessengerHub MessengerHub { get { return _container.Resolve<ITinyMessengerHub>(); } }

		public TaxiHailSetting Settings { get { return _container.Resolve<IAppSettings>().Data; } }        

        public ICacheService Cache { get { return _container.Resolve<ICacheService>(); } }        

        public IApplicationInfoService ApplicationInfo { get { return _container.Resolve<IApplicationInfoService>(); } }        

        public IPackageInfo PackageInfo { get { return _container.Resolve<IPackageInfo>(); } }
    }
}