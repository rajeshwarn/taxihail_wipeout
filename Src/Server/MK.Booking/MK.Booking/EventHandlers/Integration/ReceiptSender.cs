using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class ReceiptSender : IIntegrationEventHandler,
          IEventHandler<OrderCompleted>
    {
        readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        public ReceiptSender(Func<BookingDbContext> contextFactory, ICommandBus commandBus)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
        }

        public void Handle(OrderCompleted @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderStatus = context.Find<OrderStatusDetail>(@event.SourceId);
                if (orderStatus != null)
                {
                     if (orderStatus.IBSStatusId == VehicleStatuses.Common.Done && @event.Fare.GetValueOrDefault() > 0)
                     {
                         var account = context.Find<AccountDetail>(orderStatus.AccountId);
                         var command = new Commands.SendReceipt
                         {
                             Id = Guid.NewGuid(),
                             OrderId = @event.SourceId,
                             EmailAddress = account.Email,
                             IBSOrderId = orderStatus.IBSOrderId.GetValueOrDefault(),
                             TransactionDate = orderStatus.PickupDate,
                             VehicleNumber = orderStatus.VehicleNumber,
                             Fare = @event.Fare.GetValueOrDefault(),
                             Toll = @event.Toll.GetValueOrDefault(),
                             Tip = @event.Tip.GetValueOrDefault(),
                         };

                         _commandBus.Send(command);
                     }
                }
            }
        }
    }
}
