using System;
using System.Net;
using ServiceStack;
namespace CustomerPortal.Client
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

        protected virtual ServiceClientBase Client
        {
            get { return _client ?? (_client = CreateClient()); }
        }

        private ServiceClientBase CreateClient()
        {
            var client = new JsonServiceClient(_url);
            var uri = new Uri(_url);
            if (!string.IsNullOrEmpty(_sessionId))
            {
                client.CookieContainer = new CookieContainer();
                client.CookieContainer.Add(uri, new Cookie("ss-opt", "perm"));
                client.CookieContainer.Add(uri, new Cookie("ss-pid", _sessionId));
            }
            return client;
        }
    }
}