using System;
namespace apcurium.MK.Booking.Api.Jobs
{
    public interface IUpdateOrderStatusJob
    {
        
        bool CheckStatus(string uniqueId, int pollingValue);

        void CheckStatus(Guid orderId);
    }
}