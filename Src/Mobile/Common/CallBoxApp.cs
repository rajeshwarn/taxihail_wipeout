using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using MK.Booking.Mobile.Infrastructure.Mvx;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Mobile
{
    public class CallBoxApp: MvxApplication
        , IMvxServiceProducer<IMvxStartNavigation>
    {

        public CallBoxApp()
        {
            InitaliseServices();
            InitialiseStartNavigation();
        }
        
        private void InitaliseServices()
        {
            TinyIoCContainer.Current.Register<ITinyMessengerHub, TinyMessengerHub>();

			
			TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => 
			                                                         new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, null, null),
			                                                         "NotAuthenticated");
			
			TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) =>
			                                                         new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c), null),
			                                                         "Authenticate");
			
			TinyIoCContainer.Current.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c),null));

            TinyIoCContainer.Current.Register<ReferenceDataServiceClient>((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            TinyIoCContainer.Current.Register<OrderServiceClient>((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            TinyIoCContainer.Current.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));
            
            TinyIoCContainer.Current.Register<ApplicationInfoServiceClient>((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c), c.Resolve<IPackageInfo>().UserAgent));

            TinyIoCContainer.Current.Register<IConfigurationManager>((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().ServiceUrl, this.GetSessionId(c), c.Resolve<ILogger>(), c.Resolve<IPackageInfo>().UserAgent));

            TinyIoCContainer.Current.Register<IAccountService, AccountService>();
            TinyIoCContainer.Current.Register<IBookingService, BookingService>();

            TinyIoCContainer.Current.Register<IApplicationInfoService, ApplicationInfoService>();
        }
        
        private string GetSessionId (TinyIoCContainer container)
        {
            var authData = container.Resolve<ICacheService> ().Get<AuthenticationData> ("AuthenticationData");
            var sessionId = authData == null ? null : authData.SessionId;
            if (sessionId == null) {
                sessionId = container.Resolve<ICacheService> ().Get<string>("SessionId");
            }
            return sessionId;
        }
        
        private void InitialiseStartNavigation()
        {
            var startApplicationObject = new StartCallboxNavigation();
            this.RegisterServiceInstance<IMvxStartNavigation>(startApplicationObject);
        }

        protected override IMvxViewModelLocator CreateDefaultViewModelLocator()
        {
            return new TinyIocViewModelLocator(); //base.CreateDefaultViewModelLocator();
        }
    }
}





