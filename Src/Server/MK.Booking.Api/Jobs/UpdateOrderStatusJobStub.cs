using System;
namespace apcurium.MK.Booking.Api.Jobs
{
    /// <summary>
    ///     This is a replacement for UpdateOrderStatus job used when IBS order updates
    ///     are faked using OrderStatusIbsMock
    /// </summary>
    internal class UpdateOrderStatusJobStub : IUpdateOrderStatusJob
    {
        public void CheckStatus(string uniqueId)
        {
            // No op
        }

        public void CheckStatus(Guid orderId)
        {
            // No op
        }
    }
}