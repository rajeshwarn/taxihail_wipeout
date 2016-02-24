#region

using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

using apcurium.MK.Common.Configuration;
using System.Web;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Http;

#endregion

namespace apcurium.MK.Booking.Api.Helpers
{
    internal class OrderStatusIbsMock : OrderStatusHelper
    {
        private readonly OrderStatusUpdater _updater;
        private readonly IOrderDao _orderDao;

        private const double DefaultTaxiLatitude = 45.5134;
        private const double DefaultTaxiLongitude = -73.5530;
        private const double NearbyTaxiDelta = 0.0009; // Approx. 100 meters

        public OrderStatusIbsMock(IOrderDao orderDao, OrderStatusUpdater updater, IServerSettings serverSettings)
            : base(orderDao, serverSettings)
        {
            _updater = updater;
            _orderDao = orderDao;
        }

        public override async Task<OrderStatusDetail> GetOrderStatus(Guid orderId, SessionEntity session)
        {
            var orderStatus = await base.GetOrderStatus(orderId, session);

            if (orderStatus.Status == OrderStatus.Completed)
            {
                return orderStatus;
            }

            var ibsInfo = new IBSOrderInformation
            {
                VehicleMake = "Lamborghini",
                VehicleColor = "Red",
                VehicleModel = "Diablo",
                VehicleNumber = "93002",
                VehicleRegistration = "123",
                VehicleType = "Sport",
                FirstName = "Tony",
                LastName = "Apcurium",
                MobilePhone = "5145551234",
                DriverId = "99123",
                TerminalId = "98695",
                ReferenceNumber = "1209",
				DriverPhotoUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/assets/img/tony.jpg",
                Status = VehicleStatuses.Common.Waiting
            };

            var order = _orderDao.FindById(orderId);

            if (order.IBSOrderId.HasValue && order.IBSOrderId != 0)
            {
                ibsInfo.IBSOrderId = order.IBSOrderId.Value;
            }

            if (order.IBSOrderId.HasValue && (!orderStatus.IBSStatusId.HasValue() || orderStatus.IBSStatusId == VehicleStatuses.Common.Waiting))
            {
                ibsInfo.VehicleLatitude = DefaultTaxiLatitude;
                ibsInfo.VehicleLongitude = DefaultTaxiLongitude;

                ibsInfo.Status = VehicleStatuses.Common.Assigned;
            }
            else if (orderStatus.IBSStatusId == VehicleStatuses.Common.Assigned &&
                     orderStatus.VehicleLatitude == DefaultTaxiLatitude &&
                     orderStatus.VehicleLongitude == DefaultTaxiLongitude)
            {
                // Move taxi close to user
                ibsInfo.VehicleLatitude = order.PickupAddress.Latitude - NearbyTaxiDelta;
                ibsInfo.VehicleLongitude = order.PickupAddress.Longitude - NearbyTaxiDelta;

                ibsInfo.Status = VehicleStatuses.Common.Assigned;
            }
            else if (orderStatus.IBSStatusId == VehicleStatuses.Common.Assigned)
            {
                // Move taxi to user position
                ibsInfo.VehicleLatitude = order.PickupAddress.Latitude;
                ibsInfo.VehicleLongitude = order.PickupAddress.Longitude;

                ibsInfo.Status = VehicleStatuses.Common.Arrived;
            }
            else if (orderStatus.IBSStatusId == VehicleStatuses.Common.Arrived)
            {
                ibsInfo.VehicleLatitude = orderStatus.VehicleLatitude;
                ibsInfo.VehicleLongitude = orderStatus.VehicleLongitude;

                ibsInfo.Status = VehicleStatuses.Common.Loaded;
            }
            else if (orderStatus.IBSStatusId == VehicleStatuses.Common.Loaded)
            {
                ibsInfo.VehicleLatitude = orderStatus.VehicleLatitude;
                ibsInfo.VehicleLongitude = orderStatus.VehicleLongitude;

                ibsInfo.Status = VehicleStatuses.Common.Done;
            }

            _updater.Update(ibsInfo, orderStatus);
            return orderStatus;
        }
    }
}