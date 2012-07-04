using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Security
{


    public class CustomCredentialsAuthProvider : CredentialsAuthProvider
    {
        

            
        public CustomCredentialsAuthProvider(IAccountDao dao)
        {
            Dao = dao;
        }

        protected IAccountDao Dao { get; set; }

        public override bool TryAuthenticate(IServiceBase authService, string userName, string password)
        {
            var account = Dao.FindByEmail(userName);
            
            return ( account != null) && ( account.Password == password );
        }



        public override void OnAuthenticated(IServiceBase authService, IAuthSession session, IOAuthTokens tokens, Dictionary<string, string> authInfo)
        {
            var account = Dao.FindByEmail(session.UserAuthName);
            session.UserAuthId = account.Id.ToString();
            authService.SaveSession(session, SessionExpiry);
        }
    }

}
