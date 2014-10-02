#region

using System;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Common.Configuration;

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

        public OrderStatusIbsMock(IOrderDao orderDao, OrderStatusUpdater updater, IConfigurationManager configManager, IAppSettings appSettings)
            : base(orderDao, configManager, appSettings)
        {
            _updater = updater;
            _orderDao = orderDao;
        }

        public override OrderStatusDetail GetOrderStatus(Guid orderId, IAuthSession session)
        {
            var orderStatus = base.GetOrderStatus(orderId, session);

            if (orderStatus.Status == OrderStatus.Completed)
            {
                return orderStatus;
            }

            var ibsInfo = new IBSOrderInformation
            {
                IBSOrderId = 9999,
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
                ReferenceNumber = "1209"
            };

            var order = _orderDao.FindById(orderId);

            if (string.IsNullOrEmpty(orderStatus.IBSStatusId) || orderStatus.IBSStatusId == VehicleStatuses.Common.Waiting)
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