using System;
using System.Linq;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AddressHistoryGenerator : IEventHandler<OrderCreated>, IEventHandler<FavoriteAddressAdded>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        public AddressHistoryGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            AutoMapper.Mapper.CreateMap<OrderCreated, HistoricAddress>()
                .ForMember(p=>p.Apartment, opt => opt.MapFrom(m=>m.PickupApartment))
                .ForMember(p=>p.FullAddress, opt => opt.MapFrom(m=>m.PickupAddress))
                .ForMember(p=>p.RingCode, opt => opt.MapFrom(m=>m.PickupRingCode))
                .ForMember(p=>p.Latitude, opt => opt.MapFrom(m=>m.PickupLatitude))
                .ForMember(p=>p.Longitude, opt => opt.MapFrom(m=>m.PickupLongitude));

            AutoMapper.Mapper.CreateMap<FavoriteAddressAdded, HistoricAddress>()
                .ForMember(p => p.AccountId, opt => opt.MapFrom(m => m.SourceId));

        }

        public void Handle(OrderCreated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var identicalAddresses = from a in context.Query<HistoricAddress>()
                                         where a.AccountId == @event.AccountId
                                         where a.Apartment == @event.PickupApartment
                                         where a.FullAddress == @event.PickupAddress
                                         where a.RingCode == @event.PickupRingCode
                                         where a.Latitude == @event.PickupLatitude
                                         where a.Longitude == @event.PickupLongitude
                                         select a;

                if(!identicalAddresses.Any())
                {
                    var address = new HistoricAddress();
                    AutoMapper.Mapper.Map(@event, address);
                    address.Id = Guid.NewGuid();
                context.Save(address);
                }
            }
        }

        public void Handle(FavoriteAddressAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var identicalAddresses = from a in context.Query<HistoricAddress>()
                                         where a.AccountId == @event.SourceId
                                         where a.Apartment == @event.Apartment
                                         where a.FullAddress == @event.FullAddress
                                         where a.RingCode == @event.RingCode
                                         where a.Latitude == @event.Latitude
                                         where a.Longitude == @event.Longitude
                                         select a;

                if (identicalAddresses.Any())
                {
                    var historicAddress = context.Query<HistoricAddress>().FirstOrDefault(c => c.AccountId.Equals(@event.SourceId));
                    context.Set<HistoricAddress>().Remove(historicAddress);
                    context.SaveChanges();
                }
            }
        }
    }
}