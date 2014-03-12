#region

using System;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments
{
    public class CmtPaymentServiceClient : BaseServiceClient
    {
        public CmtPaymentServiceClient(CmtPaymentSettings cmtSettings, string sessionId, string userAgent)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxBaseUrl
                : cmtSettings.BaseUrl, sessionId, userAgent)
        {
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;
            ConsumerKey = cmtSettings.CustomerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;

            //todo - Bug accept all certificates
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
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
            request.ContentType = ContentType.Json;

        }

        public Task<T> GetAsync<T>(IReturn<T> request)
        {
            return Client.GetAsync(request);
        }

        public Task<T> DeleteAsync<T>(IReturn<T> request)
        {
            return Client.DeleteAsync(request);
        }

        public Task<T> PostAsync<T>(IReturn<T> request)
        {
            return Client.PostAsync(request);
        }
    }
}