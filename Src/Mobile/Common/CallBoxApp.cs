using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;

namespace apcurium.MK.Booking.Mobile
{
    public class CallBoxApp: MvxApplication
    {
        readonly TinyIoCContainer _container;
        public CallBoxApp()
        {
            _container = TinyIoCContainer.Current;

            InitaliseServices();
            InitialiseStartNavigation();
        }

        private void InitaliseServices()
        {
            _container.Register<ITinyMessengerHub, TinyMessengerHub>();

			
            _container.Register<IAccountServiceClient>((c, p) => 
                new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, null, null),
			                                                         "NotAuthenticated");
			
            _container.Register<IAccountServiceClient>((c, p) =>
                new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), null),
			                                                         "Authenticate");
			
            _container.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(),null));

            _container.Register((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

            _container.Register((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

            _container.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));
            
            _container.Register((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

            _container.Register<ConfigurationClientService>((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>().UserAgent));

            _container.Register<IAccountService, AccountService>();
            _container.Register<IBookingService, BookingService>();

            _container.Register<IApplicationInfoService, ApplicationInfoService>();
        }
        
        private string GetSessionId ()
        {
            var authData = _container.Resolve<ICacheService> ().Get<AuthenticationData> ("AuthenticationData");
            var sessionId = authData == null ? null : authData.SessionId;
            if (sessionId == null) {
                sessionId = _container.Resolve<ICacheService> ().Get<string>("SessionId");
            }
            return sessionId;
        }
        
        private void InitialiseStartNavigation()
        {
			Mvx.RegisterSingleton<IMvxAppStart>(new StartCallboxNavigation());
        }

        protected override IMvxViewModelLocator CreateDefaultViewModelLocator()
        {
            return new ViewModelLocator(Mvx.Resolve<IAnalyticsService>());
        }
    }
}





