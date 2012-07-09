using TaxiMobile.Lib.Infrastructure;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Localization;


namespace TaxiMobile
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

