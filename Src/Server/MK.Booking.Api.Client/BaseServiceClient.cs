using System;
using System.Net;
#if !CLIENT
using ServiceStack.ServiceInterface.Auth;
#else
using ServiceStack.Common.ServiceClient.Web;
#endif
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Api.Client
{
    public class BaseServiceClient
    {
        private ServiceClientBase _client;
        private readonly string _url;
        private readonly string _sessionId;

        public BaseServiceClient(string url, string sessionId)
        {
            _url = url;
            _sessionId = sessionId;
        }

        protected ServiceClientBase Client
        {
            get
            {
                if (_client == null)
                {
                    _client = CreateClient();
                }
                return _client;
            }
        }

        private ServiceClientBase CreateClient()
        {
            var client = new JsonServiceClient(_url);
            client.Timeout = new TimeSpan(0, 0, 0, 20, 0);
            var uri = new Uri(_url);
            if (!string.IsNullOrEmpty(_sessionId))
            {
                client.CookieContainer = new System.Net.CookieContainer();
                client.CookieContainer.Add(uri, new Cookie("ss-opt", "perm"));
                client.CookieContainer.Add(uri, new Cookie("ss-pid", _sessionId));
            }

            return client;
        }
    }
}
