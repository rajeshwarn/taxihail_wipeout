using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Mvx;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Binding.Android;
using Android.Content;
using TinyIoC;
using apcurium.MK.Booking.Mobile;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using apcurium.MK.Callbox.Mobile.Client.Cache;
using apcurium.MK.Callbox.Mobile.Client.Converters;
using apcurium.MK.Callbox.Mobile.Client.Diagnostic;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Callbox.Mobile.Client.PlatformIntegration;
using apcurium.MK.Callbox.Mobile.Client.Localization;

namespace apcurium.MK.Callbox.Mobile.Client
{
    public class Setup
        : MvxBaseAndroidBindingSetup
    {
        public Setup(Context applicationContext)
            : base(applicationContext)
        {


        }

        protected override void InitializeAdditionalPlatformServices()
        {
            base.InitializeAdditionalPlatformServices();


			TinyIoCContainer.Current.Register<IMessageService>(new MessageService(this.ApplicationContext));
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(this.ApplicationContext));
            TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
            TinyIoCContainer.Current.Register<IAppResource>(new ResourceManager(this.ApplicationContext));
            TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();
            TinyIoCContainer.Current.Register<IPhoneService>(new PhoneService(this.ApplicationContext));

            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());

        }

		protected override void FillTargetFactories (IMvxTargetBindingFactoryRegistry registry)
		{
			base.FillTargetFactories (registry);
			//registry.RegisterFactory(new MvxCustomBindingFactory<EditTextSpinner>("SelectedItem", spinner => new EditTextSpinnerSelectedItemBinding(spinner)));
		}

        protected override MvxApplication CreateApp()
        {
            var app = new CallBoxApp();
            return app;
        }
                
        protected override void InitializeIoC()
        {
            TinyIoCServiceProviderSetup.Initialize();
        }

        protected override IEnumerable<Type> ValueConverterHolders
        {
            get { return new[] { typeof(AppConverters) }; }
        }
    }
}
