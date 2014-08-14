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
                var events = context.Set<Event>().Where(x =>x.EventType == typeof(PaymentSettingUpdated).FullName ).ToList();
                foreach (var message in events)
                {
                    // fix BraintreeClientSettings namespace problem
                    message.Payload = message.Payload.Replace("apcurium.MK.Common.Configuration.BraintreeClientSettings" , "apcurium.MK.Common.Configuration.Impl.BraintreeClientSettings");

                    // rename order pairing events
                    message.Payload = message.Payload.Replace("OrderPairedForRideLinqCmtPayment", "OrderPairedForPayment");
                    message.Payload = message.Payload.Replace("OrderUnpairedForRideLinqCmtPayment", "OrderUnpairedForPayment");
                    message.EventType = message.EventType.Replace("OrderPairedForRideLinqCmtPayment", "OrderPairedForPayment");
                    message.EventType = message.EventType.Replace("OrderUnpairedForRideLinqCmtPayment", "OrderUnpairedForPayment");
                }

                // delete OrderVehiclePositionChanged events
                events.Remove(x => x.EventType.Contains("OrderVehiclePositionChanged"));

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