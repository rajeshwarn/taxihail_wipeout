using System;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using apcurium.MK.Common.Extensions;
using DatabaseInitializer.OldEvents;
using Infrastructure.Sql.EventSourcing;
using Newtonsoft.Json;
using apcurium.MK.Booking.Events;
using log4net;
using OrderCompleted = DatabaseInitializer.OldEvents.OrderCompleted;

namespace DatabaseInitializer.Services
{
    public class EventsMigrator : IEventsMigrator
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly JsonSerializer _serializer;
        private readonly JsonSerializer _deserializer;
        private ILog _logger;

        public EventsMigrator(Func<EventStoreDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            //deserailize without type
            _deserializer = new JsonSerializer();
            //sereailize with type as expected in infrastructure
            _serializer = new JsonSerializer(){ TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple, TypeNameHandling = TypeNameHandling.All};
            _logger = LogManager.GetLogger("DatabaseInitializer");
        }

        public void Do()
        {
            using (var context = _contextFactory.Invoke())
            {
                var events = context.Set<Event>().Where(x => x.EventType == typeof(PaymentSettingUpdated).FullName ).ToList();
                foreach (var message in events)
                {
                    // fix BraintreeClientSettings namespace problem
                    message.Payload = message.Payload.Replace("apcurium.MK.Common.Configuration.BraintreeClientSettings", "apcurium.MK.Common.Configuration.Impl.BraintreeClientSettings");
                }
                context.SaveChanges();

                // rename Order Pairing events
                events = context.Set<Event>().Where(x => x.EventType.Contains("OrderPairedForRideLinqCmtPayment") || x.EventType.Contains("OrderUnpairedForRideLinqCmtPayment")).ToList();
                foreach (var message in events)
                {
                    message.Payload = message.Payload.Replace("OrderPairedForRideLinqCmtPayment", "OrderPairedForPayment");
                    message.Payload = message.Payload.Replace("OrderUnpairedForRideLinqCmtPayment", "OrderUnpairedForPayment");
                    message.EventType = message.EventType.Replace("OrderPairedForRideLinqCmtPayment", "OrderPairedForPayment");
                    message.EventType = message.EventType.Replace("OrderUnpairedForRideLinqCmtPayment", "OrderUnpairedForPayment");
                }
                context.SaveChanges();

                // convert OrderCompleted to OrderStatusChanged
                events = context.Set<Event>().Where(x => x.EventType.Contains("OrderCompleted")).ToList();
                foreach (var message in events)
                {
                    var @event = Deserialize<OrderCompleted>(message.Payload);
                    var newEvent = new OrderStatusChanged
                    {
                        EventDate = @event.EventDate,
                        SourceId = @event.SourceId,
                        Version = @event.Version,
                        Fare = @event.Fare,
                        Tax = @event.Tax,
                        Tip = @event.Tip,
                        Toll = @event.Toll,
                        IsCompleted = true
                    };
                    message.Payload = Serialize(newEvent);
                    message.EventType = message.EventType.Replace("OrderCompleted", "OrderStatusChanged");
                }
                context.SaveChanges();

                // convert OrderFareUpdated to OrderStatusChanged
                events = context.Set<Event>().Where(x => x.EventType.Contains("OrderFareUpdated")).ToList();
                foreach (var message in events)
                {
                    var @event = Deserialize<OrderFareUpdated>(message.Payload);
                    var newEvent = new OrderStatusChanged
                    {
                        EventDate = @event.EventDate,
                        SourceId = @event.SourceId,
                        Version = @event.Version,
                        Fare = @event.Fare,
                        Tax = @event.Tax,
                        Tip = @event.Tip,
                        Toll = @event.Toll,
                        IsCompleted = false
                    };
                    message.Payload = Serialize(newEvent);
                    message.EventType = message.EventType.Replace("OrderFareUpdated", "OrderStatusChanged");
                }
                context.SaveChanges();

                // update OrderStatusChanged containing a Status with an invalid pickup date
                events = context.Set<Event>().Where(x => x.EventType.Contains("OrderStatusChanged") && !x.Payload.Contains("\"Status\":null")).ToList();
                foreach (var message in events)
                {
                    var @event = Deserialize<OrderStatusChanged>(message.Payload);

                    @event.Status.PickupDate = @event.Status.PickupDate < ((DateTime)SqlDateTime.MinValue)
                            ? (DateTime)SqlDateTime.MinValue
                            : @event.Status.PickupDate;

                    message.Payload = Serialize(@event);
                }
                context.SaveChanges();
            }
        }

        public string Serialize<T>(T data)
        {
            using (var writer = new StringWriter())
            {
                _serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }
      
        public T Deserialize<T>(string serialized)
        {
            using (var reader = new StringReader(serialized))
            {
                var jsonReader = new JsonTextReader(reader);

                try
                {
                    return this._deserializer.Deserialize<T>(jsonReader);
                }
                catch (JsonSerializationException e)
                {
                    // Wrap in a standard .NET exception.
                    throw new SerializationException(e.Message, e);
                }
            }
        }
    }
}