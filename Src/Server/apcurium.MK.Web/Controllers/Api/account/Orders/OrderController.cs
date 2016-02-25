using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.Messaging;
using CancelOrder = apcurium.MK.Booking.Api.Contract.Requests.CancelOrder;

namespace apcurium.MK.Web.Controllers.Api.account
{
    [RoutePrefix("api/account/orders")]
    [Auth]
    public class OrderController : BaseApiController
    {
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
            _accountDao = accountDao;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _promotionDao = promotionDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            _orderRatingsDao = orderRatingsDao;
            _serverSettings = serverSettings;
            _networkServiceClient = networkServiceClient;
            _ibsCreateOrderService = ibsCreateOrderService;
            _updateOrderStatusJob = updateOrderStatusJob;

            _resources = new Booking.Resources.Resources(serverSettings);
        }

        [HttpGet]
        [Route("{orderId}")]
        public Order GetOrder(Guid orderId)
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

            return result;
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
        [Route("{OrderId}/calldriver")]
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
        [Route("{OrderId}/cancel")]
        public object CancelOrder(CancelOrder request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(GetSession().UserId);

            if (order == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "Can't cancel another account's order");
            }

            if (order.IBSOrderId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey);
                var orderStatus = _orderDao.FindOrderStatusById(order.Id);

                var marketSettings = _networkServiceClient.GetCompanyMarketSettings(order.PickupAddress.Latitude, order.PickupAddress.Longitude);

                var canCancelWhenPaired = orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded)
                    && marketSettings.DisableOutOfAppPayment;

                if (currentIbsAccountId.HasValue
                    && (!orderStatus.IBSStatusId.HasValue()
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Waiting)
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned)
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Arrived)
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled)
                        || canCancelWhenPaired))
                {
                    _ibsCreateOrderService.CancelIbsOrder(order.IBSOrderId.Value, order.CompanyKey, order.Settings.Phone, account.Id);
                }
                else
                {
                    var errorReason = !currentIbsAccountId.HasValue
                        ? string.Format("no IbsAccountId found for accountid {0} and companykey {1}", account.Id, order.CompanyKey)
                        : string.Format("orderDetail.IBSStatusId is not in the correct state: {0}, state: {1}", orderStatus.IBSStatusId, orderStatus.IBSStatusId);
                    var errorMessage = string.Format("Could not cancel order because {0}", errorReason);

                    Logger.LogMessage(errorMessage);

                    throw new HttpException((int)HttpStatusCode.BadRequest, _resources.Get("CancelOrderError"), new Exception(errorMessage));
                }
            }
            else
            {
                Logger.LogMessage("We don't have an ibs order id yet, send a CancelOrder command so that when we receive the ibs order info, we can cancel it");
            }
            
            var command = new Booking.Commands.CancelOrder { OrderId = request.OrderId };
            _commandBus.Send(command);

            UpdateStatusAsync(command.OrderId);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        private void UpdateStatusAsync(Guid orderId)
        {
            new TaskFactory().StartNew(async () =>
            {
                //We have to wait for the order to be completed.
                await Task.Delay(750);
                _updateOrderStatusJob.CheckStatus(orderId);
            });
        }

    }
}
