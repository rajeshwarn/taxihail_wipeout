using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardDetailsGenerator : 
        IEventHandler<CreditCardAdded>, 
        IEventHandler<CreditCardRemoved>,
        IEventHandler<PaymentModeChanged>

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

        public void Handle(CreditCardRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var address = context.Find<CreditCardDetails>(@event.CreditCardId);
                if (address != null)
                {
                    context.Set<CreditCardDetails>().Remove(address);
                    context.SaveChanges();
                }
            }
        }

        public void Handle(PaymentModeChanged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveAll<CreditCardDetails>();
                context.SaveChanges();
            }
        }
    }
}