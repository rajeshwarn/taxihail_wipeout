﻿using System;
using System.Collections.Generic;
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
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore.Droid;
using ErrorHandler = apcurium.MK.Booking.Mobile.ErrorHandler;
using apcurium.MK.Callbox.Mobile.Client.Cache;
using apcurium.MK.Callbox.Mobile.Client.Localization;
using apcurium.MK.Common;
using apcurium.MK.Common.Services;
using apcurium.MK.Booking.MapDataProvider;
using PCLCrypto;

namespace apcurium.MK.Callbox.Mobile.Client
{
    public class Setup : MvxAndroidSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {

        }

		protected override void InitializeLastChance()
        {
			base.InitializeLastChance();

			var container = TinyIoCContainer.Current;

            container.Register<ILogger>(new LoggerImpl());
			container.Register<ICacheService>(new CacheService());
			container.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "UserAppCache");
			container.Register<IMessageService, MessageService>();
			container.Register<IPackageInfo, PackageInfo>();
            container.Register<IIPAddressManager, IPAddressManager>();
            container.Register<ISymmetricKeyAlgorithmProviderFactory>((c, x) => WinRTCrypto.SymmetricKeyAlgorithmProvider);
            container.Register<ICryptographicEngine>((c, x) => WinRTCrypto.CryptographicEngine);
            container.Register<IHashAlgorithmProviderFactory>((c, x) => WinRTCrypto.HashAlgorithmProvider);
            container.Register<IAppSettings>(new AppSettingsService(container.Resolve<ICacheService>(),container.Resolve<ILogger>(), container.Resolve<ICryptographyService>()));
			container.Register<ILocalization>(new Localize(ApplicationContext, container.Resolve<ILogger>()));
			container.Register<IPhoneService, PhoneService>();
			container.Register<IAnalyticsService>((c, x) => new DummyAnalyticsService());
			container.Register<IGeocoder>((c, p) => new AndroidGeocoder(c.Resolve<ILogger>(), c.Resolve<IMvxAndroidGlobals>()));
            container.Register<IConnectivityService, ConnectivityService>();
			container.Register<IErrorHandler, ErrorHandler>();
        }

		protected override IMvxApplication CreateApp()
        {
			return new CallBoxApp();
        }

        protected override Cirrious.MvvmCross.Droid.Views.IMvxAndroidViewPresenter CreateViewPresenter()
        {
            return new CallboxPresenter();
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
