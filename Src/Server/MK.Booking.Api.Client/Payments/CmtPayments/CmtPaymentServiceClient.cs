using System;
using System.Net;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Authorization;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Capture;
using apcurium.MK.Booking.Api.Client.Cmt.Payments.Tokenize;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments
{
    public class CmtPaymentServiceClient : BaseServiceClient
    {

        public CmtPaymentServiceClient(
            CmtPaymentSettings cmtSettings, string sessionId)
            : base(cmtSettings.IsSandbox
                    ? cmtSettings.SandboxBaseUrl
                    : cmtSettings.BaseUrl, sessionId)            
        {
            Client.Timeout = new TimeSpan(0, 0, 0, 20, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;
            

			ConsumerKey = cmtSettings.CustomerKey;
			ConsumerSecretKey = cmtSettings.ConsumerSecretKey;


            //todo - Bug accept all certificates
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
            
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


        public T Delete<T>(IReturn<T> request)
        {
            return Client.Post(request);
        }
        public T Post<T>(IReturn<T> request)
        {
            return Client.Post(request);
        }
    }
}