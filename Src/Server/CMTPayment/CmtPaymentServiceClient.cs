using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using ServiceStack.ServiceHost;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using CMTPayment.Authorization;


#if CLIENT
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
#else
using CMTPayment.Extensions;
using ServiceStack.Text;
#endif

namespace CMTPayment
{
    public partial class CmtPaymentServiceClient : BaseServiceClient
    {
        private readonly ILogger _logger;

        public CmtPaymentServiceClient(CmtPaymentSettings cmtSettings, string sessionId, IPackageInfo packageInfo, ILogger logger, IConnectivityService connectivityService)
            : base(cmtSettings.IsSandbox
                ? cmtSettings.SandboxBaseUrl
                : cmtSettings.BaseUrl, sessionId, packageInfo, connectivityService)
        {
            _logger = logger;

            ClientSetup();

            //Client.Proxy = new WebProxy("192.168.12.122", 8888);
            ConsumerKey = cmtSettings.ConsumerKey;
            ConsumerSecretKey = cmtSettings.ConsumerSecretKey;
        }

        partial void ClientSetup();

        protected string ConsumerKey { get; private set; }
        protected string ConsumerSecretKey { get; private set; }
#if CLIENT
        public Task<T> GetAsync<T>(string requestUrl)
        {
            SetOAuthHeader(requestUrl, "GET", ConsumerKey, ConsumerSecretKey);

            return Client.GetAsync<T>(requestUrl, LogSuccess, LogError);
        }

        public Task<T> GetAsync<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            return GetAsync<T>(url);
        }

        public Task<T> PostAsync<T>(string requestUrl, object content)
        {
            SetOAuthHeader(requestUrl, "POST", ConsumerKey, ConsumerSecretKey);

            return Client.PostAsync<T>(requestUrl, content, LogSuccess, LogError);
        }

        public Task<T> PostAsync<T>(IReturn<T> request)
        {
            var url = request.GetUrlFromRoute();

            return PostAsync<T>(url, request);
        }

        public Task<T> DeleteAsync<T>(string requestUrl, object content)
        {
            SetOAuthHeader(requestUrl, "DELETE", ConsumerKey, ConsumerSecretKey);

            return Client.DeleteAsync<T>(requestUrl, LogSuccess, LogError);
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
#else   
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
#endif
    }
}