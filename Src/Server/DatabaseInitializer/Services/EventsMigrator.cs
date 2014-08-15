using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using apcurium.MK.Common.Extensions;
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

                // convert OrderCompleted to OrderFareUpdated
                events = context.Set<Event>().Where(x => x.EventType.Contains("OrderCompleted")).ToList();
                foreach (var message in events)
                {
                    var @event = Deserialize<OrderCompleted>(message.Payload);
                    var newEvent = new OrderFareUpdated
                    {
                        EventDate = @event.EventDate,
                        SourceId = @event.SourceId,
                        Version = @event.Version,
                        Fare = @event.Fare.GetValueOrDefault(0),
                        Tax = @event.Tax.GetValueOrDefault(0),
                        Tip = @event.Tip.GetValueOrDefault(0),
                        Toll = @event.Toll.GetValueOrDefault(0)
                    };
                    message.Payload = Serialize(newEvent);
                    message.EventType = message.EventType.Replace("OrderCompleted", "OrderFareUpdated");
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