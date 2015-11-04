using System;

namespace apcurium.MK.Booking.Jobs
{
    public interface IUpdateOrderStatusJob
    {
        
        bool CheckStatus(string uniqueId, int pollingValue);

        void CheckStatus(Guid orderId);
    }
}