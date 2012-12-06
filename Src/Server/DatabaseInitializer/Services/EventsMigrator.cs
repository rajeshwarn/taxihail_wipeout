using System;
using System.Linq;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
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
        private readonly ITextSerializer _serializer;
        private ILog _loggger;

        public EventsMigrator(IAddresses addressesService, Func<EventStoreDbContext> contextFactory, ITextSerializer serializer)
        {
            _addressesService = addressesService;
            _contextFactory = contextFactory;
            _serializer = serializer;
            _loggger = LogManager.GetLogger("DatabaseInitializer");
        }

        public void Do()
        {
            using (var context = _contextFactory.Invoke())
            {
                var orderCreatedEvent = context.Set<Event>().Where(x => x.EventType == typeof(OrderCreated).FullName).ToList();
                foreach (var message in orderCreatedEvent)
                {
                    var @event = _serializer.Deserialize<OrderCreated>(message.Payload);
                    FillAdress(@event.PickupAddress, @event.PickupAddress.FullAddress);
                    message.Payload = _serializer.Serialize(@event);
                    
                }
                context.SaveChanges();
            }

            using (var context = _contextFactory.Invoke())
            {
                var events = context.Set<Event>().Where(x => x.EventType == typeof(FavoriteAddressAdded).FullName).ToList();
                foreach (var message in events)
                {
                    var @event = _serializer.Deserialize<OldEvents.FavoriteAddressAddedv1>(message.Payload);
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
                        message.Payload = _serializer.Serialize(newEvent);
                    }
                }
                context.SaveChanges();
            }

            using (var context = _contextFactory.Invoke())
            {
                var events = context.Set<Event>().Where(x => x.EventType == typeof(FavoriteAddressUpdated).FullName).ToList();
                foreach (var message in events)
                {
                    var @event = _serializer.Deserialize<OldEvents.FavoriteAddressUpdatedv1>(message.Payload);
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
                        FillAdress(@event.Address, @event.FullAddress);
                        message.Payload = _serializer.Serialize(newEvent);
                    }
                }
                context.SaveChanges();
            }
        }

        private bool FillAdress(Address toBeCompleted, string fullAdress)
        {
            var addresses = _addressesService.Search(fullAdress, toBeCompleted.Latitude, toBeCompleted.Longitude);
            if (addresses.Any())
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
    }
}