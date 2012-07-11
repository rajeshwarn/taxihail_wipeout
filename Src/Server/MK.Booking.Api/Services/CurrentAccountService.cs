using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Services
{
    public class CurrentAccountService : RestServiceBase<CurrentAccount>
    {

        public CurrentAccountService(IAccountDao dao)
        {
            Dao = dao;
        }

        protected IAccountDao Dao { get; set; }


        public override object OnGet(CurrentAccount request)
        {
            var session = this.GetSession() ;
            var account = Dao.FindById( new Guid( session.UserAuthId) );
            account.Settings.ToString(); 
            return account;
        }
    }
}
