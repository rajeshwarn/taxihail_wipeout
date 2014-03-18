#region

using System;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Configuration.Impl;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.Text;
using apcurium.MK.Common.Diagnostic;
using System.IO;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments
{
    public class CmtPaymentServiceClient : BaseServiceClient
    {
        private readonly ILogger _logger;
        public CmtPaymentServiceClient(CmtPaymentSettings cmtSettings, string sessionId, string userAgent, ILogger logger)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxBaseUrl
                : cmtSettings.BaseUrl, sessionId, userAgent)
        {
            _logger = logger;
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;            
            ConsumerKey = cmtSettings.ConsumerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;

            //todo - Bug accept all certificates
            ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
        }

        protected string ConsumerKey { get; private set; }
        protected string ConsumerSecretKey { get; private set; }

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
            request.ContentType = ContentType.Json;


            _logger.LogMessage("CMT request header info : " + request.Headers.ToString());
            _logger.LogMessage("CMT request info : " + request.ToJson());
        }

       
        public Task<T> GetAsync<T>(IReturn<T> request)
        {
            _logger.LogMessage("CMT Get : " + request.ToJson());
            var result = Client.GetAsync(request);
            result.ContinueWith(r => _logger.LogMessage("CMT Get Result: " + r.Result.ToJson())); 
            return result;
        }

        public Task<T> DeleteAsync<T>(IReturn<T> request)
        {
            _logger.LogMessage("CMT Delete : " + request.ToJson());
            var result = Client.DeleteAsync(request);
            result.ContinueWith(r => _logger.LogMessage("CMT Delete Result: " + r.Result.ToJson())); 
            return result;

        }

        public Task<T> PostAsync<T>(IReturn<T> request)
        {
            _logger.LogMessage("CMT Post : " + request.ToJson());
            var result = Client.PostAsync(request);
            result.ContinueWith(r => _logger.LogMessage("CMT Post Result: " + r.Result.ToJson())); 
            return result;
        }
    }
}