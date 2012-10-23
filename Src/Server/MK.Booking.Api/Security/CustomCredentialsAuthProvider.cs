using System.Collections.Generic;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Security;

namespace apcurium.MK.Booking.Api.Security
{
    public class CustomCredentialsAuthProvider : CredentialsAuthProvider
    {
        private readonly IPasswordService _passwordService;

        public CustomCredentialsAuthProvider(IAccountDao dao, IPasswordService passwordService)
        {
            _passwordService = passwordService;
            Dao = dao;
        }

        protected IAccountDao Dao { get; set; }

        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            var account = Dao.FindByEmail(userName);

            return (account != null) 
                && account.IsConfirmed 
                && _passwordService.IsValid(password, account.Id.ToString(), account.Password);
        }
        
        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            var account = Dao.FindByEmail(session.UserAuthName);
            session.UserAuthId = account.Id.ToString();
            session.IsAuthenticated = true;
            if(account.IsAdmin)
            {
                session.Permissions = new List<string> { Permissions.Admin };
            }
            authService.SaveSession(session, SessionExpiry);
        }
    }

}
