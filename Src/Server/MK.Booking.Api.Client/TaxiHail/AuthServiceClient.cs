using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;

#if !CLIENT
#else
using ServiceStack.Common.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AuthServiceClient : BaseServiceClient, IAuthServiceClient
    {
        public AuthServiceClient(string url, string sessionId)
            : base(url, sessionId)
        {
        }

        public void CheckSession()
        {
            Client.Get<Account>("/account");
        }

        public AuthenticationData Authenticate(string email, string password)
        {
            var response =  Authenticate(new Auth
            {
                UserName = email,
                Password = password,
                RememberMe = true,
            }, "credentials");
            return new AuthenticationData
            {
                ReferrerUrl = response.ReferrerUrl,
                UserName = response.UserName,
                SessionId = response.SessionId
            };
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
            
            var response = Client.Post<AuthResponse>("/auth/" + provider , auth);
             
            return response;
        }
    }
}
