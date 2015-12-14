using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
#if CLIENT
using ModernHttpClient;
using System.Net.Http;
#else
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;
using HttpClient = ServiceStack.ServiceClient.Web.ServiceClientBase;
#endif

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
#if CLIENT
        protected HttpClient Client
        {
            get { return _client ?? (_client = CreateClient()); }
        }

        private HttpClient CreateClient()
        {
            var uri = new Uri(_url, UriKind.Absolute);
            Console.WriteLine(_url);
            var cookieHandler = new NativeCookieHandler();

            // CustomSSLVerification must be set to true to enable certificate pinning.
            var nativeHandler = new NativeMessageHandler(throwOnCaptiveNetwork: false, customSSLVerification: true, cookieHandler: cookieHandler);
            
            var client = new HttpClient(nativeHandler)
            {
                BaseAddress = uri,
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };

            // When packageInfo is not specified, we use a default value as the useragent
            client.DefaultRequestHeaders.Add("User-Agent", _packageInfo == null ? DefaultUserAgent : _packageInfo.UserAgent);
            if (_packageInfo != null)
            {
                client.DefaultRequestHeaders.Add("ClientVersion", _packageInfo.Version);
            }

            if (_sessionId.HasValueTrimmed())
            {
                client.DefaultRequestHeaders.Add("Cookie", "ss-opt=perm; ss-pid=" + _sessionId);
            }
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");

            return client;
        }
#else
        protected ServiceClientBase Client
        {
            get { return _client ?? (_client = CreateClient()); }
        }

        private ServiceClientBase CreateClient()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig.EmitCamelCaseNames = true;

            var client = new JsonServiceClient(_url) { Timeout = new TimeSpan(0, 0, 2, 0, 0) };

            var uri = new Uri(_url);
            if (!string.IsNullOrEmpty(_sessionId))
            {
                client.CookieContainer = new CookieContainer();
                client.CookieContainer.Add(uri, new Cookie("ss-opt", "perm"));
                client.CookieContainer.Add(uri, new Cookie("ss-pid", _sessionId));
            }

            // When packageInfo is not specified, we use a default value as the useragent
            client.LocalHttpWebRequestFilter = request =>
            {
                request.UserAgent = _packageInfo == null ? DefaultUserAgent : _packageInfo.UserAgent;

                if (_packageInfo != null)
                {
                    request.Headers.Add("ClientVersion", _packageInfo.Version);
                }
            };

            return client;
        }
#endif

        

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}