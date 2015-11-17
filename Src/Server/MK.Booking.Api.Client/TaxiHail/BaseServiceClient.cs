using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private string _sessionId;
        private string _url;
        private readonly IPackageInfo _packageInfo;
        private ServiceClientBase _client;

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

        protected ServiceClientBase Client
        {
            get { return _client ?? (_client = CreateClient()); }
        }

        private ServiceClientBase CreateClient()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            JsConfig.EmitCamelCaseNames = true;

            var client = new JsonServiceClient(_url) {Timeout = new TimeSpan(0, 0, 2, 0, 0)};

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

        protected static string BuildQueryString(IEnumerable<KeyValuePair<string, string>> @params)
        {
            return "?" + string.Join("&", @params.Select(x => string.Join("=", x.Key, x.Value)));
        }
    }
}