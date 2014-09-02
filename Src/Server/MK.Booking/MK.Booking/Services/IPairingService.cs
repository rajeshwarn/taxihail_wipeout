using System;

namespace apcurium.MK.Booking.Services
{
    public interface IPairingService
    {
        void Pair(Guid orderId, string cardToken, int? autoTipPercentage);
        void Unpair(Guid orderId);
    }
}