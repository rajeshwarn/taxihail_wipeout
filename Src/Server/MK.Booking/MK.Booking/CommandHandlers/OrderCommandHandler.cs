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
    public class OrderCommandHandler : ICommandHandler<CreateOrder>
    {
        private readonly IEventSourcedRepository<Order> _repository;

        public OrderCommandHandler(IEventSourcedRepository<Order> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateOrder command)
        {
            var order = new Order(command.Id, command.AccountId, command.PickupDate, command.RequestedDateTime,
                                  command.FriendlyName, command.FullAddress, command.Longitude
                                  , command.Latitude, command.Apartment, command.RingCode);
            _repository.Save(order);
        }
    }
}
