using System;
using System.Net;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments
{
    public class CmtPaymentServiceClient : CustomXmlServiceClient
    {

        public CmtPaymentServiceClient(
            string baseUrl,  bool ignoreCertificateErrors )
            :base(baseUrl,ignoreCertificateErrors)
        {
            Timeout = new TimeSpan(0, 0, 0, 20, 0);
            LocalHttpWebRequestFilter = SignRequest;

		    string consumerKey = "";
		    string consumerSecretKey = "";

			ConsumerKey = consumerKey;
			ConsumerSecretKey = consumerSecretKey;  
        }

		protected string ConsumerKey {
			get;
			private set;
		}

		
		protected string ConsumerSecretKey {
			get;
			private set;
		}

        
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