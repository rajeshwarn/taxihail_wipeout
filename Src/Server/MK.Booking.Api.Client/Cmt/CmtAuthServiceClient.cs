#region

using System;
using apcurium.MK.Booking.Api.Contract.Security;

#endregion

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtAuthServiceClient : CmtBaseServiceClient, IAuthServiceClient
    {
        public CmtAuthServiceClient(string url) : base(url, null)
        {
        }

        public CmtAuthServiceClient(string url, CmtAuthCredentials credentials)
            : base(url, credentials)
        {
        }

        public void CheckSession()
        {
            throw new NotImplementedException();
        }

        #region IAuthServiceClient implementation

        public AuthenticationData Authenticate(string email, string password)
        {
            var response =
                Client.Get<CmtAuthResponse>(string.Format("/auth?emailAddress={0}&password={1}", email, password));
            return new AuthenticationData
            {
                SessionId = response.SessionToken,
                AccessTokenSecret = response.AccessTokenSecret,
                AccessToken = response.AccessToken,
                AccountId = response.AccountId
            };
        }

        public AuthenticationData AuthenticateTwitter(string twitterId)
        {
            throw new NotImplementedException();
        }

        public AuthenticationData AuthenticateFacebook(string twitterId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}