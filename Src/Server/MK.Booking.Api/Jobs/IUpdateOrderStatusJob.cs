using System;
namespace apcurium.MK.Booking.Api.Jobs
{
    public interface IUpdateOrderStatusJob
    {
        void CheckStatus();

        void CheckStatus(Guid orderId);
    }
}