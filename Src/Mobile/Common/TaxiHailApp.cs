using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Localization;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using MK.Booking.Mobile.Infrastructure.Mvx;
using TinyIoC;
using TinyMessenger;
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

namespace apcurium.MK.Booking.Mobile
{
    public class TaxiHailApp  : MvxApplication
        , IMvxServiceProducer<IMvxStartNavigation>
    {    
      
        public TaxiHailApp()
        {
            InitaliseServices();
            InitialiseStartNavigation();
        }
        
        private void InitaliseServices()
        {
            TinyIoCContainer.Current.Register<ITinyMessengerHub, TinyMessengerHub>();


            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null), "NotAuthenticated");
            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) =>
            {

                return new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId"));
            }, "Authenticate");

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<ReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<PopularAddressesServiceClient>((c, p) => new PopularAddressesServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<TariffsServiceClient>((c, p) => new TariffsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<OrderServiceClient>((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<AuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            
            TinyIoCContainer.Current.Register<ApplicationInfoServiceClient>((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<IConfigurationManager>((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

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





