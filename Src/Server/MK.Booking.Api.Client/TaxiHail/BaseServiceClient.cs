using System;
using System.Net;
using MK.Booking.Api.Client;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;
#if !CLIENT
#else
using ServiceStack.Common.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
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

            Console.WriteLine ( "-----------------------------------  CreateClient Session ID :" + _sessionId );
            JsConfig.DateHandler = JsonDateHandler.ISO8601;         
            
			var client = new TaxiHailJsonServiceClient(_url);
            client.Timeout = new TimeSpan(0, 0, 2,0, 0);

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
