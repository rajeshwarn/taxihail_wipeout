using System;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public class AppModule : IModule 
	{
		public AppModule ()
		{
		}

		#region IModule implementation
		public void Initialize ()
		{
			ServiceLocator.Current.Register<IAppResource, Resources>();
		}
		#endregion
	}
}

