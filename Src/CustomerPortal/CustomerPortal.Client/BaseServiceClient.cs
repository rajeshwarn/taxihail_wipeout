using System;
using System.Net;
using System.Net.Http;

namespace CustomerPortal.Client
{
    public class BaseServiceClient
    {
        public BaseServiceClient()
        {
            Client = new HttpClient(new HttpClientHandler
            {
                Credentials = new NetworkCredential("taxihail@apcurium.com", "apcurium5200!")
            });
            Client.BaseAddress = new Uri(GetUrl());
        }

        public HttpClient Client { get; set; }

        private static string GetUrl()
        {
            var url = "http://customer.taxihail.com/api/";
            			#if DEBUG
                        url = "http://localhost/CustomerPortal.Web/api/";
            			#endif
            return url;
        }

       
    }
}