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
            var order = new Order(command.Id, command.AccountId, command.PickupDate, command.PickupAddress, command.PickupLongitude, command.PickupLatitude,
                                    command.PickupApartment, command.PickupRingCode);
            _repository.Save(order);
        }

        public void Handle(CancelOrder command)
        {
            var order = _repository.Find(command.OrderId);
            order.Cancel();
            //TODO update order statuts here
            _repository.Save(order);
        }
    }
}
