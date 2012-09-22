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

            //new ModuleService().Initialize();

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null), "NotAuthenticated");

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) =>
            {

                return new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId"));
            }, "Authenticate");

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<ReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));


            TinyIoCContainer.Current.Register<SearchLocationsServiceClient>((c, p) => new SearchLocationsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<GeocodingServiceClient>((c, p) => new GeocodingServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<DirectionsServiceClient>((c, p) => new DirectionsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<OrderServiceClient>((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<PlaceDetailServiceClient>((c, p) => new PlaceDetailServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));


            TinyIoCContainer.Current.Register<AuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<NearbyPlacesClient>((c, p) => new NearbyPlacesClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<ApplicationInfoServiceClient>((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<IAccountService, AccountService>();
            TinyIoCContainer.Current.Register<IBookingService, BookingService>();
            TinyIoCContainer.Current.Register<IUserPositionService, UserPositionService>();
            TinyIoCContainer.Current.Register<IGeolocService, GeolocService>();
            TinyIoCContainer.Current.Register<IGoogleService, GoogleService>();
            TinyIoCContainer.Current.Register<IApplicationInfoService, ApplicationInfoService>(); 


            TinyIoCContainer.Current.Resolve<IUserPositionService>().Refresh();
        }
        
        private void InitialiseStartNavigation()
        {
            var startApplicationObject = new StartNavigation();
            this.RegisterServiceInstance<IMvxStartNavigation>(startApplicationObject);
        }

        protected override IMvxViewModelLocator GetDefaultLocator()
        {
            return new TinyIocViewModelLocator();
        }

        
    }

}





