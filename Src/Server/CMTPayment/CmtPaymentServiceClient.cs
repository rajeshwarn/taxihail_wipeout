using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using System.Net.Http;
using apcurium.MK.Common.Http.Response;


namespace CMTPayment
{
    public class CmtPaymentServiceClient : BaseServiceClient
    {
        private readonly ILogger _logger;

        public CmtPaymentServiceClient(CmtPaymentSettings cmtSettings, string sessionId, IPackageInfo packageInfo, ILogger logger, IConnectivityService connectivityService)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxBaseUrl
                : cmtSettings.BaseUrl, sessionId, packageInfo, connectivityService)
        {
            _logger = logger;

            //Client.Proxy = new WebProxy("192.168.12.122", 8888);
            ConsumerKey = cmtSettings.ConsumerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;
        }

        protected string ConsumerKey { get; private set; }
        protected string ConsumerSecretKey { get; private set; }

        public Task<T> GetAsync<T>(string requestUrl)
        {
            SetOAuthHeader(requestUrl, "GET", ConsumerKey, ConsumerSecretKey);

            return Client.GetAsync<T>(requestUrl, LogSuccess, LogError, _logger);
        }

        public Task<T> GetAsync<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            return GetAsync<T>(url);
        }

        public Task<T> PostAsync<T>(string requestUrl, object content)
        {
            SetOAuthHeader(requestUrl, "POST", ConsumerKey, ConsumerSecretKey);

            return Client.PostAsync<T>(requestUrl, content, LogSuccess, LogError, _logger);
        }

        public Task<T> PostAsync<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            return PostAsync<T>(url, request);
        }

        public Task<T> DeleteAsync<T>(string requestUrl, object content)
        {
            SetOAuthHeader(requestUrl, "DELETE", ConsumerKey, ConsumerSecretKey);

            return Client.DeleteAsync<T>(requestUrl, LogSuccess, LogError, _logger);
        }

        public Task<T> DeleteAsync<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();
            return DeleteAsync<T>(url, request);
        }

        private async void LogSuccess(HttpResponseMessage response)
        {
#if DEBUG
            try
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogMessage("CmtPaymentService {0}: {1}", response.RequestMessage.Method.Method, response.RequestMessage.RequestUri);
                _logger.LogMessage("      Response: "+ result);
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    _logger.LogError(ex);
                }
            }
#endif
        }

        private async void LogError(HttpResponseMessage response)
        {
#if DEBUG
            try
            {
                var result = await response.Content.ReadAsStringAsync();

                _logger.LogMessage("Error while calling {0} : {1}" , response.RequestMessage.RequestUri, result);
            }
            catch (Exception ex)
            {
				if (_logger != null)
				{
					_logger.LogError(ex);
				}
            }
#endif
        }
    }
}