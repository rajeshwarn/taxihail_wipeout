using System;
using System.Linq;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class AddressHistoryGenerator : IEventHandler<OrderCreated>
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
    }
}
