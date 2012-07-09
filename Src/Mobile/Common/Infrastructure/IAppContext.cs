using System;

namespace MobileTaxiApp.Infrastructure
{
	public interface IAppContext
	{
				
		TaxiMobileApp.AccountData LoggedUser {get;}
	}
}

