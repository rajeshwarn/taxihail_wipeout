using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using System.Threading.Tasks;
using ServiceStack.ServiceClient.Web;
#if !CLIENT
using ServiceStack.ServiceInterface.Auth;
#else
using ServiceStack.ServiceClient.Web;
using ServiceStack.Common.ServiceClient.Web;
#endif

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AuthServiceClient : BaseServiceClient, IAuthServiceClient
    {
        public AuthServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId,userAgent)
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
                UserName = response.UserName,
                SessionId = response.SessionId
            };
        }

		public async Task<AuthenticationData> AuthenticateFacebook(string facebookId)
		{
			var response = await AuthenticateAsync(new Auth
				{
					UserName = facebookId,
					Password = facebookId,
					RememberMe = true,
				}, "credentialsfb")
				.ConfigureAwait(false);

			return new AuthenticationData
			{
				UserName = response.UserName,
				SessionId = response.SessionId
			};
		}

        public AuthenticationData AuthenticateTwitter(string twitterId)
        {
            var response = Authenticate(new Auth
            {
                UserName = twitterId,
                Password = twitterId,
                RememberMe = true,
            }, "credentialstw");

            return new AuthenticationData
            {
                UserName = response.UserName,
                SessionId = response.SessionId
            };
        }

        private AuthResponse Authenticate(Auth auth, string provider)
        {
            var response = Client.Post<AuthResponse>("/auth/" + provider , auth);
            return response;
        }

		private Task<AuthResponse> AuthenticateAsync(Auth auth, string provider)
		{
			return Client.PostAsync<AuthResponse>("/auth/" + provider , auth);
		}


    }
}
