using System;
namespace apcurium.MK.Booking.Api.Jobs
{
    public interface IUpdateOrderStatusJob
    {
        
        void CheckStatus(string uniqueId);

        void CheckStatus(Guid orderId);
    }
}