using AutoMapper;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class OrderCommandHandler :
        ICommandHandler<CreateOrder>, 
        ICommandHandler<CancelOrder>, 
        ICommandHandler<RemoveOrderFromHistory>,
        ICommandHandler<RateOrder>,
        ICommandHandler<ChangeOrderStatus>
    {
        private readonly IEventSourcedRepository<Order> _repository;

        public OrderCommandHandler(IEventSourcedRepository<Order> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateOrder command)
        {
            var order = new Order(command.OrderId, command.AccountId, command.IBSOrderId, command.PickupDate, 
                                    command.PickupAddress, command.DropOffAddress, command.Settings, command.EstimatedFare);

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

        public void Handle(ChangeOrderStatus command)
        {
            var order = _repository.Find(command.Status.OrderId);
            order.ChangeStatus(command.Status);

            if (command.Status.Status == Common.Entity.OrderStatus.Completed)
            {
                order.Complete(command.Fare, command.Tip, command.Toll, command.Tax);
            }
            _repository.Save(order, command.Id.ToString());
        }

    }
}
