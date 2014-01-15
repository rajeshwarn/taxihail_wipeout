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
      
		public TaxiHailApp()
		{
            InitalizeServices();
            InitializeStartNavigation();
        }
        
        private void InitalizeServices()
        {
            var container = TinyIoCContainer.Current;
            container.Register<ITinyMessengerHub, TinyMessengerHub>();

            container.Register<IAccountServiceClient>((c, p) => 
                                                      new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null, c.Resolve<IPackageInfo>().UserAgent, c.Resolve<IPaymentService>()),
                                                                     "NotAuthenticated");
            
            container.Register<IAccountServiceClient>((c, p) =>
                                                      new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent, c.Resolve<IPaymentService>()),
                                                                     "Authenticate");
            
            container.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent,c.Resolve<IPaymentService>()));

            container.Register((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
            container.Register((c, p) => new PopularAddressesServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
            container.Register((c, p) => new TariffsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
            container.Register((c, p) => new PushNotificationRegistrationServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            container.Register((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            container.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
            
            container.Register((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            container.Register<IConfigurationManager>((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<ILogger>(), c.Resolve<IPackageInfo>().UserAgent));

            container.Register<IAccountService, AccountService>();
            container.Register<IBookingService, BookingService>();
			container.Register<IVehicleService, VehicleService>();
            container.Register<ITutorialService, TutorialService>();
            container.Register<ITermsAndConditionsService, TermsAndConditionsService>();

            container.Register<IGeolocService, GeolocService>();
            container.Register<IGoogleService, GoogleService>();
            container.Register<IApplicationInfoService, ApplicationInfoService>();

            container.Register<IPriceCalculator, PriceCalculator>();
            container.Register<IAddresses, Addresses>();
            container.Register<IDirections, Directions>();
            container.Register<IGeocoding, Geocoding>();
            container.Register<IPlaces, Places>();
            container.Register<IMapsApiClient, MapsApiClient>();
            container.Register<IPopularAddressProvider, PopularAddressProvider>();
            container.Register<ITariffProvider, TariffProvider>();

            // ***** PayPal *****
            container.Register<IPayPalExpressCheckoutService, PayPalExpressCheckoutService> ();
            container.Register ((c, p) => new PayPalServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            container.Register<IPaymentService>((c, p) =>
			{
				var baseUrl = c.Resolve<IAppSettings>().ServiceUrl;
				var sessionId = GetSessionId(c);

                return new PaymentService(baseUrl, sessionId, c.Resolve<IConfigurationManager>(), c.Resolve<ICacheService>());
			});
            
            container.Register<IVehicleClient>((c, p) => new VehicleServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
            container.Register<IIbsFareClient>((c, p) => new IbsFareServiceClient(c.Resolve<IAppSettings>().ServiceUrl, GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
			
			container.Resolve<IMvxLifetime>().LifetimeChanged -= TaxiHailApp_LifetimeChanged;
			container.Resolve<IMvxLifetime>().LifetimeChanged += TaxiHailApp_LifetimeChanged;

            RefreshAppData();
        }

        void TaxiHailApp_LifetimeChanged(object sender, MvxLifetimeEventArgs e)
        {
            if ( (e.LifetimeEvent == MvxLifetimeEvent.Deactivated) || (e.LifetimeEvent == MvxLifetimeEvent.Closing) )  {
                ClearAppCache ();
                TinyIoCContainer.Current.Resolve<AbstractLocationService>().Stop();
            } 
			else if ((e.LifetimeEvent == MvxLifetimeEvent.ActivatedFromDisk) || (e.LifetimeEvent == MvxLifetimeEvent.ActivatedFromMemory)|| (e.LifetimeEvent == MvxLifetimeEvent.Launching)) {
                TinyIoCContainer.Current.Resolve<AbstractLocationService>().Start();
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new AppActivated(this));
                RefreshAppData ();
            }
        }

        private void LoadAppCache()
        {
            TinyIoCContainer.Current.Resolve<IApplicationInfoService>().GetAppInfoAsync();
            TinyIoCContainer.Current.Resolve<IAccountService>().GetReferenceDataAsync();
            TinyIoCContainer.Current.Resolve<IConfigurationManager>().GetSettings();
        }

        private static void ClearAppCache()
        {
            TinyIoCContainer.Current.Resolve<IApplicationInfoService>().ClearAppInfo();
            TinyIoCContainer.Current.Resolve<IAccountService>().ClearReferenceData();
            TinyIoCContainer.Current.Resolve<IConfigurationManager>().Reset();
        }

        private void RefreshAppData()
        {
			Task.Run(() =>
			{
				ClearAppCache();
				LoadAppCache();
			});
        }
        
        private string GetSessionId (TinyIoCContainer container)
        {
            var authData = container.Resolve<ICacheService> ().Get<AuthenticationData> ("AuthenticationData");
            var sessionId = authData == null ? null : authData.SessionId;
            if (sessionId == null) {
                sessionId = container.Resolve<ICacheService> ().Get<string>("SessionId");
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





