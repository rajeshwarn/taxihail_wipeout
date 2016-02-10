using System;
using System.Net;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using CMTPayment.Authorization;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceHost;

namespace CMTPayment
{
    public class CmtMobileServiceClient : BaseServiceClient
    {
        public CmtMobileServiceClient(CmtPaymentSettings cmtSettings, ServiceType serviceType, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxMobileBaseUrl
                : cmtSettings.MobileBaseUrl, sessionId, packageInfo, connectivityService)
        {
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;

            var credentialsForService = cmtSettings.GetCredentials(serviceType);
            ConsumerKey = credentialsForService.ConsumerKey;
            ConsumerSecretKey = credentialsForService.ConsumerSecretKey;
        }

        protected string ConsumerKey { get; private set; }
        protected string ConsumerSecretKey { get; private set; }

        private void SignRequest(WebRequest request)
        {
            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(ConsumerKey,
                ConsumerSecretKey,
                "",
                "",
                request.Method,
                request.RequestUri,
                null);
            request.Headers.Add(HttpRequestHeader.Authorization, oauthHeader);
        }

        public T Get<T>(IReturn<T> request)
        {
            return Client.Get(request);
        }

        public T Delete<T>(string requestUrl)
        {
            return Client.Delete<T>(requestUrl);
        }

        public T Delete<T>(IReturn<T> payload)
        {
            return Client.Delete(payload);
        }

        public T Post<T>(IReturn<T> request)
        {
            return Client.Post(request);
        }

        public T Put<T>(string requestUrl, IReturn<T> payload)
        {
            return Client.Put<T>(requestUrl, payload);
        }

        public T Put<T>(IReturn<T> request)
        {
            return Client.Put(request);
        }
    }
}