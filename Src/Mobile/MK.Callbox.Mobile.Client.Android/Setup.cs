using System;
using System.Collections.Generic;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.Mobile;
using Android.Content;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Callbox.Mobile.Client.Converters;
using apcurium.MK.Callbox.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore.Droid;
using ErrorHandler = apcurium.MK.Callbox.Mobile.Client.Diagnostic.ErrorHandler;

namespace apcurium.MK.Callbox.Mobile.Client
{
    public class Setup
        : MvxAndroidSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {

        }

		protected override void InitializeLastChance()
        {
			base.InitializeLastChance();

			var container = TinyIoCContainer.Current;

			container.Register<ICacheService>(new CacheService());
			container.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "UserAppCache");
			container.Register<IMessageService, MessageService>();
			container.Register<IPackageInfo, PackageInfo>();
			container.Register<ILogger>(new LoggerImpl());
			container.Register<IAppSettings>(new AppSettingsService(container.Resolve<ICacheService>(),container.Resolve<ILogger>()));
			container.Register<ILocalization>(new Localize(ApplicationContext, container.Resolve<ILogger>()));
			container.Register<IPhoneService, PhoneService>();
			container.Register<IAnalyticsService>((c, x) => new DummyAnalyticsService());
			container.Register<IGeocoder>((c, p) => new AndroidGeocoder(c.Resolve<IAppSettings>(), c.Resolve<ILogger>(), c.Resolve<IMvxAndroidGlobals>()));
			container.Register<IErrorHandler, ErrorHandler>();
        }

		protected override IMvxApplication CreateApp()
        {
			return new CallBoxApp();
        }
                
		protected override Cirrious.CrossCore.IoC.IMvxIoCProvider CreateIocProvider()
		{
			return new TinyIoCProvider(TinyIoCContainer.Current);
		}

		protected override List<Type> ValueConverterHolders
        {
			get { return new List<Type> { typeof(AppConverters) }; }
        }
    }
}
