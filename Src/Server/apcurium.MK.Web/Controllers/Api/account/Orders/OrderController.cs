using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.account
{
    [RoutePrefix("api/account/orders")]
    [Auth]
    public class OrderController : BaseApiController
    {
        private readonly CancelOrderService _cancelOrderService;



        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IPromotionDao _promotionDao;
        private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderRatingsDao _orderRatingsDao;
        private readonly IServerSettings _serverSettings;
        private readonly ITaxiHailNetworkServiceClient _networkServiceClient;
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly Booking.Resources.Resources _resources;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;

        public OrderController(IAccountDao accountDao, IOrderDao orderDao, IOrderPaymentDao orderPaymentDao, IPromotionDao promotionDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider, IOrderRatingsDao orderRatingsDao, IServerSettings serverSettings, ITaxiHailNetworkServiceClient networkServiceClient, IIbsCreateOrderService ibsCreateOrderService, IUpdateOrderStatusJob updateOrderStatusJob)
        {
            _cancelOrderService = new CancelOrderService(commandBus, ibsServiceProvider, orderDao, accountDao, updateOrderStatusJob, serverSettings, networkServiceClient, ibsCreateOrderService, Logger)
            {
                HttpRequestContext = RequestContext,
                Session = GetSession()
            };
        }

        [HttpGet]
        [Route("{orderId}")]
        public IHttpActionResult GetOrder(Guid orderId)
        {
            var orderDetail = _orderDao.FindById(orderId);
            var account = _accountDao.FindById(GetSession().UserId);

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

            return Ok(result);
        }

        [HttpDelete]
        [Route("{orderId}")]
        public object DeleteOrder(Guid orderId)
        {
            var orderDetail = _orderDao.FindById(orderId);
            var account = _accountDao.FindById(GetSession().UserId);

            if (account.Id != orderDetail.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            _commandBus.Send(new RemoveOrderFromHistory { OrderId = orderId });

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("{orderId}/calldriver")]
        public object InitiateCallToDriver(Guid orderId)
        {
            var order = _orderDao.FindById(orderId);
            if (order == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            }

            var status = _orderDao.FindOrderStatusById(orderId);
            if (status == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var account = _accountDao.FindById(GetSession().UserId);
            if (account.Id != order.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't initiate a call with driver of another account's order");
            }

            if (order.IBSOrderId.HasValue
                && status.VehicleNumber.HasValue())
            {
                return _ibsServiceProvider.Booking(order.CompanyKey).InitiateCallToDriver(order.IBSOrderId.Value, status.VehicleNumber);
            }

            return false;
        }

        [HttpPost]
        [Route("{orderId}/cancel")]
        public IHttpActionResult CancelOrder(Guid orderId)
        {
            _cancelOrderService.Post(new Booking.Api.Contract.Requests.CancelOrder {OrderId = orderId});

            return Ok();
        }

    }
}
