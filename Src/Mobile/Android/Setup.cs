using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Droid.Platform;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Activities.Account;
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
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Cirrious.CrossCore.Droid.Platform;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class Setup
		: MvxAndroidSetup
    {
		public Setup(Context applicationContext) : base(applicationContext)
        {
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

			TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(ApplicationContext));
			TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
			TinyIoCContainer.Current.Register<IMessageService>(new MessageService(ApplicationContext));
			TinyIoCContainer.Current.Register<IAnalyticsService>((c, x) => new GoogleAnalyticsService(Application.Context, c.Resolve<IPackageInfo>(), c.Resolve<IAppSettings>(), c.Resolve<ILogger>()));

			TinyIoCContainer.Current.Register<AbstractLocationService>(new LocationService());

            TinyIoCContainer.Current.Register<IAppSettings, AppSettings>();
            TinyIoCContainer.Current.Register<ILocalization>(new Localize(ApplicationContext));
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

		private void InitializeSocialNetwork()
		{
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            var facebookService = new FacebookService(settings.FacebookAppId, () => TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>().Activity);
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

            TinyIoCContainer.Current.Register<ITwitterService>((c,p) => new TwitterServiceMonoDroid( oauthConfig, c.Resolve<IMvxAndroidCurrentTopActivity>()));
		}

		protected override Cirrious.CrossCore.IoC.IMvxIoCProvider CreateIocProvider()
		{
			return new TinyIoCProvider(TinyIoCContainer.Current);
		}
    }
}