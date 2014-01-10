#region

using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using System.Threading.Tasks;
#if !CLIENT
using ServiceStack.ServiceInterface.Auth;
#else
using ServiceStack.Common.ServiceClient.Web;
#endif

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AuthServiceClient : BaseServiceClient, IAuthServiceClient
    {
        public AuthServiceClient(string url, string sessionId, string userAgent)
            : base(url, sessionId, userAgent)
        {
        }

        public Task CheckSession()
        {
            return Client.GetAsync<Account>("/account");
        }

        public Task<AuthenticationData> Authenticate(string email, string password)
        {
			var responseTask = AuthenticateAsync (new Auth {
				UserName = email,
				Password = password,
				RememberMe = true,
			}, "credentials");
			//todo remove when using the async await patten in service layer
			responseTask.Wait ();
			var response = responseTask.Result;
			return Task.FromResult(new AuthenticationData
            {
                UserName = response.UserName,
                SessionId = response.SessionId
			});
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

        public Task<AuthenticationData> AuthenticateTwitter(string twitterId)
        {
			var responseTask = AuthenticateAsync(new Auth
            {
                UserName = twitterId,
                Password = twitterId,
                RememberMe = true,
            }, "credentialstw");
			//todo remove when using the async await patten in service layer
			responseTask.Wait ();
			var response = responseTask.Result;
			return Task.FromResult(new AuthenticationData
				{
					UserName = response.UserName,
					SessionId = response.SessionId
				});
        }

        private Task<AuthResponse> AuthenticateAsync(Auth auth, string provider)
		{
			return Client.PostAsync<AuthResponse>("/auth/" + provider , auth);
		}
    }
}