using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaypalAccountDetailsGenerator :
        IEventHandler<PayPalAccountLinked>,
        IEventHandler<PayPalAccountUnlinked>
    {
        private readonly IProjectionSet<PayPalAccountDetails> _paypalAccountProjectionSet;

        public PaypalAccountDetailsGenerator(IProjectionSet<PayPalAccountDetails> paypalAccountProjectionSet)
        {
            _paypalAccountProjectionSet = paypalAccountProjectionSet;
        }

        public void Handle(PayPalAccountLinked @event)
        {
            if (!_paypalAccountProjectionSet.Exists(@event.SourceId))
            {
                _paypalAccountProjectionSet.Add(new PayPalAccountDetails
                {
                    AccountId = @event.SourceId,
                    EncryptedRefreshToken = @event.EncryptedRefreshToken
                });
            }
            else
            {
                _paypalAccountProjectionSet.Update(@event.SourceId, payPalAccountDetails =>
                {
                    payPalAccountDetails.EncryptedRefreshToken = @event.EncryptedRefreshToken;
                });
            }
        }

        public void Handle(PayPalAccountUnlinked @event)
        {
            _paypalAccountProjectionSet.Remove(x => x.AccountId == @event.SourceId);
        }
    }
}