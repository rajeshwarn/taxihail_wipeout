using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using System.Net.Http;

#if CLIENT
using ModernHttpClient;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private string _sessionId;
        private string _url;
        private readonly IPackageInfo _packageInfo;
        private readonly IConnectivityService _connectivityService;
        protected readonly ILogger Logger;

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
        {
            _url = url.AppendIfMissing("/v2");
            _sessionId = sessionId;
            _packageInfo = packageInfo;
            _connectivityService = connectivityService;
            Logger = logger;
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
            // Recreate the HttpClient everytime to fix the problem with certain calls returning Unauthorized for no reason (seems to be on Android only)
            get { return CreateClient(); }
        }

#if CLIENT
        private HttpClient CreateHttpClient(Uri baseAddress)
        {
            var cookieHandler = new NativeCookieHandler();

            // CustomSSLVerification must be set to true to enable certificate pinning.
            var nativeHandler = new CustomNativeMessageHandler(_connectivityService, throwOnCaptiveNetwork: false, customSSLVerification: true, enableRc4Compatibility: false, cookieHandler: cookieHandler);

            // otherwise we won't be able to handle 304 NotModified ourselves (ie: Terms & Conditions)
            nativeHandler.DisableCaching = true;

            var client = new HttpClient(nativeHandler)
            {
                BaseAddress = baseAddress,
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };

            if (_sessionId.HasValueTrimmed())
            {
                client.DefaultRequestHeaders.Add("Cookie", "ss-opt=perm; ss-pid=" + _sessionId);
            }

            return client;
        }
#else
        private HttpClient CreateHttpClient(Uri baseAddress)
        {
            var cookieContainer = new CookieContainer();

            var basicHttpHandler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                UseCookies = true
            };

            if (_sessionId.HasValueTrimmed())
            {
                var escapedSession = Uri.EscapeDataString(_sessionId);

                cookieContainer.Add(baseAddress, new Cookie("ss-opt", "perm"));
                cookieContainer.Add(baseAddress, new Cookie("ss-pid", escapedSession));
            }

            return new HttpClient(basicHttpHandler)
            {
                BaseAddress = baseAddress,
                Timeout = new TimeSpan(0, 0, 2, 0, 0)
            };
        }
#endif
        private HttpClient CreateClient()
        {
            var client = CreateHttpClient(new Uri(_url, UriKind.Absolute));

            // When packageInfo is not specified, we use a default value as the useragent
            client.DefaultRequestHeaders.Add("User-Agent", GetUserAgent());
            if (_packageInfo != null)
            {
                client.DefaultRequestHeaders.Add("ClientVersion", _packageInfo.Version);
            }
            
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");

            return client;
        }
           
        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }

        private string GetUserAgent()
        {
            return _packageInfo == null || !_packageInfo.UserAgent.HasValueTrimmed() 
                ? DefaultUserAgent 
                : _packageInfo.UserAgent;
        }
    }
}