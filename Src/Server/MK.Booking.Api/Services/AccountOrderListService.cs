using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class AccountOrderListService : RestServiceBase<AccountOrderListRequest>
    {
        public AccountOrderListService(IOrderDao dao)
        {
            Dao = dao;
        }

        protected IOrderDao Dao { get; set; }

        public override object OnGet(AccountOrderListRequest request)
        {
            if (!request.AccountId.Equals(new Guid(this.GetSession().UserAuthId)))
            {
                throw HttpError.Unauthorized("Unauthorized");
            }

            var session = this.GetSession();
            return Dao.FindByAccountId(new Guid(session.UserAuthId)).OrderByDescending(c=>c.PickupDate);
        }
    }
}
