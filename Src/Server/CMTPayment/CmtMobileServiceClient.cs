using System;
using System.Net.Http;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
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
            
            ConsumerKey = cmtSettings.ConsumerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;
        }

        protected string ConsumerKey { get; private set; }
        protected string ConsumerSecretKey { get; private set; }

        public Task<T> Get<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            SetOAuthHeader(url, "GET", ConsumerKey, ConsumerSecretKey);

            return Client.GetAsync(request);
        }

        public Task<T> Delete<T>(string requestUrl)
        {
            SetOAuthHeader(requestUrl, "DELETE", ConsumerKey, ConsumerSecretKey);

            return Client.DeleteAsync<T>(requestUrl);
        }

        public Task<T> Delete<T>(IReturn<T> payload)
        {
            var url = payload.GetUrlFromRoute();

            SetOAuthHeader(url, "DELETE", ConsumerKey, ConsumerSecretKey);

            return Client.DeleteAsync(payload);
        }

        public Task<T> Post<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            SetOAuthHeader(url, "POST", ConsumerKey, ConsumerSecretKey);

            return Client.PostAsync(request);
        }
        public Task<HttpResponseMessage> Post<T>(string relativeOrAbsoluteUrl, IReturn<T> request)
        {
            SetOAuthHeader(relativeOrAbsoluteUrl, "POST", ConsumerKey, ConsumerSecretKey);

            return Client.PostAndGetHttpResponseMessage(relativeOrAbsoluteUrl, request);
        }

        public Task<T> Put<T>(string requestUrl, IReturn<T> payload)
        {
            SetOAuthHeader(requestUrl, "PUT", ConsumerKey, ConsumerSecretKey);

            return Client.PutAsync<T>(requestUrl, payload);
        }

        public Task<T> Put<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            SetOAuthHeader(url, "PUT", ConsumerKey, ConsumerSecretKey);

            return Client.PutAsync<T>(request);
        }
    }
}