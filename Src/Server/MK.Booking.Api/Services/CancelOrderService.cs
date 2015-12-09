#region

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common;
using apcurium.MK.Common.Diagnostic;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
	public class CancelOrderService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ICommandBus _commandBus;
        private readonly IOrderDao _orderDao;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
        private readonly ILogger _logger;
		private readonly Resources.Resources _resources;

		public CancelOrderService(ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider, IOrderDao orderDao, IAccountDao accountDao, 
            IUpdateOrderStatusJob updateOrderStatusJob, IServerSettings serverSettings, ILogger logger)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _updateOrderStatusJob = updateOrderStatusJob;
            _commandBus = commandBus;
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
				var currentIbsAccountId = _accountDao.GetIbsAccountId(account.Id, order.CompanyKey);
				var orderDetail = _orderDao.FindOrderStatusById(order.Id);

				if (currentIbsAccountId.HasValue
					&& (!orderDetail.IBSStatusId.HasValue()
					|| orderDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Waiting)
					|| orderDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Assigned)
					|| orderDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Arrived)
					|| orderDetail.IBSStatusId.SoftEqual(VehicleStatuses.Common.Scheduled)))
				{
					// We need to try many times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride... after 5 time, we are giving up. But we assume the order is completed.
					Task.Factory.StartNew(() => {
						Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(order.CompanyKey).CancelOrder(order.IBSOrderId.Value, currentIbsAccountId.Value, order.Settings.Phone);
						cancelOrder.Retry(new TimeSpan(0, 0, 0, 10), 5);
					});

					var command = new Commands.CancelOrder { OrderId = request.OrderId };
					_commandBus.Send(command);

					UpdateStatusAsync(command.OrderId);

					return new HttpResult(HttpStatusCode.OK);
				}

                var errorReason = !currentIbsAccountId.HasValue
                    ? string.Format("no IbsAccountId found for accountid {0} and companykey {1}", account.Id, order.CompanyKey)
                    : string.Format("orderDetail.IBSStatusId is not in the correct state: {0}", orderDetail.IBSStatusId);
                var errorMessage = string.Format("Could not cancel order because {0}", errorReason);

                _logger.LogMessage(errorMessage);

                throw new HttpError(HttpStatusCode.BadRequest, _resources.Get("CancelOrderError"), errorMessage);
			}

			return new HttpResult(HttpStatusCode.BadRequest, _resources.Get("CancelOrderError_NoIBSOrderId"));
		}

		private void UpdateStatusAsync(Guid id)
        {
            new TaskFactory().StartNew(() =>
            {
                //We have to wait for the order to be completed.
                Thread.Sleep(750);
                _updateOrderStatusJob.CheckStatus(id);
            });
        }
    }
}