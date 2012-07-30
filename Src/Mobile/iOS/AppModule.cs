using System;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class AppModule : IModule 
	{
		public AppModule ()
		{
		}

		#region IModule implementation
		public void Initialize ()
		{
            TinyIoCContainer.Current.Register<IAppSettings>( new AppSettings() );
            TinyIoCContainer.Current.Register<IAppContext>(AppContext.Current);

            TinyIoCContainer.Current.Register<IAppResource, Resources>();
            TinyIoCContainer.Current.Register<ILogger, LoggerWrapper>();
//
            TinyIoCContainer.Current.Register<ICacheService>(new CacheService());

            //			ServiceLocator.Current.RegisterSingleInstance2<IAppContext> (this);
//            ServiceLocator.Current.RegisterSingleInstance2<IAppSettings> (new AppSettings ());
//            ServiceLocator.Current.RegisterSingleInstance2<ILogger> (new LoggerWrapper ());
		}
		#endregion
	}
}

