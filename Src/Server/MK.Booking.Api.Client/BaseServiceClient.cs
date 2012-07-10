using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ServiceStack.ServiceClient.Web;
#if !CLIENT
using ServiceStack.ServiceInterface.Auth;
#else
using ServiceStack.Common.ServiceClient.Web;
#endif
namespace apcurium.MK.Booking.Api.Client
{
    public class BaseServiceClient
    {
        private CookieContainer _cookieContainer;
        private ServiceClientBase _client;
        private readonly string _url;
        private AuthInfo _credential;
        private bool _isSecured;
        private AuthResponse _authToken;
        public BaseServiceClient(string url, AuthInfo credential)
        {
            _url = url;
            _credential = credential;
            _isSecured = credential != null;
            _cookieContainer = new CookieContainer();
        }

        protected ServiceClientBase Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new JsonServiceClient(_url);
                    ServiceClientBase.HttpWebRequestFilter = req =>
                    {
                        req.CookieContainer = _cookieContainer;
                    };

                    if (_isSecured)
                    {
                        
                        _authToken = _client.Send<AuthResponse>(new Auth
                        {
                            UserName = _credential.Email,
                            Password = _credential.Password,
                            RememberMe = true,
                            provider = "credentials", // CredentialsAuthProvider.Name not supported on android
                        });
                    }


                }
                return _client;
            }
        }



    }
}
