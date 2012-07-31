using System;

using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyIoC;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public class ModuleService : IModule 
	{
		public ModuleService ()
		{
		}
		
		public void Initialize()
		{
            TinyIoCContainer.Current.Register<AccountServiceClient>((c , p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null), "NotAuthenticated");

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => 
                {

                    return new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId"));
                }, "Authenticate");

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<ReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));


            TinyIoCContainer.Current.Register<GeocodingServiceClient>((c, p) => new GeocodingServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            TinyIoCContainer.Current.Register<DirectionsServiceClient>((c, p) => new DirectionsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<OrderServiceClient>((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));

            TinyIoCContainer.Current.Register<AuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, c.Resolve<ICacheService>().Get<string>("SessionId")));
            

            TinyIoCContainer.Current.Register<IAccountService, AccountService>();
            TinyIoCContainer.Current.Register<IBookingService, BookingService>();
            TinyIoCContainer.Current.Register<IGeolocService, GeolocService >(); 
            //ServiceLocator.Current.Register<IAccountService, AccountService>();
            //ServiceLocator.Current.Register<IBookingService, BookingService>();
			
		}
		
	}
}

