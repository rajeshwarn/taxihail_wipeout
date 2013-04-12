using AutoMapper;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class OrderCommandHandler : ICommandHandler<CreateOrder>, ICommandHandler<CancelOrder>, ICommandHandler<CompleteOrder>, ICommandHandler<RemoveOrderFromHistory>, ICommandHandler<RateOrder>
    {
        private readonly IEventSourcedRepository<Order> _repository;

        public OrderCommandHandler(IEventSourcedRepository<Order> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateOrder command)
        {
            var settings =  new BookingSettings();
            Mapper.Map(command.Settings, settings);
            var order = new Order(command.OrderId, command.AccountId, command.IBSOrderId, command.PickupDate, 
                                    command.PickupAddress, command.DropOffAddress, settings);

            if (command.Payment.PayWithCreditCard)
            {
                var payment = Mapper.Map<PaymentInformation>(command.Payment);
                order.SetPaymentInformation(payment);
            }
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CancelOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.Cancel();
            _repository.Save(order,command.Id.ToString());
        }

        public void Handle(CompleteOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.Complete(command.Date, command.Fare, command.Tip, command.Toll);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(RemoveOrderFromHistory command)
        {
            var order = _repository.Find(command.OrderId);
            order.RemoveFromHistory();
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(RateOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.RateOrder(command.Note, command.RatingScores);
            _repository.Save(order, command.Id.ToString());
        }
    }
}
