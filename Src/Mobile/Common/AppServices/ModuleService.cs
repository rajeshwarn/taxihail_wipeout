using System;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;


namespace apcurium.MK.Booking.Mobile.AppServices
{
	public class ModuleService : IModule 
	{
		public ModuleService ()
		{
		}
		
		public void Initialize()
		{
            TinyIoC.TinyIoCContainer.Current.Register<IAccountService, AccountService>();
            TinyIoC.TinyIoCContainer.Current.Register<IBookingService, BookingService>(); 
            //ServiceLocator.Current.Register<IAccountService, AccountService>();
            //ServiceLocator.Current.Register<IBookingService, BookingService>();
			
		}
		
	}
}

