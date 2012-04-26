using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public class ModuleService : IModule 
	{
		public ModuleService ()
		{
		}
		
		public void Initialize()
		{						
			ServiceLocator.Current.Register<IAccountService, AccountService>();
			ServiceLocator.Current.Register<IBookingService, BookingService>();
			
		}
		
	}
}

