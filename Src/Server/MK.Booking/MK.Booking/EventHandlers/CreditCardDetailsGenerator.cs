using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardDetailsGenerator :  IEventHandler<CreditCardAdded>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CreditCardDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(CreditCardAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = new CreditCardDetails();
                AutoMapper.Mapper.Map(@event, details);
                context.Save(details);
            }
        }
    }
}