using System;
using System.Net;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderRatingsService : RestServiceBase<OrderRatingsRequest>
    {
        private readonly ICommandBus _commandBus;
        protected IOrderRatingsDao Dao { get; set; }

        public OrderRatingsService(IOrderRatingsDao dao, ICommandBus commandBus)
        {
            _commandBus = commandBus;
            Dao = dao;
        }

        public override object OnGet(OrderRatingsRequest request)
        {
            var orderRatings = Dao.GetOrderRatingsByOrderId(request.OrderId);

            return orderRatings;
        }

        public override object OnPost(OrderRatingsRequest request)
        {
            var command = new RateOrder { Note = request.Note, OrderId = request.OrderId, RatingScores = request.RatingScores };

            _commandBus.Send(command);

            return String.Empty;
        }
    }
}