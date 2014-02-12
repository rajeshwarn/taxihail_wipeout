#region

using System;
using apcurium.MK.Booking.Api.Jobs;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceInterface.Auth;

#endregion

namespace apcurium.MK.Booking.Api.Helpers
{
    internal class OrderStatusIbsMock : OrderStatusHelper
    {
        private readonly OrderStatusUpdater _updater;

        public OrderStatusIbsMock(IOrderDao orderDao, OrderStatusUpdater updater)
            : base(orderDao)
        {
            _updater = updater;
        }

        public override OrderStatusDetail GetOrderStatus(Guid orderId, IAuthSession session)
        {
            var orderStatus = base.GetOrderStatus(orderId, session);

            var ibsInfo = new IBSOrderInformation
            {
                IBSOrderId = 9999,
                VehicleMake = "FakeMake",
                VehicleColor = "FakeColor",
                VehicleModel = "FakeModel",
                VehicleNumber = "FakeNumber",
                VehicleRegistration = "Fake",
                VehicleType = "Fake",
                FirstName = "FakeName",
                LastName = "FakeName",
                MobilePhone = "FakePhone",
                DriverId = "FakeDriverId",
                TerminalId = "FakeTerminalId",
                ReferenceNumber = "FakeReferenceNumber",
                VehicleLatitude = 45.5134,
                VehicleLongitude = -73.5530
            };
            switch (orderStatus.IBSStatusId)
            {
                case null:
                case "":
                    ibsInfo.Status = VehicleStatuses.Common.Assigned;
                    break;
                case VehicleStatuses.Common.Assigned:
                    ibsInfo.Status = VehicleStatuses.Common.Arrived;
                    break;
                case VehicleStatuses.Common.Arrived:
                    ibsInfo.Status = VehicleStatuses.Common.Loaded;
                    break;
                case VehicleStatuses.Common.Loaded:
                    ibsInfo.Status = VehicleStatuses.Common.Done;
                    break;
            }
            _updater.Update(ibsInfo, orderStatus);
            return orderStatus;
        }
    }
}