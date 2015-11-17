using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

namespace CMTPayment
{
    public class CmtPaymentServiceClient : BaseServiceClient
    {
        private readonly ILogger _logger;
        public CmtPaymentServiceClient(CmtPaymentSettings cmtSettings, string sessionId, IPackageInfo packageInfo, ILogger logger)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxBaseUrl
                : cmtSettings.BaseUrl, sessionId, packageInfo)
        {
            _logger = logger;
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;
            Client.LocalHttpWebResponseFilter = LogErrorBody;

            //Client.Proxy = new WebProxy("192.168.12.122", 8888);
            ConsumerKey = cmtSettings.ConsumerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;
		}


        protected string ConsumerKey { get; private set; }
        protected string ConsumerSecretKey { get; private set; }

        private void SignRequest(WebRequest request)
        {
            var requestUri = request.RequestUri;
            if (request.RequestUri.Host.Contains("runscope"))
            {
                var url = request.RequestUri.ToString().Replace("payment-cmtapi-com-hqy5tesyhuwv.runscope.net", "payment.cmtapi.com");
                requestUri = new Uri(url);
            }

            var oauthHeader = OAuthAuthorizer.AuthorizeRequest(ConsumerKey,
                ConsumerSecretKey,
                "",
                "",
                request.Method,
                requestUri,
                null);
            request.Headers.Add(HttpRequestHeader.Authorization, oauthHeader);
            request.ContentType = ContentType.Json;


            _logger.Maybe(() => _logger.LogMessage("CMT request header info : " + request.Headers.ToString()));
            _logger.Maybe(() => _logger.LogMessage("CMT request info : " + request.ToJson()));
        }

        public Task<T> GetAsync<T>(IReturn<T> request)
        {
            _logger.Maybe(() => _logger.LogMessage("CMT Get : " + request.ToJson()));
            var result = Client.GetAsync(request);
            result.ContinueWith(r => LogResult(r, "CMT Get Result: "));
            return result;
        }

        public Task<T> DeleteAsync<T>(IReturn<T> request)
        {
            _logger.Maybe(() => _logger.LogMessage("CMT Delete : " + request.ToJson()));
            var result = Client.DeleteAsync(request);
            result.ContinueWith(r => LogResult(r, "CMT Delete Result: "));
            return result;
        }

        public Task<T> PostAsync<T>(IReturn<T> request)
        {
            _logger.Maybe(() => _logger.LogMessage("CMT Post : " + request.ToJson()));
            var result = Client.PostAsync(request);
            result.ContinueWith(r => LogResult(r, "CMT Post Result: "));
            return result;
        }

        private void LogResult<T>(Task<T> result, string message)
        {
            _logger.Maybe(() =>
            {
                if (!result.IsFaulted)
                {
                    _logger.LogMessage(message + result.Result.ToJson());
                }
                else if (result.Exception != null)
                {
                    _logger.LogMessage(message + " EXCEPTION.");
                    _logger.LogError(result.Exception);
                }

            });
        }

        private void LogErrorBody(HttpWebResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogMessage("CMT Response Status Code : " + response.StatusCode);
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    _logger.LogMessage("CMT Response Body : " + reader.ReadToEnd());   
                }
            }
        }
    }
}