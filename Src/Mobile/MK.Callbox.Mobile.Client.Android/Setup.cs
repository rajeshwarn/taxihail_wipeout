using System;
using System.Collections.Generic;
using Android.Content;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Callbox.Mobile.Client.Converters;
using apcurium.MK.Callbox.Mobile.Client.Diagnostic;
using apcurium.MK.Callbox.Mobile.Client.Localization;
using apcurium.MK.Callbox.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Cache;

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

			container.Register<IMessageService, MessageService>();
			container.Register<IPackageInfo, PackageInfo>();
			container.Register<IAppSettings>(new AppSettings());
			container.Register<ILocalization, ResourceManager>();
			container.Register<ILogger, LoggerImpl>();
			container.Register<IErrorHandler, ErrorHandler>();
			container.Register<IPhoneService, PhoneService>();
			container.Register<ICacheService>(new CacheService());

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
