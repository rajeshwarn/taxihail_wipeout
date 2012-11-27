using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Localization;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using MK.Booking.Mobile.Infrastructure.Mvx;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.Maps.Impl;
using apcurium.MK.Booking.Google.Impl;
using apcurium.MK.Booking.Google;
using apcurium.MK.Common.Configuration;
using Cirrious.MvvmCross.Interfaces.Platform.Location;
using System;
using ServiceStack.Text;
using apcurium.MK.Common.Provider;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Api.Client.Cmt;

namespace apcurium.MK.Booking.Mobile
{
    public class TaxiHailApp  : MvxApplication
        , IMvxServiceProducer<IMvxStartNavigation>
    {    
      
        public TaxiHailApp ()
        {
            if (TinyIoCContainer.Current.Resolve<IAppSettings> ().IsCMT) 
            {
                RegisterServiceCmt();
            }else
            {
                RegisterServiceClients();
            }
            InitaliseServices();
            InitialiseStartNavigation();
        }
        
        private void InitaliseServices()
        {
            TinyIoCContainer.Current.Register<ITinyMessengerHub, TinyMessengerHub>();

            TinyIoCContainer.Current.Register<PopularAddressesServiceClient>((c, p) => new PopularAddressesServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));
            TinyIoCContainer.Current.Register<TariffsServiceClient>((c, p) => new TariffsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));

            TinyIoCContainer.Current.Register<OrderServiceClient>((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));


            
            TinyIoCContainer.Current.Register<ApplicationInfoServiceClient>((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));

            TinyIoCContainer.Current.Register<IConfigurationManager>((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));

            TinyIoCContainer.Current.Register<IAccountService, AccountService>();
            TinyIoCContainer.Current.Register<IBookingService, BookingService>();

            TinyIoCContainer.Current.Register<IGeolocService, GeolocService>();
            TinyIoCContainer.Current.Register<IGoogleService, GoogleService>();
            TinyIoCContainer.Current.Register<IApplicationInfoService, ApplicationInfoService>();

            TinyIoCContainer.Current.Register<IPriceCalculator, PriceCalculator>();
            TinyIoCContainer.Current.Register<IAddresses, Addresses>();
            TinyIoCContainer.Current.Register<IDirections, Directions>();
            TinyIoCContainer.Current.Register<IGeocoding, Geocoding>();
            TinyIoCContainer.Current.Register<IPlaces, Places>();
            TinyIoCContainer.Current.Register<IMapsApiClient, MapsApiClient>();
            TinyIoCContainer.Current.Register<IPopularAddressProvider, PopularAddressProvider>();
            TinyIoCContainer.Current.Register<ITariffProvider, TariffProvider>();           

        }


        private void RegisterServiceClients ()
        {
            TinyIoCContainer.Current.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));
            TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null), "NotAuthenticated");
            TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)), "Authenticate");            
            TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));
            TinyIoCContainer.Current.Register<IReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c)));
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

        private void RegisterServiceCmt ()
        {
            var locationService = TinyIoCContainer.Current.Resolve<ILocationService>();
            locationService.Initialize();
            TinyIoCContainer.Current.Register<IPreCogService>((c, p) => new PreCogService(locationService, new CmtPreCogServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetCredentialsCmt(c))));            
            TinyIoCContainer.Current.Register<IAuthServiceClient>((c, p) => new CmtAuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null));            
            TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new CmtAccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null), "NotAuthenticated");
            TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new CmtAccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetCredentialsCmt(c)), "Authenticate");            
            TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new CmtAccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetCredentialsCmt(c)));
            TinyIoCContainer.Current.Register<IReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null));
        }
        
        private CmtAuthCredentials GetCredentialsCmt (TinyIoCContainer container)
        {
            var authData = container.Resolve<ICacheService> ().Get<AuthenticationData> ("AuthenticationData");
            CmtAuthCredentials credentials = null;
            if (authData != null) {
                credentials = new CmtAuthCredentials();
                credentials.AccessToken = authData.AccessToken;
                credentials.AccessTokenSecret = authData.AccessTokenSecret;
                credentials.ConsumerKey = "AH7j9KweF235hP";
                credentials.ConsumerSecret= "K09JucBn23dDrehZa";
                credentials.SessionId = authData.SessionId;
            }
            return credentials;
        }

        private void InitialiseStartNavigation()
        {
            var startApplicationObject = new StartNavigation();
            this.RegisterServiceInstance<IMvxStartNavigation>(startApplicationObject);
        }

        

        protected override IMvxViewModelLocator CreateDefaultViewModelLocator()
        {
            return new TinyIocViewModelLocator(); //base.CreateDefaultViewModelLocator();
        }


        
    }

}





