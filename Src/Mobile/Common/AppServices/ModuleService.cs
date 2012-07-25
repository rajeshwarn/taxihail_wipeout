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
                    var auth = (AuthInfo)p["credential"];
                    return new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, auth );
                }, "Authenticate");

            TinyIoCContainer.Current.Register<AccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, new AuthInfo(c.Resolve<IAppContext>().LoggedInEmail, c.Resolve<IAppContext>().LoggedInPassword)));
            TinyIoCContainer.Current.Register<ReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null));


            TinyIoCContainer.Current.Register<GeocodingServiceClient>((c, p) => new GeocodingServiceClient(c.Resolve<IAppSettings>().ServiceUrl, new AuthInfo(c.Resolve<IAppContext>().LoggedInEmail, c.Resolve<IAppContext>().LoggedInPassword)));
            TinyIoCContainer.Current.Register<DirectionsServiceClient>((c, p) => new DirectionsServiceClient(c.Resolve<IAppSettings>().ServiceUrl, new AuthInfo(c.Resolve<IAppContext>().LoggedInEmail, c.Resolve<IAppContext>().LoggedInPassword)));

            TinyIoCContainer.Current.Register<OrderServiceClient>((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, new AuthInfo(c.Resolve<IAppContext>().LoggedInEmail, c.Resolve<IAppContext>().LoggedInPassword)));
            

            TinyIoCContainer.Current.Register<IAccountService, AccountService>();
            TinyIoCContainer.Current.Register<IBookingService, BookingService>();
            TinyIoCContainer.Current.Register<IGeolocService, GeolocService >(); 
            //ServiceLocator.Current.Register<IAccountService, AccountService>();
            //ServiceLocator.Current.Register<IBookingService, BookingService>();
			
		}
		
	}
}

