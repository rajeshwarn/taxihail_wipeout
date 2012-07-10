using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class OrderCommandHandler : ICommandHandler<CreateOrder>, ICommandHandler<CancelOrder>
    {
        private readonly IEventSourcedRepository<Order> _repository;

        public OrderCommandHandler(IEventSourcedRepository<Order> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateOrder command)
        {
            var order = new Order(command.OrderId, command.AccountId, command.PickupDate, command.PickupAddress, command.PickupLongitude, command.PickupLatitude,
                                    command.PickupApartment, command.PickupRingCode, command.DropOffAddress, command.DropOffLongitude, command.DropOffLatitude, command.Status);
            _repository.Save(order);
        }

        public void Handle(CancelOrder command)
        {
            Order order = _repository.Find(command.OrderId);
            order.Cancel();
            _repository.Save(order);
        }
    }
}
