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
using SocialNetworks.Services.MonoDroid;
using Android.App;
using SocialNetworks.Services;
using SocialNetworks.Services.OAuth;


namespace apcurium.MK.Booking.Mobile.Client
{
    public class AppModule
    {

        
        public AppModule()
        {
            
        }

        public TaxiMobileApplication App { get; set; }
        public void Initialize(TaxiMobileApplication app)
        {
            App = app;
            app.PackageManager.GetPackageInfo(app.PackageName, 0);

            
            TinyIoCContainer.Current.Register<IMessageService>(new MessageService(App));
            TinyIoCContainer.Current.Register<IPackageInfo>(new PackageInfo(App));
            TinyIoCContainer.Current.Register<IAppSettings>( new AppSettings());
            TinyIoCContainer.Current.Register<IAppContext>(new AppContext(App));
            TinyIoCContainer.Current.Register<IAppResource>( new ResourceManager( App.ApplicationContext ));
            TinyIoCContainer.Current.Register<ILogger, LoggerImpl>();
            TinyIoCContainer.Current.Register<IErrorHandler, ErrorHandler>();
            

            TinyIoCContainer.Current.Register<ICacheService>(new CacheService(App));
            TinyIoCContainer.Current.Register<AddressBook>(new AddressBook(App.ApplicationContext));
            
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 500 });                        
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 10000 }, CoordinatePrecision.BallPark.ToString());
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 1000 }, CoordinatePrecision.Coarse.ToString());
            TinyIoCContainer.Current.Register<Geolocator>(new Geolocator(App.ApplicationContext) { DesiredAccuracy = 700 }, CoordinatePrecision.Medium.ToString());

            

        }

        public void InitializeSocialNetwork( Activity main)
        {
            
        
        
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
                                   


            var oauthConfig = new OAuthConfig
            {

                ConsumerKey = settings.TwitterConsumerKey,
                Callback = settings.TwitterCallback,
                ConsumerSecret = settings.TwitterConsumerSecret,
                RequestTokenUrl = settings.TwitterRequestTokenUrl,
                AccessTokenUrl = settings.TwitterAccessTokenUrl,
                AuthorizeUrl = settings.TwitterAuthorizeUrl
            };

            var twitterService = new TwitterServiceMonoDroid(oauthConfig, main);

            TinyIoCContainer.Current.Register<IFacebookService>((c, p) => new FacebookServicesMD( c.Resolve <IAppSettings>().FacebookAppId, main ));
            TinyIoCContainer.Current.Register<ITwitterService>(twitterService);

        }
    }
}