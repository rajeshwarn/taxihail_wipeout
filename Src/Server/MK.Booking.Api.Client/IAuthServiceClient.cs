#region

using apcurium.MK.Booking.Api.Contract.Security;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IAuthServiceClient
    {
        void CheckSession();
        AuthenticationData Authenticate(string email, string password);
        AuthenticationData AuthenticateTwitter(string twitterId);
        AuthenticationData AuthenticateFacebook(string facebookId);
    }
}