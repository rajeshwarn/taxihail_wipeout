#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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
            var command = new RateOrder
            {
                Note = request.Note,
                OrderId = request.OrderId,
                RatingScores = request.RatingScores
            };

            _commandBus.Send(command);

            return String.Empty;
        }
    }
}