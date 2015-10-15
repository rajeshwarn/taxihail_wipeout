#region

using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardDetailsGenerator :
        IEventHandler<CreditCardAddedOrUpdated>,
        IEventHandler<CreditCardLabelUpdated>,
        IEventHandler<CreditCardRemoved>,
        IEventHandler<AllCreditCardsRemoved>,
        IEventHandler<CreditCardDeactivated>,
        IEventHandler<OverduePaymentSettled>
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

        public void Handle(CreditCardAddedOrUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var existingCreditCard = context.Find<CreditCardDetails>(@event.CreditCardId);
                var creditCard = existingCreditCard ?? new CreditCardDetails();
                Mapper.Map(@event, creditCard);
                context.Save(creditCard);
            }
        }

        public void Handle(CreditCardLabelUpdated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var existingCreditCard = context.Find<CreditCardDetails>(@event.CreditCardId);
                existingCreditCard.Label = @event.Label.ToString();

                context.Save(existingCreditCard);
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

        public void Handle(CreditCardDeactivated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Deactivate credit card was declined
                var creditCardDetails = context.Query<CreditCardDetails>().FirstOrDefault(c => c.AccountId == @event.SourceId);
                if (creditCardDetails != null)
                {
                    creditCardDetails.IsDeactivated = true;
                    context.Save(creditCardDetails);
                }
            }
        }

        public void Handle(OverduePaymentSettled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Re-activate credit card
                var creditCardDetails = context.Query<CreditCardDetails>().FirstOrDefault(c => c.AccountId == @event.SourceId);
                if (creditCardDetails != null)
                {
                    creditCardDetails.IsDeactivated = false;
                    context.Save(creditCardDetails);
                }
            }
        }
    }
}