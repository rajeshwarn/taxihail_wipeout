#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderRatingsService : BaseApiService
    {
        private readonly ICommandBus _commandBus;

        public OrderRatingsService(IOrderRatingsDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        protected IOrderRatingsDao Dao { get; set; }

        public OrderRatings Get(Guid orderId)
        {
            return Dao.GetOrderRatingsByOrderId(orderId);
        }

        public void Post(OrderRatingsRequest request)
        {
            var accountId = Session.UserId;

            if (request.RatingScores == null)
            {
                return;
            }

            // cleanup ratings in case we were sent duplicates
            var ratingScoresCleanedUpForDuplicates = new List<RatingScore>();
            foreach (var rating in request.RatingScores)
            {
                if (ratingScoresCleanedUpForDuplicates.None(x => x.Name == rating.Name))
                {
                    ratingScoresCleanedUpForDuplicates.Add(rating);
                }
            }

            request.RatingScores = ratingScoresCleanedUpForDuplicates;

            if (HasNoValidExistingRating(request.OrderId) && request.RatingScores.Any())
            {
                var command = new RateOrder
                {
                    AccountId = accountId,
                    Note = request.Note,
                    OrderId = request.OrderId,
                    RatingScores = request.RatingScores
                };

                _commandBus.Send(command);
            }
        }

        private bool HasNoValidExistingRating(Guid orderId)
        {
            var ratings = Dao.GetOrderRatingsByOrderId(orderId);
            return ratings.OrderId.IsNullOrEmpty() || ratings.RatingScores.None();
        }
    }
}