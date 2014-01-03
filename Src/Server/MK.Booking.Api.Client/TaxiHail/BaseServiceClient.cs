#region

using System;
using System.Net;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

#endregion

#if CLIENT
using ServiceStack.Common.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class BaseServiceClient
    {
        private readonly string _sessionId;
        private readonly string _url;
        private readonly string _userAgent;
        private ServiceClientBase _client;

        public BaseServiceClient(string url, string sessionId, string userAgent)
        {
            _url = url;
            _sessionId = sessionId;
            _userAgent = userAgent;
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

            client.LocalHttpWebRequestFilter = request => request.UserAgent = _userAgent;
            return client;
        }
    }
}