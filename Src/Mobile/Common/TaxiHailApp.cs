using System.Collections.Generic;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Google;
using apcurium.MK.Booking.Google.Impl;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Provider;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;
using apcurium.MK.Booking.Mobile.Messages;

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
				new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, null, c.Resolve<IPackageInfo>().UserAgent, c.Resolve<IPaymentService>()),
                                                                     "NotAuthenticated");
            
			_container.Register<IAccountServiceClient>((c, p) =>
				new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent, c.Resolve<IPaymentService>()),
                                                                     "Authenticate");
            
			_container.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent,c.Resolve<IPaymentService>()));

			_container.Register((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
			_container.Register((c, p) => new PopularAddressesServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
			_container.Register((c, p) => new TariffsServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
			_container.Register((c, p) => new PushNotificationRegistrationServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

			_container.Register((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

			_container.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
            
			_container.Register((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

			_container.Register<IConfigurationManager>((c, p) => {
				return new ConfigurationClientService(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<ILogger>(), c.Resolve<IPackageInfo>().UserAgent);
			});

			_container.Register<IAccountService, AccountService>();
			_container.Register<IBookingService, BookingService>();
			_container.Register<IVehicleService, VehicleService>();
			_container.Register<ITutorialService, TutorialService>();
			_container.Register<ITermsAndConditionsService, TermsAndConditionsService>();

			_container.Register<IGeolocService, GeolocService>();
			_container.Register<IGoogleService, GoogleService>();
			_container.Register<IApplicationInfoService, ApplicationInfoService>();

			_container.Register<IPriceCalculator, PriceCalculator>();
			_container.Register<IAddresses, Addresses>();
			_container.Register<IDirections, Directions>();
			_container.Register<IGeocoding, Geocoding>();
			_container.Register<IPlaces, Places>();
			_container.Register<IMapsApiClient, MapsApiClient>();
			_container.Register<IPopularAddressProvider, PopularAddressProvider>();
			_container.Register<ITariffProvider, TariffProvider>();

            // ***** PayPal *****
			_container.Register<IPayPalExpressCheckoutService, PayPalExpressCheckoutService> ();
			_container.Register ((c, p) => new PayPalServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

			_container.Register<IPaymentService>((c, p) =>
			{
					var baseUrl = c.Resolve<IAppSettings>().Data.ServiceUrl;
                var sessionId = GetSessionId();

                return new PaymentService(baseUrl, sessionId, c.Resolve<IConfigurationManager>(), c.Resolve<ICacheService>(), c.Resolve<IPackageInfo>());
			});
            
			_container.Register<IVehicleClient>((c, p) => new VehicleServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
			_container.Register<IIbsFareClient>((c, p) => new IbsFareServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
			
			_container.Resolve<IMvxLifetime>().LifetimeChanged -= TaxiHailApp_LifetimeChanged;
			_container.Resolve<IMvxLifetime>().LifetimeChanged += TaxiHailApp_LifetimeChanged;

            RefreshAppData();
        }

        void TaxiHailApp_LifetimeChanged(object sender, MvxLifetimeEventArgs e)
        {
			if ( (e.LifetimeEvent == MvxLifetimeEvent.Deactivated) || (e.LifetimeEvent == MvxLifetimeEvent.Closing) )  {
                ClearAppCache ();
				_container.Resolve<AbstractLocationService>().Stop();
            } 
			else if ((e.LifetimeEvent == MvxLifetimeEvent.ActivatedFromDisk) || (e.LifetimeEvent == MvxLifetimeEvent.ActivatedFromMemory)|| (e.LifetimeEvent == MvxLifetimeEvent.Launching)) {
				_container.Resolve<AbstractLocationService>().Start();
				_container.Resolve<ITinyMessengerHub>().Publish(new AppActivated(this));
                RefreshAppData ();
            }
        }

        private void LoadAppCache()
        {
			_container.Resolve<IApplicationInfoService>().GetAppInfoAsync();
			_container.Resolve<IAccountService>().GetReferenceData();
			_container.Resolve<IConfigurationManager>().GetSettings();
        }

        private void ClearAppCache()
        {
			_container.Resolve<IApplicationInfoService>().ClearAppInfo();
			_container.Resolve<IAccountService>().ClearReferenceData();
			_container.Resolve<IConfigurationManager>().Reset();
        }

        private void RefreshAppData()
        {
			Task.Run(() =>
			{
				ClearAppCache();
				LoadAppCache();
			});
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





