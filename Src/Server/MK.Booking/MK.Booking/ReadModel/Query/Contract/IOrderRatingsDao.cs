using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IOrderRatingsDao
    {
        OrderRatings GetOrderRatingsByOrderId(Guid orderId);
    }
}