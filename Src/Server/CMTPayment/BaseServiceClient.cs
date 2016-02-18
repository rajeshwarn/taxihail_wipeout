using System;
using System.Net.Http;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Extensions;
using CMTPayment.Authorization;

namespace CMTPayment
{
    public partial class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private readonly string _sessionId;
        private readonly string _url;
        private readonly IPackageInfo _packageInfo;
        private readonly IConnectivityService _connectivityService;

        private HttpClient _client;

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService)
        {
            _url = url;
            _sessionId = sessionId;
            _packageInfo = packageInfo;
            _connectivityService = connectivityService;
        }

        protected HttpClient Client
        {
            get { return _client ?? (_client = CreateClient()); }
        }

        public void SetOAuthHeader(string url, string method, string consumerKey, string consumerSecretKey)
        {
            var baseAddress = Client.BaseAddress.ToString();
            if (Client.BaseAddress.Host.Contains("runscope"))
            {
                baseAddress = baseAddress.Replace("payment-cmtapi-com-hqy5tesyhuwv.runscope.net", "payment.cmtapi.com");
            }

            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(consumerKey,
                consumerSecretKey,
                "",
                "",
                method,
                new Uri(baseAddress + url),
                null);

            if (Client.DefaultRequestHeaders.Authorization != null)
            {
                Client.DefaultRequestHeaders.Authorization = null;
            }

            Client.DefaultRequestHeaders.Add("Authorization", oauthHeader);
        }

        private string GetUserAgent()
        {
            return _packageInfo == null || !_packageInfo.UserAgent.HasValueTrimmed()
                ? DefaultUserAgent
                : _packageInfo.UserAgent;
        }
    }
}