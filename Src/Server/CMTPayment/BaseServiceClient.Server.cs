using System;
using System.Net;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

namespace CMTPayment
{
    public partial class BaseServiceClient
    {
        private ServiceClientBase _client;

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
    }
}
