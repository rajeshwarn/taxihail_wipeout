using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ModernHttpClient;

namespace CMTPayment
{
    public class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private readonly string _sessionId;
        private readonly string _url;
        private readonly IPackageInfo _packageInfo;
        private HttpClient _client;

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo)
        {
            _url = url;
            _sessionId = sessionId;
            _packageInfo = packageInfo;
        }

        protected HttpClient Client
        {
            get { return _client ?? (_client = CreateClient()); }
        }

        private HttpClient CreateClient()
        {
            var uri = new Uri(_url);

            var cookieHandler = new NativeCookieHandler();

            if (!string.IsNullOrEmpty(_sessionId))
            {
                cookieHandler.SetCookies(new[]
                {
                    new Cookie("ss-opt", "perm"),
                    new Cookie("ss-pid", _sessionId)
                });
            }

            var messageHandler = new NativeMessageHandler(throwOnCaptiveNetwork: false, customSSLVerification: true, cookieHandler: cookieHandler);

            var client = new HttpClient(messageHandler)
            {
                Timeout = new TimeSpan(0, 0, 2, 0, 0),
                BaseAddress = uri
            };

            // When packageInfo is not specified, we use a default value as the useragent
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_packageInfo == null ? DefaultUserAgent : _packageInfo.UserAgent));
            if (_packageInfo != null)
            {
                client.DefaultRequestHeaders.Add("ClientVersion", _packageInfo.Version);
            }

            return client;
        }
    }
}