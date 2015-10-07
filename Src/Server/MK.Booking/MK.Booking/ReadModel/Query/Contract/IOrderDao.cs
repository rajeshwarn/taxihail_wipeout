#region

using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOrderDao
    {
        IList<OrderDetail> GetAll();
        OrderDetail FindById(Guid id);
        IList<OrderDetail> FindByAccountId(Guid id);
        IList<OrderStatusDetail> GetOrdersInProgress();
        IList<OrderStatusDetail> GetOrdersInProgressByAccountId(Guid accountId);
        OrderStatusDetail FindOrderStatusById(Guid orderId);
        OrderPairingDetail FindOrderPairingById(Guid orderId);
        void UpdateVehiclePosition(Guid orderId, double? newLatitude, double? newLongitude);
        IEnumerable<Position> GetVehiclePositions(Guid orderId);
        TemporaryOrderCreationInfoDetail GetTemporaryInfo(Guid orderId);
        TemporaryOrderPaymentInfoDetail GetTemporaryPaymentInfo(Guid orderId);
        void DeleteTemporaryPaymentInfo(Guid orderId);
        OrderManualRideLinqDetail GetManualRideLinqById(Guid orderId);
		OrderManualRideLinqDetail GetCurrentManualRideLinq(string pairingCode, Guid accountId);
    }
}