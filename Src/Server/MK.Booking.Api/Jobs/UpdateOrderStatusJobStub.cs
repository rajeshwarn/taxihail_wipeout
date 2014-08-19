using System;
namespace apcurium.MK.Booking.Api.Jobs
{
    /// <summary>
    ///     This is a replacement for UpdateOrderStatus job used when IBS order updates
    ///     are faked using OrderStatusIbsMock
    /// </summary>
    internal class UpdateOrderStatusJobStub : IUpdateOrderStatusJob
    {
        public bool CheckStatus(string uniqueId, int pollingValue)
        {
            // No op
            return false;
        }

        public void CheckStatus(Guid orderId)
        {
            // No op
        }
    }
}