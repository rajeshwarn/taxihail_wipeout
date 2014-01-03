#region

using System;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOrderRatingsDao
    {
        OrderRatings GetOrderRatingsByOrderId(Guid orderId);
    }
}