using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.Api.Services
{
    public class CancelOrderService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
        private readonly Resources.Resources _resources;
        private readonly IServerSettings _serverSettings;
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly ILogger _logger;

        public CancelOrderService(ICommandBus commandBus, 
            IIBSServiceProvider ibsServiceProvider, 
            IOrderDao orderDao, 
            IAccountDao accountDao,
            IUpdateOrderStatusJob updateOrderStatusJob, 
            IServerSettings serverSettings,
            IIbsCreateOrderService ibsCreateOrderService,
            ILogger logger)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _updateOrderStatusJob = updateOrderStatusJob;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
            _ibsCreateOrderService = ibsCreateOrderService;
            _logger = logger;

            _resources = new Resources.Resources(serverSettings);
        }

        public object Post(CancelOrder request)
        {
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (order == null)
            {
                return new HttpResult(HttpStatusCode.NotFound);
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't cancel another account's order");
            }

            if (order.IBSOrderId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey, order.Settings.ServiceType);
                var orderStatus = _orderDao.FindOrderStatusById(order.Id);

                var canCancelWhenPaired = orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Loaded)
                    && _serverSettings.GetPaymentSettings(orderStatus.CompanyKey).CancelOrderOnUnpair;

                if (currentIbsAccountId.HasValue
                    && (!orderStatus.IBSStatusId.HasValue()
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Waiting)
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned)
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Arrived)
                        || orderStatus.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled)
                        || canCancelWhenPaired))
                {
                    _ibsCreateOrderService.CancelIbsOrder(order.IBSOrderId.Value, order.CompanyKey, order.Settings.ServiceType, order.Settings.Phone, account.Id);
                }
                else
                {
                    var errorReason = !currentIbsAccountId.HasValue
                    ? string.Format("no IbsAccountId found for accountid {0} and companykey {1}", account.Id, order.CompanyKey)
                    : string.Format("orderDetail.IBSStatusId is not in the correct state: {0}, state: {1}", orderStatus.IBSStatusId, orderStatus.IBSStatusId);
                    var errorMessage = string.Format("Could not cancel order because {0}", errorReason);

                    _logger.LogMessage(errorMessage);

                    throw new HttpError(HttpStatusCode.BadRequest, _resources.Get("CancelOrderError"), errorMessage);
                }
            }
            else
            {
                _logger.LogMessage("We don't have an ibs order id yet, send a CancelOrder command so that when we receive the ibs order info, we can cancel it");
            }
            
            var command = new Commands.CancelOrder { OrderId = request.OrderId };
            _commandBus.Send(command);

            UpdateStatusAsync(command.OrderId);

            return new HttpResult(HttpStatusCode.OK);
        }

        private void UpdateStatusAsync(Guid orderId)
        {
            new TaskFactory().StartNew(() =>
            {
                //We have to wait for the order to be completed.
                Thread.Sleep(750);
                _updateOrderStatusJob.CheckStatus(orderId);
            });
        }
    }
}