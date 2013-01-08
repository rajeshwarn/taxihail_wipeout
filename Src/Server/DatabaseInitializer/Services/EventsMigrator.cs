using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Infrastructure.Sql.EventSourcing;
using Newtonsoft.Json;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Common.Entity;
using log4net;

namespace DatabaseInitializer.Services
{
    public class EventsMigrator : IEventsMigrator
    {
        private readonly IAddresses _addressesService;
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly JsonSerializer _serializer;
        private readonly JsonSerializer _deserializer;
        private ILog _loggger;

        public EventsMigrator(IAddresses addressesService, Func<EventStoreDbContext> contextFactory)
        {
            _addressesService = addressesService;
            _contextFactory = contextFactory;
            //deserailize without type
            _deserializer = new JsonSerializer();
            //sereailize with type as expected in infrastructure
            _serializer = new JsonSerializer(){ TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple, TypeNameHandling = TypeNameHandling.All};
            _loggger = LogManager.GetLogger("DatabaseInitializer");
        }

        public void Do(string version)
        {
            //parse version i.e. x.y.z to get y and z
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

                using (var context = _contextFactory.Invoke())
                {
                    var orderCreatedEvent =
                        context.Set<Event>().Where(x => x.EventType == typeof (OrderCreated).FullName).ToList();
                    foreach (var message in orderCreatedEvent)
                    {
                        var @event = Deserialize<OrderCreated>(message.Payload);
                        FillAdress(@event.PickupAddress, @event.PickupAddress.FullAddress);
                        message.Payload = Serialize(@event);

                    }
                    context.SaveChanges();
                }

                using (var context = _contextFactory.Invoke())
                {
                    var events =
                        context.Set<Event>().Where(x => x.EventType == typeof (FavoriteAddressAdded).FullName).ToList();
                    foreach (var message in events)
                    {
                        var @event = Deserialize<OldEvents.FavoriteAddressAddedv1>(message.Payload);
                        if (@event.Address == null)
                        {
                            var newEvent = new FavoriteAddressAdded
                                               {
                                                   SourceId = @event.SourceId,
                                                   Version = @event.Version,
                                                   Address =
                                                       new Address
                                                           {
                                                               Id = @event.AddressId,
                                                               FullAddress = @event.FullAddress
                                                           }
                                               };
                            FillAdress(newEvent.Address, @event.FullAddress);
                            message.Payload = Serialize(newEvent);
                        }
                    }
                    context.SaveChanges();
                }

                using (var context = _contextFactory.Invoke())
                {
                    var events =
                        context.Set<Event>()
                               .Where(x => x.EventType == typeof (FavoriteAddressUpdated).FullName)
                               .ToList();
                    foreach (var message in events)
                    {
                        var @event = Deserialize<OldEvents.FavoriteAddressUpdatedv1>(message.Payload);
                        if (@event.Address == null)
                        {
                            var newEvent = new FavoriteAddressUpdated
                                               {
                                                   SourceId = @event.SourceId,
                                                   Version = @event.Version,
                                                   Address =
                                                       new Address
                                                           {
                                                               Id = @event.AddressId,
                                                               FullAddress = @event.FullAddress
                                                           }
                                               };
                            FillAdress(newEvent.Address, @event.FullAddress);
                            message.Payload = Serialize(newEvent);
                        }
                    }
                    context.SaveChanges();
                }
            }
        }

        private bool FillAdress(Address toBeCompleted, string fullAdress)
        {
            var addresses = _addressesService.Search(fullAdress, toBeCompleted.Latitude, toBeCompleted.Longitude);
            if (addresses != null && addresses.Any())
            {
                var address = addresses.First();
                toBeCompleted.StreetNumber = address.StreetNumber;
                toBeCompleted.Street = address.Street;
                toBeCompleted.City = address.City;
                toBeCompleted.ZipCode = address.ZipCode;
                toBeCompleted.State = address.State;
                return true;
            }
            _loggger.Debug("Can't geocode + " + fullAdress);
            return false;
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