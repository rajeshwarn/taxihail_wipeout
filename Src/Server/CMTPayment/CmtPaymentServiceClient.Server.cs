using System;
using System.IO;
using System.Net;
using apcurium.MK.Common.Extensions;
using CMTPayment.Authorization;
using ServiceStack.Common.Web;
using ServiceStack.Text;

namespace CMTPayment
{
    public partial class CmtPaymentServiceClient
    {
        partial void ClientSetup()
        {
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;
            Client.LocalHttpWebResponseFilter = LogErrorBody;
        }

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


            _logger.Maybe(() => _logger.LogMessage("CMT request header info : " + request.Headers));
            _logger.Maybe(() => _logger.LogMessage("CMT request info : " + request.ToJson()));
        }

        private void LogErrorBody(HttpWebResponse response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return;
            }

            _logger.LogMessage("CMT Response Status Code : " + response.StatusCode);
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                _logger.LogMessage("CMT Response Body : " + reader.ReadToEnd());
            }
        }

        
    }
}
