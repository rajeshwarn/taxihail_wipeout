#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using System.Threading;
using log4net;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;
using System.Reactive.Linq;
using System.Diagnostics;
#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class UpdateOrderStatusJob : IUpdateOrderStatusJob
    {
        private readonly IOrderDao _orderDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderStatusUpdateDao _orderStatusUpdateDao;
        private readonly OrderStatusUpdater _orderStatusUpdater;

        private const int NumberOfConcurrentServers = 2;

        public UpdateOrderStatusJob(IOrderDao orderDao, IBookingWebServiceClient bookingWebServiceClient, IOrderStatusUpdateDao orderStatusUpdateDao, OrderStatusUpdater orderStatusUpdater)
        {
            _orderStatusUpdateDao = orderStatusUpdateDao;
            _orderDao = orderDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderStatusUpdater = orderStatusUpdater;
        }

        public void CheckStatus(Guid orderId)
        {
            Func<OrderDetail> getOrderDetails = () =>
            {
                var o = _orderDao.FindById(orderId);
                if (o == null)
                {
                    throw new Exception();
                }
                return o;
            };

            try
            {
                var order = getOrderDetails.Retry(TimeSpan.FromMilliseconds(300), 7);
                if (order != null)
                {
                    var orderStatus = _orderDao.FindOrderStatusById(orderId);
                    var status = _bookingWebServiceClient.GetOrdersStatus( new [] { order.IBSOrderId.Value });
                 
                    _orderStatusUpdater.Update(status.ElementAt(0), orderStatus);
                }
            }
            catch
            {}
        }

        public bool CheckStatus(string updaterUniqueId, int pollingValue)
        {
            var lastUpdate = _orderStatusUpdateDao.GetLastUpdate();
            bool hasOrdersWaitingForPayment = false;
            
            if ((lastUpdate == null) ||
                (lastUpdate.UpdaterUniqueId == updaterUniqueId) ||
                (DateTime.UtcNow.Subtract(lastUpdate.LastUpdateDate).TotalSeconds > NumberOfConcurrentServers * pollingValue))
            {
                // Update LastUpdateDate while processing to block the other instance from starting while we're executing the try block
                var timer = Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(pollingValue))
                                .Subscribe(_ =>_orderStatusUpdateDao.UpdateLastUpdate(updaterUniqueId, DateTime.UtcNow));

                
                try
                {
                    var orders = _orderDao.GetOrdersInProgress().ToArray();

                    BatchUpdateStatus(orders.Where(o => o.Status == OrderStatus.WaitingForPayment));
                    BatchUpdateStatus(orders.Where(o => o.Status == OrderStatus.Pending));
                    BatchUpdateStatus(orders.Where(o => o.Status == OrderStatus.Created));

                    hasOrdersWaitingForPayment = orders.Any(o => o.Status == OrderStatus.WaitingForPayment);
                }
                finally
                {
                    timer.Dispose();
                }
            }
            return hasOrdersWaitingForPayment;
        }

        private void BatchUpdateStatus(IEnumerable<OrderStatusDetail> orders)
        {
            var ibsOrdersIds = orders.Select(statusDetail => statusDetail.IBSOrderId != null ? statusDetail.IBSOrderId.Value : 0).ToList();
            const int take = 10;
            for (var skip = 0; skip < ibsOrdersIds.Count; skip = skip + take)
            {
                var nextGroup = ibsOrdersIds.Skip(skip).Take(take).ToList();
                var orderStatuses = _bookingWebServiceClient.GetOrdersStatus(nextGroup);

                foreach (var ibsStatus in orderStatuses)
                {
                    var order = orders.FirstOrDefault(o => o.IBSOrderId == ibsStatus.IBSOrderId);

                    if (order == null) continue;

                    _orderStatusUpdater.Update(ibsStatus, order);
                }
            }
        }
    }
}