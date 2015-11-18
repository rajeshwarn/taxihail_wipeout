using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ModernHttpClient;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private string _sessionId;
        private string _url;
        private readonly IPackageInfo _packageInfo;
        private HttpClient _client;

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo)
        {
            _url = url;
            _sessionId = sessionId;
            _packageInfo = packageInfo;
        }

        public BaseServiceClient()
        {
        }

        public void Setup(string url, string sessionId)
        {
            _url = url;
            _sessionId = sessionId;
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

            // CustomSSLVerification must be set to true to enable certificate pinning.
            var nativeHandler = new NativeMessageHandler(throwOnCaptiveNetwork: false, customSSLVerification: true, cookieHandler: cookieHandler);

            var client = new HttpClient(nativeHandler)
            {
                BaseAddress = uri,
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };

            // When packageInfo is not specified, we use a default value as the useragent
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(_packageInfo == null ? DefaultUserAgent : _packageInfo.UserAgent));
            if (_packageInfo != null)
            {
                client.DefaultRequestHeaders.Add("ClientVersion", _packageInfo.Version);
            }

            return client;
        }

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}