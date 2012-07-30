using System;
using System.Net;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderService : RestServiceBase<OrderRequest>
    {
        private readonly IAccountDao _accountDao;
        protected IOrderDao Dao { get; set; }

        public OrderService(IOrderDao dao, IAccountDao accountDao)
        {
            _accountDao = accountDao;
            Dao = dao;
        }

        public override object OnGet(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            return new OrderMapper().ToResource( orderDetail);
        }
    }
}
