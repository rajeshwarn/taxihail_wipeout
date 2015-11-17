#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Mobile.Infrastructure;
#if !CLIENT
using ServiceStack.ServiceInterface.Auth;
#endif

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AuthServiceClient : BaseServiceClient, IAuthServiceClient
    {
        public AuthServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task CheckSession()
        {
            return Client.GetAsync<Account>("/account");
        }

        public async Task<AuthenticationData> Authenticate(string email, string password)
        {
            var response = await AuthenticateAsync (new Auth {
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

        public async Task<AuthenticationData> AuthenticateTwitter(string twitterId)
        {
            var response = await AuthenticateAsync(new Auth
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

        private Task<AuthResponse> AuthenticateAsync(Auth auth, string provider)
		{
			return Client.PostAsync<AuthResponse>("/auth/" + provider , auth);
		}
    }
}