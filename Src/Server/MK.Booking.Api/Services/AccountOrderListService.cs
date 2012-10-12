using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Text.Common;

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
            var session = this.GetSession();
            var orders = Dao.FindByAccountId(new Guid(session.UserAuthId)).OrderByDescending(c => c.CreatedDate).Select(read => new OrderMapper().ToResource(read)); 
            var o =orders.ToArray();
            return o;
            
        }
    }
}
