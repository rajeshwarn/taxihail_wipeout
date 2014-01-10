using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Mvx;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Binding.Android;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Binding.Interfaces.Bindings.Target.Construction;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Helpers;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class Setup : MvxBaseAndroidBindingSetup
    {
		public Setup(Context applicationContext) : base(applicationContext)
        {
        }

        protected override void InitializeAdditionalPlatformServices()
        {
			base.InitializeAdditionalPlatformServices();

			TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(ApplicationContext));
			TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
			TinyIoCContainer.Current.Register<IMessageService>(new MessageService(ApplicationContext));
			TinyIoCContainer.Current.Register<IAnalyticsService>((c, x) => new GoogleAnalyticsService(Application.Context, c.Resolve<IPackageInfo>(), c.Resolve<IAppSettings>(), c.Resolve<ILogger>()));

			TinyIoCContainer.Current.Register<AbstractLocationService>(new LocationService());

			TinyIoCContainer.Current.Register<IAppSettings>(new AppSettings());
			TinyIoCContainer.Current.Register<IAppResource>(new ResourceManager(ApplicationContext));
			TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();
			TinyIoCContainer.Current.Register<ICacheService>(new CacheService());
			TinyIoCContainer.Current.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "AppCache");
			TinyIoCContainer.Current.Register<IPhoneService>(new PhoneService(ApplicationContext));
			TinyIoCContainer.Current.Register<IPushNotificationService>((c, p) => new PushNotificationService(ApplicationContext, c.Resolve<IConfigurationManager>()));

			InitializeSocialNetwork();
        }

        protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
			registry.RegisterFactory(new MvxCustomBindingFactory<EditTextSpinner>("SelectedItem", spinner => new EditTextSpinnerSelectedItemBinding(spinner)));
			registry.RegisterFactory(new MvxCustomBindingFactory<EditTextLeftImage>("CreditCardNumber", editTextLeftImage => new EditTextCreditCardNumberBinding(editTextLeftImage)));
			registry.RegisterFactory(new MvxCustomBindingFactory<ListViewCell2>("IsBottom", cell => new CellItemBinding(cell, CellItemBindingProperty.IsBottom)));
			registry.RegisterFactory(new MvxCustomBindingFactory<ListViewCell2>("IsTop", cell => new CellItemBinding(cell, CellItemBindingProperty.IsTop)));
            CustomBindingsLoader.Load(registry);
        }

		public void InitializeSocialNetwork()
		{
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

			var facebookService = new FacebookService(settings.FacebookAppId, () => LoginActivity.TopInstance);
			TinyIoCContainer.Current.Register<IFacebookService>(facebookService);

			var oauthConfig = new OAuthConfig
			{
				ConsumerKey = settings.TwitterConsumerKey,
				Callback = settings.TwitterCallback,
				ConsumerSecret = settings.TwitterConsumerSecret,
				RequestTokenUrl = settings.TwitterRequestTokenUrl,
				AccessTokenUrl = settings.TwitterAccessTokenUrl,
				AuthorizeUrl = settings.TwitterAuthorizeUrl
			};

			TinyIoCContainer.Current.Register<ITwitterService>((c,p) => new TwitterServiceMonoDroid( oauthConfig, LoginActivity.TopInstance ) );
		}

        protected override MvxApplication CreateApp()
        {
            var app = new TaxiHailApp(_params);
            return app;
        }

        protected override void InitializeIoC()
        {
            TinyIoCServiceProviderSetup.Initialize();
        }

        protected override IEnumerable<Type> ValueConverterHolders
        {
            get { return new[] {typeof (AppConverters)}; }
        }

        private IDictionary<string, string> _params;

        public void SetParams(IDictionary<string, string> @params)
        {
            _params = @params;
        }
    }
}