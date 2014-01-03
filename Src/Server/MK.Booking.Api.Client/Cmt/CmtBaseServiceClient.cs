#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Client.Client;
using apcurium.MK.Booking.Api.Client.Cmt.OAuth;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;

#endregion

#if CLIENT
using ServiceStack.Common.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtBaseServiceClient
    {
        protected readonly CmtAuthCredentials Credentials;
        private readonly string _url;
        private ServiceClientBase _client;

        public CmtBaseServiceClient(string url) : this(url, null)
        {
        }

        public CmtBaseServiceClient(string url, CmtAuthCredentials cmtAuthCredentials)
        {
            _url = url;
            Credentials = cmtAuthCredentials;
        }

        protected ServiceClientBase Client
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

        private ServiceClientBase CreateClient()
        {
            JsConfig.DateHandler = JsonDateHandler.ISO8601;
            var client = new CmtJsonServiceClient(_url)
            {
                Timeout = new TimeSpan(0, 0, 0, 20, 0),
                LocalHttpWebRequestFilter = SignRequest
            };
            return client;
        }

        private void SignRequest(HttpWebRequest request)
        {
            if (Credentials != null)
            {
                request.Headers.Add("X-CMT-SessionToken", Credentials.SessionId);

                var oauthHeader = OAuthAuthorizer.AuthorizeRequest(Credentials.ConsumerKey,
                    Credentials.ConsumerSecret,
                    Credentials.AccessToken,
                    Credentials.AccessTokenSecret,
                    request.Method,
                    request.RequestUri,
                    null);
                request.Headers.Add(HttpRequestHeader.Authorization, oauthHeader);
            }
        }
    }
}