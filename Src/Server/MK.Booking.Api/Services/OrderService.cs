using System;
using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderService : RestServiceBase<OrderRequest>
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        protected IOrderDao Dao { get; set; }

        public OrderService(IOrderDao dao, IAccountDao accountDao, ICommandBus commandBus)
        {
            _accountDao = accountDao;
            _commandBus = commandBus;
            Dao = dao;
        }

        public override object OnGet(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if(orderDetail == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Order Not Found");
            }

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            return new OrderMapper().ToResource( orderDetail);
        }

        public override object OnDelete(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            _commandBus.Send(new RemoveOrderFromHistory() { OrderId = request.OrderId });

            return HttpStatusCode.Accepted;
        }
    }
}
