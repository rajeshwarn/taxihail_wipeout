#region

using System;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderService : BaseApiService
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IPromotionDao _promotionDao;

        public OrderService(IOrderDao dao, IOrderPaymentDao orderPaymentDao, IPromotionDao promotionDao, IAccountDao accountDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _orderPaymentDao = orderPaymentDao;
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            Dao = dao;
        }

        protected IOrderDao Dao { get; set; }

        public Order Get(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(Session.UserId);

            if (orderDetail == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Order Not Found");
            }

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            var payment = _orderPaymentDao.FindByOrderId(orderDetail.Id, orderDetail.CompanyKey);
            if (payment != null && !payment.IsCancelled && payment.IsCompleted)
            {
                orderDetail.Fare = Convert.ToDouble(payment.Meter);
                orderDetail.Toll = 0;
                orderDetail.Tip = Convert.ToDouble(payment.Tip);
            }
            
            var result = new OrderMapper().ToResource(orderDetail);

            var promoUsed = _promotionDao.FindByOrderId(orderDetail.Id);
            if (promoUsed != null)
            {
                result.PromoCode = promoUsed.Code;
            }

            return result;
        }

        public bool Get(InitiateCallToDriverRequest request)
        {
            var order = Dao.FindById(request.OrderId);
            if (order == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Order not found");
            }

            var status = Dao.FindOrderStatusById(request.OrderId);
            if (status == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Order status not found");
            }

            var account = _accountDao.FindById(Session.UserId);
            if (account.Id != order.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't initiate a call with driver of another account's order");
            }

            if (order.IBSOrderId.HasValue && status.VehicleNumber.HasValue())
            {
                return _ibsServiceProvider.Booking(order.CompanyKey).InitiateCallToDriver(order.IBSOrderId.Value, status.VehicleNumber);
            }

            return false;
        }

        public void Delete(OrderRequest request)
        {
            var orderDetail = Dao.FindById(request.OrderId);
            var account = _accountDao.FindById(Session.UserId);

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            _commandBus.Send(new RemoveOrderFromHistory {OrderId = request.OrderId});
        }
    }
}