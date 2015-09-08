using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Payments.PayPal;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Security;

using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Provider;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.CraftyClicks;
using apcurium.MK.Booking.Mobile.AppServices.Social;


namespace apcurium.MK.Booking.Mobile
{
    public class TaxiHailApp  : MvxApplication
    {
		readonly TinyIoCContainer _container;
    
		public TaxiHailApp()
		{
			_container = TinyIoCContainer.Current;

            InitalizeServices();
            InitializeStartNavigation();
        }
        
        private void InitalizeServices()
        {
			_container.Register<ITinyMessengerHub, TinyMessengerHub>();

			_container.Register<IAccountServiceClient>((c, p) => 
				new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, null, c.Resolve<IPackageInfo>(), c.Resolve<IPaymentService>()),
                                                                     "NotAuthenticated");
            
			_container.Register<IAccountServiceClient>((c, p) =>
				new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<IPaymentService>()),
                                                                     "Authenticate");
            
			_container.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>(),c.Resolve<IPaymentService>()));

			_container.Register((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
			_container.Register((c, p) => new PopularAddressesServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
            _container.Register((c, p) => new POIServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
			_container.Register((c, p) => new TariffsServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
			_container.Register((c, p) => new PushNotificationRegistrationServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

			_container.Register((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

            _container.Register((c, p) => new CompanyServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<ICacheService>()));
            _container.Register((c, p) => new ManualPairingForRideLinqServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

			_container.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
            
			_container.Register((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

			_container.Register((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<ILogger>()));

            _container.Register((c, p) => new NetworkRoamingServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

			_container.Register<IAccountService, AccountService>();
            _container.Register<IMetricsService, MetricsService>();
			_container.Register<IBookingService, BookingService>();
            _container.Register<INetworkRoamingService, NetworkRoamingService>();
			_container.Register<IOrderWorkflowService, OrderWorkflowService>();
            _container.Register<IPromotionService, PromotionService>();
			_container.Register<IVehicleService, VehicleService>();
			_container.Register<ITutorialService, TutorialService>();
			_container.Register<ITermsAndConditionsService, TermsAndConditionsService>();
			_container.Register<IAccountPaymentService, AccountPaymentService>();
			_container.Register<IRegisterWorkflowService,RegisterWorkflowService> ();

			_container.Register<IGeolocService, GeolocService>();
			_container.Register<IGoogleService, GoogleService>();
			_container.Register<IApplicationInfoService, ApplicationInfoService>();

			_container.Register<IPriceCalculator, PriceCalculator>();
			_container.Register<IAddresses, Addresses>();
			_container.Register<IDirections, Directions>();
			_container.Register<IGeocoding, Geocoding>();
			_container.Register<IPlaces, Places>();
			_container.Register<IPopularAddressProvider, PopularAddressProvider>();
            _container.Register<IPOIProvider, POIProvider>();
			_container.Register<ITariffProvider, TariffProvider>();

            _container.Register<IPostalCodeService, CraftyClicksService>();


            // ***** PayPal *****
			_container.Register ((c, p) => new PayPalServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

			_container.Register<IPaymentService>((c, p) =>
			{
				var baseUrl = c.Resolve<IAppSettings>().Data.ServiceUrl;
                var sessionId = GetSessionId();

                return new PaymentService(baseUrl, sessionId, c.Resolve<ConfigurationClientService>(), c.Resolve<ICacheService>(), c.Resolve<IPackageInfo>(), c.Resolve<ILogger>());
			});
            
			_container.Register<IVehicleClient>((c, p) => new VehicleServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<ILogger>()));
			_container.Register<IIbsFareClient>((c, p) => new IbsFareServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
			
			var lifeTimeMonitor = _container.Resolve<IMvxLifetime> ();
			lifeTimeMonitor.LifetimeChanged -= TaxiHailApp_LifetimeChanged;
			lifeTimeMonitor.LifetimeChanged += TaxiHailApp_LifetimeChanged;

			_container.Register<IErrorHandler, ErrorHandler>();
			_container.Register<IOrientationService, OrientationService>();

            RefreshAppData();
        }

        void TaxiHailApp_LifetimeChanged(object sender, MvxLifetimeEventArgs e)
        {
            switch (e.LifetimeEvent)
            {
                case MvxLifetimeEvent.Closing:
                case MvxLifetimeEvent.Deactivated:
                    ClearAppCache ();
                    break;
				case MvxLifetimeEvent.Launching:
				case MvxLifetimeEvent.ActivatedFromMemory:
				case MvxLifetimeEvent.ActivatedFromDisk:
					RefreshAppData();
#if !MONOTOUCH
					TryFacebookInitAndPublish();
#endif
                    break;
            }
        }

        private void LoadAppCache()
        {
			_container.Resolve<IApplicationInfoService>().GetAppInfoAsync();
			_container.Resolve<IAccountService>().GetReferenceData();
        }

        private void ClearAppCache()
        {
            _container.Resolve<IApplicationInfoService>().ClearAppInfo();
            var accountService = _container.Resolve<IAccountService>();
            accountService.ClearReferenceData();
            accountService.ClearVehicleTypesCache();
        }

        private void RefreshAppData()
        {
			Task.Run(() =>
			{
				ClearAppCache();
				LoadAppCache();
			});
        }

		private void TryFacebookInitAndPublish()
		{
		    var appSettings = Mvx.Resolve<IAppSettings>();
		    var facebookService = Mvx.Resolve<IFacebookService>();

            if (appSettings.Data.FacebookEnabled)
			{
                facebookService.Init();
			}

            if (appSettings.Data.FacebookPublishEnabled) 
			{
                facebookService.PublishInstall();
			}
		}
        
        private string GetSessionId ()
        {
            var authData = _container.Resolve<ICacheService> ().Get<AuthenticationData> ("AuthenticationData");
            var sessionId = authData == null ? null : authData.SessionId;
            if (sessionId == null) {
                sessionId = _container.Resolve<ICacheService> ().Get<string>("SessionId");
            }
            return sessionId;
        }
        
        private void InitializeStartNavigation()
        {
			Mvx.RegisterSingleton<IMvxAppStart>(new StartNavigation());
        }

		protected override IMvxViewModelLocator CreateDefaultViewModelLocator()
		{
			return new ViewModelLocator(Mvx.Resolve<IAnalyticsService>());
		}
    }
}





