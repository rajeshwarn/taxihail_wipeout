#region

using apcurium.MK.Booking.Api.Contract.Security;
using System.Threading.Tasks;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IAuthServiceClient
    {
        void CheckSession();
		AuthenticationData Authenticate(string email, string password);
        AuthenticationData AuthenticateTwitter(string twitterId);
		Task<AuthenticationData> AuthenticateFacebook (string facebookId);
    }
}