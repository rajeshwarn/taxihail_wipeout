using TinyIoC;
using Xamarin.Contacts;
using apcurium.MK.Booking.Mobile.Practices;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Settings;
using Xamarin.Geolocation;
using apcurium.MK.Booking.Mobile.Data;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppModule : IModule
    {

        public AppModule(TaxiMobileApplication app)
        {

            App = app;
            app.PackageManager.GetPackageInfo(app.PackageName, 0);
        }

        public TaxiMobileApplication App { get; set; }
        public void Initialize()
        {
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(App));
            TinyIoCContainer.Current.Register<IAppSettings>( new AppSettings());
            TinyIoCContainer.Current.Register<IAppContext>(new AppContext(App));
            TinyIoCContainer.Current.Register<IAppResource>( new ResourceManager( App.ApplicationContext ));
            TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();
            

            TinyIoCContainer.Current.Register<ICacheService>(new CacheService(App));
            TinyIoCContainer.Current.Register<AddressBook>(new AddressBook(App.ApplicationContext));
            
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 100 });                        
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 10000 }, CoordinatePrecision.BallPark.ToString());
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 1000 }, CoordinatePrecision.Coarse.ToString());
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 400 }, CoordinatePrecision.Medium.ToString());
            
        
        }
    }
}