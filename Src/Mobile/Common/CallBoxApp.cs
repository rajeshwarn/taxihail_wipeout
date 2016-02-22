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
using apcurium.MK.Common;

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
                new AccountServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), null, null, c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()), 
                "NotAuthenticated");
			
            _container.Register<IAccountServiceClient>((c, p) =>
                new AccountServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), null, c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()),
                "Authenticate");
			
            _container.Register<IAccountServiceClient>((c, p) => new AccountServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), null, c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()));

            _container.Register((c, p) => new ReferenceDataServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()));

            _container.Register((c, p) => new OrderServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()));

            _container.Register<IAuthServiceClient>((c, p) => new AuthServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()));
            
            _container.Register((c, p) => new ApplicationInfoServiceClient(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()));

            _container.Register((c, p) => new ConfigurationClientService(c.Resolve<IAppSettings>().GetServiceUrl(), GetSessionId(), c.Resolve<IPackageInfo>(), c.Resolve<IConnectivityService>(), c.Resolve<ILogger>()));

            _container.Register<IAccountService>((c, p) => new AccountService(c.Resolve<IAppSettings>(), null, null, c.Resolve<ILocalization>(), c.Resolve<IConnectivityService>(), null, null));

            _container.Register<IBookingService>((c, p) => new BookingService(
				c.Resolve<IAccountService>(),
				c.Resolve<ILocalization>(), 
				c.Resolve<IAppSettings>(),
				null,
				c.Resolve<IMessageService>(),
				c.Resolve<IIPAddressManager>()));

            _container.Register<IApplicationInfoService, ApplicationInfoService>();

            ServicePointManager.ServerCertificateValidationCallback += ServerCertificateValidationCallback;
        }

		private static readonly string[] Hosts = { ".cmtapi.com", ".goarro.com", ".taxihail.com" };

		private static readonly string[] PinnedKeys = 
		{
			//*.goarro.com
			"3082010a0282010100a73bd22fc833cf1947894278a33264fe4a447c40d9da3b1e65c746a8fda204d9061b99b233978c22fc6c3ea80c60ee2e3b970b4e7508e42576b2bdaedb7df40a15b3978f8da01bf1fb16c671e54a15c1895dd4386c66905f4d4a7e03496115864ef7ee248ca9a9eca7f0f65c938d1156b290ceb8b0bf689ee37d96bdc0dec7d42233d3458ff94abd6fe64c2e0dd38a96a307602d49a3d64e03d58174e2b78b2d71076887e9396368d36e5e95cc2db06e4610cadaa676cfad197fbbd54e6f28615353b086049df255da12a8357aaa04af35a7e9442b190f85168f6dbbaa0a20f7a2b12de06e34b07e83ea105a16572167d081a9cfb321ccfdc7738248cbfb31650203010001",
			//*.cmtapi.com
			"3082010a0282010100871714605cf4f2ed06e3dca9c00f3199a2fc500645d3276a9fc7e3ad302e37606f63a08d46c4b1e78b02d72696782145202c205202ddf5693594e385866ebf7670a7c4d718d309a3126a01d451764eedc855944f1fdda545c343522d3c1fc1097caea997bf0bb2fdd666bcd4d5d1e12f723fc70e1cbca4d359571382aad664577df8f5855adcc3ff6af396a5a136cce8aeb8c9d6b93051450f58813bb6b501ceea2b182673b0d82b957e0c195a1b616f886a306c4ae883240e470a01805a9ef330db1f9f3ba35e3ae8f70b0265d679146a9e99b9917028b46bed351bba43475062dc06b494629cb2936a702179fe2322b7289a5a41ce4b309f4eae61b6aff91d0203010001",
			//NEW *.cmtapi.com
            "3082010a0282010100b26068b17c14ca401e4b67502530184a0238cb2b46f19148d1c99ef98b06da79ea74028e408d96b8b493988aaec705f019cb19a16514b295f4d94cc9aed30e90b7409030b46a2e49cc6bfd36a202e43c1b1443347f118215ceef4bdacf5d9c10f482b33c7dda419e06c1bf79a98c45423b06b734722f4aa0bdb44feb53ed9fa1691694dbbe6a1678c63c4da3642c9ff29f7dff49e1f393967bcc1fd73c3eee92309ff1ac270551b390f51e0ea60797581be9916bcf14c82698590ee05c3c4a0bd72b4c9f812ca0df0d2ce9f022bcd6207e82e95f32ad82d52c1907a039669639c97b595fe7fb98c12fa09a69d9ef9c2b97dc0b825d57c4f65af964ee9a1728fd0203010001",
            //*.taxihail.com
			"3082010a0282010100ad5bf14c5cfdc46212c2ee9d7f055ec9229650fab5fbc54590c5aadef24d9e667b72f8a4246421ff4b82f325d1df98c18d5b6f8be11b1cfe8a335ce10a1bd017bf9fbddf568f4c72e007770c1560771b40b2163afcea2fa4c743145cff98a98d66957e770fc60ed40af17c13523af7d897bc6ca7b7b2c2cf2cb3c85ae3f6459a29e6072be0dbbab895457fca9e69af3d801ed1a067b347aa84d401e92bece6b68033eeec4178453c977dcdcbdf2c6e864a94bac99c9e122a07c2e526c6251c7d21ef9c6a6ec9fb2c36dd43d541a459ff8b5d5979f52eb5c34ca3481dfd75fc6cef8c641f9c4cf1de643ec12d736a1e6f0e662c9f451361dc127d9f74ab8cdd6f0203010001"
		};

		private static string GetHostFromFullAddress(string url)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				return null;
			}

			if(!url.StartsWith("http"))
			{
				return url;
			}

			var uri = new Uri(url);

			return uri.Host;
		}

		private static bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			var request = sender as HttpWebRequest;

			var host = request != null
				? request.Host
				: GetHostFromFullAddress(sender as string) ?? "unknown";

			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				Mvx.Resolve<ILogger>().LogMessage("WARNING: Following certificate was not found as valid. \n website: {0} \n certificate: {1} \n cause: {2}",
					host,
					certificate.ToString(true),
					sslPolicyErrors.ToString());

				return false;
			}

			// If the certificate is valid but not part of our pinned certs.
			if (request == null || Hosts.None(host.EndsWith))
			{
				return true;
			}

			var publicKeyString = certificate.GetPublicKeyString();

			return PinnedKeys.Any(p => p.Equals(publicKeyString, StringComparison.InvariantCultureIgnoreCase));
		}

        private string GetSessionId()
        {
            var appCacheService = _container.Resolve<ICacheService>();
            var authData = appCacheService.Get<AuthenticationData>("AuthenticationData");

            var userCacheService = _container.Resolve<ICacheService>("UserAppCache");
            if (authData != null)
            {
                // We have a legacy authData (Auth data stored in the application Cache. We need to move it to the UserCache.
                userCacheService.Set("AuthenticationData", authData);
                appCacheService.Clear("AuthenticationData");
            }
            else
            {
                authData = userCacheService.Get<AuthenticationData>("AuthenticationData");
            }


            var sessionId = authData.SelectOrDefault(data => data.SessionId) ?? appCacheService.Get<string>("SessionId");

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





