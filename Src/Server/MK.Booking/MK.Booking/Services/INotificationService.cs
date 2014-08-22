using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Services
{
    public interface INotificationService
    {
        void SendStatusChangedNotification(OrderStatusDetail orderStatusDetail);
        void SendPaymentCaptureNotification(Guid orderId, decimal amount);
        void SendTaxiNearbyNotification(Guid orderId, string ibsStatus, double? newLatitude, double? newLongitude);
    }
}