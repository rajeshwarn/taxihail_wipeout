using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using ModernHttpClient;

namespace CMTPayment
{
    public partial class BaseServiceClient
    {
        private HttpClient _client;

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
        
        public void SetOAuthHeader(string url, string method, string consumerKey, string consumerSecretKey)
        {
            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(consumerKey,
                consumerSecretKey,
                "",
                "",
                method,
                new Uri(url),
                null);

            var authHeader = new AuthenticationHeaderValue(oauthHeader);

            _client.DefaultRequestHeaders.Authorization = authHeader;
        }

    }
}