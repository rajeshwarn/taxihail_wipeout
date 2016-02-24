using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http.Extensions;
using CMTPayment.Authorization;
using ServiceStack.ServiceHost;

namespace CMTPayment
{
    public class CmtMobileServiceClient : BaseServiceClient
    {
        public CmtMobileServiceClient(CmtPaymentSettings cmtSettings, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxMobileBaseUrl
                : cmtSettings.MobileBaseUrl, sessionId, packageInfo, connectivityService)
        {
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            //Client.LocalHttpWebRequestFilter = SignRequest;

            ConsumerKey = cmtSettings.ConsumerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;
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

        public Task<T> Get<T>(IReturn<T> request)
        {
            return Client.GetAsync(request);
        }

        public Task<T> Delete<T>(string requestUrl)
        {
            return Client.DeleteAsync<T>(requestUrl);
        }

        public Task<T> Delete<T>(IReturn<T> payload)
        {
            return Client.DeleteAsync(payload);
        }

        public Task<T> Post<T>(IReturn<T> request)
        {
            return Client.PostAsync(request);
        }
        public Task<HttpResponseMessage> Post<T>(string relativeOrAbsoluteUrl, IReturn<T> request)
        {
            return Client.PostAndGetHttpResponseMessage(relativeOrAbsoluteUrl, request);
        }

        public Task<T> Put<T>(string requestUrl, IReturn<T> payload)
        {
            return Client.PutAsync<T>(requestUrl, payload);
        }

        public Task<T> Put<T>(IReturn<T> request)
        {
            return Client.PutAsync<T>(request);
        }
    }
}