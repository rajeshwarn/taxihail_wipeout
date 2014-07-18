#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments
{
    public class CmtMobileServiceClient : BaseServiceClient
    {
        public CmtMobileServiceClient(CmtPaymentSettings cmtSettings, string sessionId, IPackageInfo packageInfo)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxMobileBaseUrl
                : cmtSettings.MobileBaseUrl, sessionId, packageInfo)
        {
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;

            ConsumerKey = cmtSettings.ConsumerKey;
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
        }

        public T Get<T>(IReturn<T> request)
        {
            return Client.Get(request);
        }

        public T Delete<T>(IReturn<T> request)
        {
            return Client.Delete(request);
        }

        public T Post<T>(IReturn<T> request)
        {
            return Client.Post(request);
        }
    }
}