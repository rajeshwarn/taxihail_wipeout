using System;
using System.Linq;
using System.Reactive.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using ServiceStack.Logging;

namespace apcurium.MK.Booking.Jobs
{
    /// <summary>
    ///     This is a replacement for UpdateOrderStatus job used when IBS order updates
    ///     are faked using OrderStatusIbsMock
    /// </summary>
    internal class UpdateOrderStatusJobStub : IUpdateOrderStatusJob
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateOrderStatusJob));
        private readonly IOrderDao _orderDao;
        private readonly IOrderStatusUpdateDao _orderStatusUpdateDao;
        private readonly OrderStatusUpdater _orderStatusUpdater;

        private const int NumberOfConcurrentServers = 2;

        public UpdateOrderStatusJobStub(IOrderDao orderDao, IOrderStatusUpdateDao orderStatusUpdateDao, OrderStatusUpdater orderStatusUpdater)
        {
            _orderDao = orderDao;
            _orderStatusUpdateDao = orderStatusUpdateDao;
            _orderStatusUpdater = orderStatusUpdater;
        }


        public bool CheckStatus(string uniqueId, int pollingValue)
        {
            Log.WarnFormat("FAKE IBS ACTIVE. only manual RideLinq orders will be processed");

            var lastUpdate = _orderStatusUpdateDao.GetLastUpdate();

            if (lastUpdate != null
                && lastUpdate.UpdaterUniqueId != uniqueId
                && !(DateTime.UtcNow.Subtract(lastUpdate.LastUpdateDate).TotalSeconds > NumberOfConcurrentServers * pollingValue))
            {
                return false;
            }

            var cycleStartDateTime = DateTime.UtcNow;

            // Update LastUpdateDate while processing to block the other instance from starting while we're executing the try block
            var timer = Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(pollingValue))
                .Subscribe(_ => _orderStatusUpdateDao.UpdateLastUpdate(uniqueId, DateTime.UtcNow, cycleStartDateTime));

            try
            {
                var orders = _orderDao.GetOrdersInProgress(true)
                    .Where(x => x.Status == OrderStatus.Created);

                foreach (var orderStatusDetail in orders)
                {
                    Log.InfoFormat("Starting OrderStatusUpdater for order {0} (Paired via Manual RideLinQ code).", orderStatusDetail.OrderId);
                    _orderStatusUpdater.HandleManualRidelinqFlow(orderStatusDetail);
                }
            }
            finally
            {
                timer.Dispose();
            }

            // No op
            return false;
        }


        public void CheckStatus(Guid orderId)
        {
            Log.WarnFormat("FAKE IBS ACTIVE");
            // No op
        }
    }
}