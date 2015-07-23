using System;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IMetricsService
    {
        void LogApplicationStartUp();

        void LogOriginalRideEta(Guid orderId, long? originalEta);
    }
}