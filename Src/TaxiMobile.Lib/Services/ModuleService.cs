using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services.Impl;

namespace TaxiMobile.Lib.Services
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

