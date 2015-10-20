#region

using System;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOrderRatingsDao
    {
        OrderRatings GetOrderRatingsByOrderId(Guid orderId);

		IList<RatingScoreDetails> GetRatings();
    }
}