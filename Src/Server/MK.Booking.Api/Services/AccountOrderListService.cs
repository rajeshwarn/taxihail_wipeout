using System;
using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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
            var session = this.GetSession();
            var orders = Dao.FindByAccountId(new Guid(session.UserAuthId))
                .Where(x=> !x.IsRemovedFromHistory)
                .OrderByDescending(c => c.CreatedDate)
                .Select(read => new OrderMapper().ToResource(read));

            var response = new AccountOrderListRequestResponse();
            response.AddRange(orders);
            return response;
        }
    }
}
