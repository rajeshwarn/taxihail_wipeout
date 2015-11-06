using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using TinyMessenger;

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

            _container.Register((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

            _container.Register((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

            _container.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));
            
            _container.Register((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>()));

            _container.Register<ConfigurationClientService>((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().Data.ServiceUrl, GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<ILogger>()));

			_container.Register<IAccountService>((c, p) => AccountService(c));
			_container.Register<IBookingService>((c, p) => new BookingService(
				c.Resolve<IAccountService>(),
				c.Resolve<ILocalization>(), 
				c.Resolve<IAppSettings>(), 
				null,
				c.Resolve<IMessageService>()));

            _container.Register<IApplicationInfoService, ApplicationInfoService>();

#if DEBUG
			// In debug, we should allow all certs to allow us to debug issues that might arise.
			ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;
#else
			ServicePointManager.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
#endif

        }

		private static readonly string[] Hosts = { ".cmtapi.com", ".goarro.com", ".taxihail.com", "test.taxihail.biz" };

		private static readonly string[] PinnedKeys = 
		{
			//*.goarro.com
			"3082010a0282010100a73bd22fc833cf1947894278a33264fe4a447c40d9da3b1e65c746a8fda204d9061b99b233978c22fc6c3ea80c60ee2e3b970b4e7508e42576b2bdaedb7df40a15b3978f8da01bf1fb16c671e54a15c1895dd4386c66905f4d4a7e03496115864ef7ee248ca9a9eca7f0f65c938d1156b290ceb8b0bf689ee37d96bdc0dec7d42233d3458ff94abd6fe64c2e0dd38a96a307602d49a3d64e03d58174e2b78b2d71076887e9396368d36e5e95cc2db06e4610cadaa676cfad197fbbd54e6f28615353b086049df255da12a8357aaa04af35a7e9442b190f85168f6dbbaa0a20f7a2b12de06e34b07e83ea105a16572167d081a9cfb321ccfdc7738248cbfb31650203010001",
			//*.cmtapi.com
			"3082010a0282010100871714605cf4f2ed06e3dca9c00f3199a2fc500645d3276a9fc7e3ad302e37606f63a08d46c4b1e78b02d72696782145202c205202ddf5693594e385866ebf7670a7c4d718d309a3126a01d451764eedc855944f1fdda545c343522d3c1fc1097caea997bf0bb2fdd666bcd4d5d1e12f723fc70e1cbca4d359571382aad664577df8f5855adcc3ff6af396a5a136cce8aeb8c9d6b93051450f58813bb6b501ceea2b182673b0d82b957e0c195a1b616f886a306c4ae883240e470a01805a9ef330db1f9f3ba35e3ae8f70b0265d679146a9e99b9917028b46bed351bba43475062dc06b494629cb2936a702179fe2322b7289a5a41ce4b309f4eae61b6aff91d0203010001",
			//*.taxihail.com
			"3082010a0282010100ad5bf14c5cfdc46212c2ee9d7f055ec9229650fab5fbc54590c5aadef24d9e667b72f8a4246421ff4b82f325d1df98c18d5b6f8be11b1cfe8a335ce10a1bd017bf9fbddf568f4c72e007770c1560771b40b2163afcea2fa4c743145cff98a98d66957e770fc60ed40af17c13523af7d897bc6ca7b7b2c2cf2cb3c85ae3f6459a29e6072be0dbbab895457fca9e69af3d801ed1a067b347aa84d401e92bece6b68033eeec4178453c977dcdcbdf2c6e864a94bac99c9e122a07c2e526c6251c7d21ef9c6a6ec9fb2c36dd43d541a459ff8b5d5979f52eb5c34ca3481dfd75fc6cef8c641f9c4cf1de643ec12d736a1e6f0e662c9f451361dc127d9f74ab8cdd6f0203010001",
			//test.taxihail.biz
			"3082010a028201010098e11cdf768f2ba64857d9852a106d70e60a449fc2314305241a37198554017fc18e19e0c70bd5d61a2eb5cc0956cad4e4ff4cc403f7dea36797e6349e2b9fc9ab76914c2edad0042e2632013b466f095c1bc97309c63fac0a60068ca2e428ffcc7b8e76443f5bb18fd6c5b0b7b2641037e1a891b1e0e92d3b6bc9492684e61091becaa0e86b606fd9412ae0b5bd1fb6246c7a96182f97aa20df656224eaa7ca80d1faa22114ac8d265106b4340c0e809c0a081685cacf37c8bba57401405dc0cd65f1343dc83bd15aff28e26875fdc0d0f00d75e65e429906829398e69b9914e46969427ba1d767aaa46d5915d4bb0b20d76e61f247280cedf431198506c5890203010001"
		};


		private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			var request = sender as HttpWebRequest;

			if (request == null || Hosts.None(request.Host.EndsWith))
			{
				// We are using the default certificate validation.
				return new X509Certificate2(certificate).Verify();
			}

			var publicKeyString = certificate.GetPublicKeyString();

			return PinnedKeys.Any(p => p.Equals(publicKeyString, StringComparison.InvariantCultureIgnoreCase));
		}

	    private static AccountService AccountService(TinyIoCContainer c)
	    {
		    return new AccountService(c.Resolve<IAppSettings>(), null, null, null, null, null);
	    }

	    private string GetSessionId ()
        {
            var authData = _container.Resolve<ICacheService> ().Get<AuthenticationData> ("AuthenticationData");
            var sessionId = authData == null ? null : authData.SessionId;
            if (sessionId == null) 
			{
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





