using System;
using System.Linq;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;
using log4net;

namespace DatabaseInitializer.Services
{
    public class OrderStatusEventsMigrator : IEventsMigrator
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly ITextSerializer _textSerializer;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private ILog _loggger;

        public OrderStatusEventsMigrator(Func<EventStoreDbContext> contextFactory, ITextSerializer textSerializer, IBookingWebServiceClient bookingWebServiceClient)
        {
            _contextFactory = contextFactory;
            _textSerializer = textSerializer;
            _bookingWebServiceClient = bookingWebServiceClient;
            _loggger = LogManager.GetLogger("DatabaseInitializer");
        }

        public void Do(string version)
        {
            var versionNumbers = version.Split('.');
            var versionNumber = int.Parse(versionNumbers[1]);
            var minorVersionNumber = 0;
            if (versionNumbers.Length > 2)
            {
                minorVersionNumber = int.Parse(versionNumbers[2]);
            }


            if (versionNumber < 3
                || (versionNumber == 3 && minorVersionNumber == 0))
            {
                _loggger.Debug("Migrating Order StatusEvent");
                using (var context = _contextFactory.Invoke())
                {
                    var orderCreatedEvents = context.Set<Event>().Where(x => x.EventType == typeof(OrderCreated).FullName).ToList();
                    foreach (var orderCreatedMessage in orderCreatedEvents)
                    {
                        var orderCreatedEvent = _textSerializer.Deserialize<OrderCreated>(orderCreatedMessage.Payload);

                        
                        var versionEvent = (from @event in context.Set<Event>()
                                   where @event.AggregateId == orderCreatedMessage.AggregateId
                                   select @event.Version).Max() + 1;
                        var statusChangedEvent = new OrderStatusChanged
                                                     {
                                                         EventDate = DateTime.UtcNow,
                                                         SourceId = orderCreatedMessage.AggregateId,
                                                         Version = versionEvent,
                                                         Status = new OrderStatusDetail{ OrderId = orderCreatedMessage.AggregateId, IBSOrderId = orderCreatedEvent.IBSOrderId}
                                                     };

                        CompleteStatusFromIbs(statusChangedEvent, orderCreatedEvent.AccountId, context);


                        var statusChangedEventMessage = new Event
                                                            {
                                                                AggregateId = orderCreatedMessage.AggregateId,
                                                                AggregateType = orderCreatedMessage.AggregateType,
                                                                CorrelationId = orderCreatedMessage.CorrelationId,
                                                                EventDate = DateTime.UtcNow,
                                                                Payload = _textSerializer.Serialize(statusChangedEvent),
                                                                Version = versionEvent,
                                                                EventType = typeof(OrderStatusChanged).FullName
                                                            };
                        context.Set<Event>().Add(statusChangedEventMessage);
                    }
                    context.SaveChanges();
                }
            }
        }

        private void CompleteStatusFromIbs(OrderStatusChanged statusChangedEvent, Guid accountId, EventStoreDbContext context)
        {
            var accountCreatedPayload = (from @event in context.Set<Event>()
                                where @event.AggregateId == accountId && @event.EventType == typeof(AccountRegistered).FullName 
                                select @event.Payload).FirstOrDefault();

            var accountCreatedEvent = _textSerializer.Deserialize<AccountRegistered>(accountCreatedPayload);
            var ibsAccountId = accountCreatedEvent.IbsAcccountId;

            var ibsStatus = _bookingWebServiceClient.GetOrderStatus(statusChangedEvent.Status.IBSOrderId.Value, ibsAccountId, null);

            statusChangedEvent.Status.IBSStatusId = ibsStatus.Status;

        }
    }
}