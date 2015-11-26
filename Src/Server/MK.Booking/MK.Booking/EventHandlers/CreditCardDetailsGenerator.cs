using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using AutoMapper;
using Infrastructure.Messaging.Handling;

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
        private readonly IProjectionSet<CreditCardDetails> _creditCardProjectionSet;

        public CreditCardDetailsGenerator(IProjectionSet<CreditCardDetails> creditCardProjectionSet)
        {
            _creditCardProjectionSet = creditCardProjectionSet;
        }

        public void Handle(AllCreditCardsRemoved @event)
        {
            _creditCardProjectionSet.Remove(x => x.AccountId == @event.SourceId);
        }

        public void Handle(CreditCardAddedOrUpdated @event)
        {
            if (!_creditCardProjectionSet.Exists(@event.CreditCardId))
            {
                _creditCardProjectionSet.Add(Mapper.Map(@event, new CreditCardDetails()));
            }
            else
            {
                _creditCardProjectionSet.Update(@event.CreditCardId, card =>
                {
                    Mapper.Map(@event, card);
                });
            }
        }

        public void Handle(CreditCardLabelUpdated @event)
        {
            _creditCardProjectionSet.Update(@event.CreditCardId, card =>
            {
                card.Label = @event.Label;
            });
        }

        public void Handle(CreditCardRemoved @event)
        {
            _creditCardProjectionSet.Remove(@event.CreditCardId);
        }

        public void Handle(CreditCardDeactivated @event)
        {
            // Deactivate every cards of the account
            _creditCardProjectionSet.Update(x => x.AccountId == @event.SourceId, card =>
            {
                card.IsDeactivated = true;
            });
        }

        public void Handle(OverduePaymentSettled @event)
        {
            // Reactivate every cards of the account
            _creditCardProjectionSet.Update(x => x.AccountId == @event.SourceId, card =>
            {
                card.IsDeactivated = false;
            });
        }
    }
}