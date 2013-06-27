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
using MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    public class CmtBasePaymentServiceClient
    {
   

		public CmtBasePaymentServiceClient(string baseUrl,  string consumerKey, string consumerSecretKey,  bool ignoreCertificateErrors )
        {
			ConsumerKey = consumerKey;
			ConsumerSecretKey = consumerSecretKey;

			Client = new CustomXmlServiceClient(baseUrl, ignoreCertificateErrors)
            {
                Timeout = new TimeSpan(0, 0, 0, 20, 0),
                LocalHttpWebRequestFilter = r=>SignRequest(r)
            };        
        }

		protected string ConsumerKey {
			get;
			private set;
		}

		
		protected string ConsumerSecretKey {
			get;
			private set;
		}
        protected CustomXmlServiceClient Client { get; set; }

        private void SignRequest(WebRequest request)
        {
            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(ConsumerKey,
                                                               ConsumerSecretKey,
                                                               "",
                                                               "",
                                                               request.Method,
                                                               request.RequestUri,
                                                               null);
            request.Headers.Add(HttpRequestHeader.Authorization, oauthHeader);         
        }
        
    }
}