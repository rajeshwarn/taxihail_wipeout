using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Common.Extensions;

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
            var settings =  new BookingSettings();
            AutoMapper.Mapper.Map(command.Settings, settings);
            var order = new Order(command.OrderId, command.AccountId, command.IBSOrderId, command.PickupDate, 
                                    command.PickupAddress.FullAddress, command.PickupAddress.Longitude, command.PickupAddress.Latitude,command.PickupAddress.Apartment, command.PickupAddress.RingCode,
                                    command.DropOffAddress.SelectOrDefault(a => a.FullAddress), command.DropOffAddress.SelectOrDefault(a => (double?)a.Longitude, null), command.DropOffAddress.SelectOrDefault(a => (double?)a.Latitude, null),
                                    settings);
            _repository.Save(order, command.Id.ToString());
        }

        public void Handle(CancelOrder command)
        {
            Order order = _repository.Find(command.OrderId);
            order.Cancel();
            _repository.Save(order,command.Id.ToString());
        }
    }
}
