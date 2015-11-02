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
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderRatingsService : Service
    {
        private readonly ICommandBus _commandBus;

        public OrderRatingsService(IOrderRatingsDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        protected IOrderRatingsDao Dao { get; set; }

        public object Get(OrderRatingsRequest request)
        {
            var orderRatings = Dao.GetOrderRatingsByOrderId(request.OrderId);

            return orderRatings;
        }

        public object Post(OrderRatingsRequest request)
        {
			var accountId = new Guid(this.GetSession().UserAuthId);

            if (request.RatingScores != null)
            {
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
            }
            
            var command = new RateOrder
            {
				AccountId = accountId,
                Note = request.Note,
                OrderId = request.OrderId,
                RatingScores = request.RatingScores
            };

            _commandBus.Send(command);

            return String.Empty;
        }
    }
}