using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface.Auth;

namespace apcurium.MK.Booking.Api.Client
{
    public class AuthServiceClient : BaseServiceClient
    {
        public AuthServiceClient(string url, AuthInfo credential)
            : base(url, credential)
        {
        }

        public AuthResponse Authenticate(string email, string password)
        {
            var cookieContainer = new CookieContainer();
            ServiceClientBase.HttpWebRequestFilter = req =>
            {
                req.CookieContainer = cookieContainer;
            };

           return Client.Post<AuthResponse>("/auth/credentials", new Auth
            {
                UserName = email,
                Password = password,
                RememberMe = true,
            });
        }
    }
}
