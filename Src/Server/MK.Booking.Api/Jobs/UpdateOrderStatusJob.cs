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

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class UpdateOrderStatusJob : IUpdateOrderStatusJob
    {
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly OrderStatusUpdater _orderStatusUpdater;

        public UpdateOrderStatusJob(IOrderDao orderDao, IAccountDao accountDao, IBookingWebServiceClient bookingWebServiceClient, OrderStatusUpdater orderStatusUpdater)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderStatusUpdater = orderStatusUpdater;
        }

        public void CheckStatus(Guid orderId)
        {

            var order = _orderDao.FindById(orderId);

            while (order == null)
            {
                order = _orderDao.FindById(orderId);
                Thread.Sleep(100);
            }

            var orderStatus = _orderDao.FindOrderStatusById(orderId);
            var account = _accountDao.FindById(order.AccountId);
            var status = _bookingWebServiceClient.GetOrderStatus(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

            var ibsStatus = new IBSOrderInformation
            {
                Status = status.Status,
                IBSOrderId = order.IBSOrderId.Value,
            };


            _orderStatusUpdater.Update(ibsStatus, orderStatus);
        }


        public void CheckStatus()
        {

            var orders = _orderDao.GetOrdersInProgress().ToArray();

            BatchUpdateStatus(orders.Where(o => o.Status == OrderStatus.Pending));

            BatchUpdateStatus(orders.Where(o => o.Status == OrderStatus.Created));

            BatchUpdateStatus(orders.Where(o => (o.Status != OrderStatus.Pending) && (o.Status != OrderStatus.Created)));

        }

        public void BatchUpdateStatus(IEnumerable<OrderStatusDetail> orders)
        {
            var ibsOrders = new List<IBSOrderInformation>();
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