using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Dialog.Droid;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.IoC;
using Cirrious.CrossCore.Droid.Platform;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class Setup
        : MvxAndroidDialogSetup
    {
        readonly TinyIoCContainer _container;

		public Setup(Context applicationContext) : base(applicationContext)
        {
            _container = TinyIoCContainer.Current;
        }

		protected override IMvxApplication CreateApp()
		{
			return new TaxiHailApp();
		}

		protected override List<Type> ValueConverterHolders
		{
			get
			{
				return new List<Type> { typeof(AppConverters) };
			}
		}

		protected override void InitializeLastChance()
        {
			base.InitializeLastChance();

            _container.Register<IPackageInfo>(new PackageInfo(ApplicationContext));
            _container.Register<ILogger, LoggerImpl>();
            _container.Register<IMessageService>(new MessageService(ApplicationContext));
            _container.Register<IAnalyticsService>((c, x) => new GoogleAnalyticsService(Application.Context, c.Resolve<IPackageInfo>(), c.Resolve<IAppSettings>(), c.Resolve<ILogger>()));

            _container.Register<AbstractLocationService>(new LocationService());

            _container.Register<IAppSettings, AppSettings>();
			_container.Register<ILocalization>(new Localize(ApplicationContext,_container.Resolve<ILogger>()));
            _container.Register<IErrorHandler, ErrorHandler>();
            _container.Register<ICacheService>(new CacheService());
            _container.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "AppCache");
            _container.Register<IPhoneService>(new PhoneService(ApplicationContext));
            _container.Register<IPushNotificationService>((c, p) => new PushNotificationService(ApplicationContext, c.Resolve<IConfigurationManager>()));

			InitializeSocialNetwork();
        }

		protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            registry.RegisterFactory(new MvxCustomBindingFactory<EditTextRightSpinner>("SelectedItem", spinner => new EditTextRightSpinnerSelectedItemBinding(spinner)));
            registry.RegisterFactory(new MvxCustomBindingFactory<EditTextSpinner>("SelectedItem", spinner => new EditTextSpinnerSelectedItemBinding(spinner)));
			registry.RegisterFactory(new MvxCustomBindingFactory<EditTextLeftImage>("CreditCardNumber", editTextLeftImage => new EditTextCreditCardNumberBinding(editTextLeftImage)));
			registry.RegisterFactory(new MvxCustomBindingFactory<ListViewCell>("IsBottom", cell => new CellItemBinding(cell, CellItemBindingProperty.IsBottom)));
			registry.RegisterFactory(new MvxCustomBindingFactory<ListViewCell>("IsTop", cell => new CellItemBinding(cell, CellItemBindingProperty.IsTop)));
            CustomBindingsLoader.Load(registry);
        }

		private void InitializeSocialNetwork()
		{
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            var facebookService = new FacebookService(settings.FacebookAppId, () => _container.Resolve<IMvxAndroidCurrentTopActivity>().Activity);
            _container.Register<IFacebookService>(facebookService);

			var oauthConfig = new OAuthConfig
			{
				ConsumerKey = settings.TwitterConsumerKey,
				Callback = settings.TwitterCallback,
				ConsumerSecret = settings.TwitterConsumerSecret,
				RequestTokenUrl = settings.TwitterRequestTokenUrl,
				AccessTokenUrl = settings.TwitterAccessTokenUrl,
				AuthorizeUrl = settings.TwitterAuthorizeUrl
			};

            _container.Register<ITwitterService>((c,p) => new TwitterServiceMonoDroid( oauthConfig, c.Resolve<IMvxAndroidCurrentTopActivity>()));
		}

		protected override Cirrious.CrossCore.IoC.IMvxIoCProvider CreateIocProvider()
		{
			return new TinyIoCProvider(TinyIoCContainer.Current);
		}
    }
}