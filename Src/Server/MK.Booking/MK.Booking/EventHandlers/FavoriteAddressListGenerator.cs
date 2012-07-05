using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.BackOffice.EventHandlers
{
    public class FavoriteAddressListGenerator : IEventHandler<FavoriteAddressAdded>, IEventHandler<FavoriteAddressRemoved>, IEventHandler<FavoriteAddressUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        public FavoriteAddressListGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
            AutoMapper.Mapper.CreateMap<FavoriteAddressUpdated, FavoriteAddress >().ForMember(p=>p.Id,options=>options.MapFrom(m=>m.AddressId));

        }

        public void Handle(FavoriteAddressAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new FavoriteAddress
                {
                    Id = @event.AddressId,
                    AccountId = @event.SourceId,
                    FriendlyName = @event.FriendlyName,
                    Apartment = @event.Apartment,
                    FullAddress = @event.FullAddress,
                    RingCode = @event.RingCode,
                    Latitude = @event.Latitude,
                    Longitude = @event.Longitude
                });
            }
        }

        public void Handle(FavoriteAddressRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<FavoriteAddress>(@event.AddressId);
                context.Set<FavoriteAddress>().Remove(address);
                context.SaveChanges();
            }
        }

        public void Handle(FavoriteAddressUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<FavoriteAddress>(@event.AddressId);
                AutoMapper.Mapper.Map(@event, address);
                context.SaveChanges();
            }
        }
    }
}
