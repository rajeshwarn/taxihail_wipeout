#region

using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardDetailsGenerator :
        IEventHandler<CreditCardAdded>,
        IEventHandler<CreditCardRemoved>,
        IEventHandler<AllCreditCardsRemoved>

    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CreditCardDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveWhere<CreditCardDetails>(cc => @event.SourceId == cc.AccountId);
                context.SaveChanges();
            }
        }

        public void Handle(CreditCardAdded @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = new CreditCardDetails();
                Mapper.Map(@event, details);
                context.Save(details);
            }
        }

        public void Handle(CreditCardRemoved @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var creditCard = context.Find<CreditCardDetails>(@event.CreditCardId);
                if (creditCard != null)
                {
                    context.Set<CreditCardDetails>().Remove(creditCard);
                    context.SaveChanges();
                }
            }
        }
    }
}