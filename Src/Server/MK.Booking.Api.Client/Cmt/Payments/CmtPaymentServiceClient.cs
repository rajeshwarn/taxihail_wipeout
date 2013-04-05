using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Client.Client;
using System.Net;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using ServiceStack.Service;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    public class CmtPaymentServiceClient
    {
        public static string MERCHANT_TOKEN = "E4AFE87B0E864228200FA947C4A5A5F98E02AA7A3CFE907B0AD33B56D61D2D13E0A75F51641AB031500BD3C5BDACC114";
        public static string CONSUMER_KEY = "vmAoqWEY3zIvUCM4";
        public static string CONSUMER_SECRET_KEY= "DUWzh0jAldPc7C5I";

        public static string PRODUCTION_BASE_URL = "https://payment.cmtapi.com/v2/merchants/"+MERCHANT_TOKEN+"/";        
        public static string SANDBOX_BASE_URL = "https://payment-sandbox.cmtapi.com/v2/merchants/"+MERCHANT_TOKEN+"/";
#if DEBUG
        public static string BASE_URL = SANDBOX_BASE_URL;
#else
        public static string BASE_URL = SANDBOX_BASE_URL; // for now will will not use production
#endif

        private CustomXmlServiceClient _client;
        protected CustomXmlServiceClient Client
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

        private CustomXmlServiceClient CreateClient()
        {
            var client = new CustomXmlServiceClient(BASE_URL)
            {
                Timeout = new TimeSpan(0, 0, 0, 20, 0),
                LocalHttpWebRequestFilter = r=>SignRequest(r)
            };
            return client;
        }

        private void SignRequest(WebRequest request)
        {
            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(CONSUMER_KEY,
                                                               CONSUMER_SECRET_KEY,
                                                               "",
                                                               "",
                                                               request.Method,
                                                               request.RequestUri,
                                                               null);
            request.Headers.Add(HttpRequestHeader.Authorization, oauthHeader);         
        }
        
    }
}