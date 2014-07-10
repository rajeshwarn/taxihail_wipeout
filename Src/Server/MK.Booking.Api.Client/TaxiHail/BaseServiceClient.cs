using System.Net;
using System.Net.Cache;
using apcurium.MK.Booking.Mobile.Infrastructure;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

#region

using System;

#endregion

#if CLIENT

#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class BaseServiceClient
    {
        private const string DefaultUserAgent = "TaxiHail";

        private readonly string _sessionId;
        private readonly string _url;
        private readonly IPackageInfo _packageInfo;
        private ServiceClientBase _client;

        public BaseServiceClient(string url, string sessionId, IPackageInfo packageInfo)
        {
            _url = url;
            _sessionId = sessionId;
            _packageInfo = packageInfo;
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
                    request.Headers.Add("Version", _packageInfo.Version);
            };

            return client;
        }
    }
}