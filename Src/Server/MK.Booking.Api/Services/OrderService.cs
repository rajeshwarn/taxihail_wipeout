#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
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


        public object Post(GratuityRequest request)
        {
            var order = Dao.FindById(request.OrderId);
            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            var status = Dao.FindOrderStatusById(request.OrderId);
            if (status == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't initiate a call with driver of another account's order");
            }

            if (status.ServiceType == ServiceType.Taxi)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't pay a gratuity for taxi service");
            }

            // Here, pay gratuity using payment provider. Event should be raised if payment is successful.

            _commandBus.Send(new PayGratuity
            {
                OrderId = request.OrderId,
                Percentage = request.Percentage
            });

            return new HttpResult(HttpStatusCode.OK);
        }

        public object Get(InitiateCallToDriverRequest request)
        {
            var order = Dao.FindById(request.OrderId);
            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            var status = Dao.FindOrderStatusById(request.OrderId);
            if (status == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));
            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't initiate a call with driver of another account's order");
            }

            if (order.IBSOrderId.HasValue
                && status.VehicleNumber.HasValue())
            {
                return _ibsServiceProvider.Booking(order.CompanyKey, order.Settings.ServiceType).InitiateCallToDriver(order.IBSOrderId.Value, status.VehicleNumber);
            }

            return false;
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