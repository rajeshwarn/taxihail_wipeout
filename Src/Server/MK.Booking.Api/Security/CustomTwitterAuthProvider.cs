#region

using System.Collections.Generic;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;

#endregion

namespace apcurium.MK.Booking.Api.Security
{
    public class CustomTwitterAuthProvider : CredentialsAuthProvider
    {
        public CustomTwitterAuthProvider(IAccountDao dao)
        {
            Dao = dao;
            Provider = "credentialstw";
        }

        protected IAccountDao Dao { get; set; }

        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            var account = Dao.FindByTwitterId(userName);
            return (account != null);
        }

        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens,
            Dictionary<string, string> authInfo)
        {
            var account = Dao.FindByTwitterId(session.UserAuthName);
            session.UserAuthId = account.Id.ToString();
            session.IsAuthenticated = true;
            authService.SaveSession(session, SessionExpiry);
        }
    }
}