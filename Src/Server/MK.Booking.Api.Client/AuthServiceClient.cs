using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using ServiceStack.ServiceClient.Web;
#if !CLIENT
using ServiceStack.ServiceInterface.Auth;
#else
using ServiceStack.Common.ServiceClient.Web;


#endif

namespace apcurium.MK.Booking.Api.Client
{
    public class AuthServiceClient : BaseServiceClient
    {
        public AuthServiceClient(string url)
            : base(url)
        {
        }

        public AuthResponse Authenticate(string email, string password)
        {
            return Authenticate(new Auth
            {
                UserName = email,
                Password = password,
                RememberMe = true,
            }, "credentials");
        }

        public AuthResponse AuthenticateFacebook(string facebookId)
        {
            return Authenticate(new Auth
            {
                UserName = facebookId,
                Password = facebookId,
                RememberMe = true,
            }, "credentialsfb");
        }

        public AuthResponse AuthenticateTwitter(string twitterId)
        {
            return Authenticate(new Auth
            {
                UserName = twitterId,
                Password = twitterId,
                RememberMe = true,
            }, "credentialstw");
        }

        private AuthResponse Authenticate(Auth auth, string provider)
        {
            var cookieContainer = new CookieContainer();
            ServiceClientBase.HttpWebRequestFilter = req =>
            {
                req.CookieContainer = cookieContainer;
            };

            
            var response = Client.Post<AuthResponse>("/auth/" + provider , auth);


            var cookies = cookieContainer.GetCookies( new Uri(_url ));

            foreach (var item in cookies)
            {
                item.ToString();
            }

            return response;
        }
    }
}
