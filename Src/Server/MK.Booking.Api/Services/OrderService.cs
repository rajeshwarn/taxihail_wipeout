#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IOrderPaymentDao _orderPaymentDao;

        public OrderService(IOrderDao dao, IOrderPaymentDao orderPaymentDao, IAccountDao accountDao, ICommandBus commandBus)
        {
            _orderPaymentDao = orderPaymentDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
            Dao = dao;
        }

        protected IOrderDao Dao { get; set; }

        public object Get(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (orderDetail == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Order Not Found");
            }

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            var payment = _orderPaymentDao.FindByOrderId(orderDetail.Id);
            if ( (payment != null) && (!payment.IsCancelled ) && (payment.IsCompleted ))
            {
                orderDetail.Fare = Convert.ToDouble( payment.Amount);
                orderDetail.Toll = 0;
                orderDetail.Tip = Convert.ToDouble( payment.Tip);
            }

            return new OrderMapper().ToResource(orderDetail);
        }

        public object Delete(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            _commandBus.Send(new RemoveOrderFromHistory {OrderId = request.OrderId});

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}