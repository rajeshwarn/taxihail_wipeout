#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OrderRatingsDao : IOrderRatingsDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public OrderRatingsDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public OrderRatings GetOrderRatingsByOrderId(Guid orderId)
        {
            OrderRatingDetails orderRatingDetails;
            List<RatingScoreDetails> ratingScoreDetails;

            using (var context = _contextFactory.Invoke())
            {
                orderRatingDetails = context.Query<OrderRatingDetails>().SingleOrDefault(d => d.OrderId == orderId);
                ratingScoreDetails = context.Query<RatingScoreDetails>().Where(d => d.OrderId == orderId).ToList();
            }

            return orderRatingDetails == null
                ? new OrderRatings()
                : new OrderRatings
                {
                    OrderId = orderRatingDetails.OrderId,
                    Note = orderRatingDetails.Note,
                    RatingScores =
                        ratingScoreDetails.Select(
                            s => new RatingScore {RatingTypeId = s.RatingTypeId, Score = s.Score, Name = s.Name})
                            .ToList()
                };
        }
    }
}