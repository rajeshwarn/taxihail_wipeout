#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Contract.Security;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using MK.Common.DummyServiceStack;
#if CLIENT
using apcurium.MK.Common.Extensions;
#endif

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class AuthServiceClient : BaseServiceClient, IAuthServiceClient
    {
        public AuthServiceClient(string url, string sessionId, IPackageInfo packageInfo, IConnectivityService connectivityService, ILogger logger)
            : base(url, sessionId, packageInfo, connectivityService, logger)
        {
        }

        public Task CheckSession()
        {
            return Client.GetAsync<Account>("/accounts", logger: Logger);
        }

        public async Task<AuthenticationData> Authenticate(string email, string password)
        {
            var response = await AuthenticateAsync (new Auth {
				UserName = email,
				Password = password,
				RememberMe = true,
			}, "password");
			
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
				}, "facebook")
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
            }, "twitter");
			

            return new AuthenticationData
            {
                UserName = response.UserName,
                SessionId = response.SessionId
            };
        }

        private Task<AuthResponse> AuthenticateAsync(Auth auth, string provider)
		{
            return Client.PostAsync<AuthResponse>("/auth/login/" + provider , auth, logger: Logger);
		}
    }
}