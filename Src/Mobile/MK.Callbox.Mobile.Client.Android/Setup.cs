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

			TinyIoCContainer.Current.Register<IMessageService>(new MessageService(this.ApplicationContext));
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(this.ApplicationContext));
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
			TinyIoCContainer.Current.Register<ILocalization>(new ResourceManager(this.ApplicationContext));
            TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();
            TinyIoCContainer.Current.Register<IPhoneService>(new PhoneService(this.ApplicationContext));
            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());

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
