#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class UpdateOrderStatusJob : IUpdateOrderStatusJob
    {
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly IOrderDao _orderDao;
        private readonly OrderStatusUpdater _orderStatusUpdater;

        public UpdateOrderStatusJob(IOrderDao orderDao, IBookingWebServiceClient bookingWebServiceClient,
            OrderStatusUpdater orderStatusUpdater)
        {
            _orderDao = orderDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderStatusUpdater = orderStatusUpdater;
        }

        public void CheckStatus()
        {
            var orders = _orderDao.GetOrdersInProgress();


            var ibsOrdersIds = orders
                .Where(
                    statusDetail =>
                        statusDetail.PickupDate >= DateTime.Now.AddDays(-1) ||
                        statusDetail.IBSStatusId == VehicleStatuses.Common.Scheduled)
                .Where(statusDetail => statusDetail.IBSOrderId.HasValue)
                .Select(statusDetail => statusDetail.IBSOrderId != null ? statusDetail.IBSOrderId.Value : 0)
                .ToList();


            var ibsOrders = new List<IBSOrderInformation>();

            const int take = 5;
            for (var skip = 0; skip < ibsOrdersIds.Count; skip = skip + take)
            {
                var nextGroup = ibsOrdersIds.Skip(skip).Take(take).ToList();
                ibsOrders.AddRange(_bookingWebServiceClient.GetOrdersStatus(nextGroup));
            }

            foreach (var order in orders)
            {
                var ibsStatus = ibsOrders.FirstOrDefault(status => status.IBSOrderId == order.IBSOrderId);

                if (ibsStatus == null) continue;

                _orderStatusUpdater.Update(ibsStatus, order);
            }
        }
    }
}