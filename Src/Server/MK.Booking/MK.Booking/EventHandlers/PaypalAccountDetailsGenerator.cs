using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class PaypalAccountDetailsGenerator :
        IEventHandler<PayPalAccountLinked>,
        IEventHandler<PayPalAccountUnlinked>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public PaypalAccountDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(PayPalAccountLinked @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payPalAccountDetails = context.Find<PayPalAccountDetails>(@event.SourceId);
                if (payPalAccountDetails == null)
                {
                    context.Save(new PayPalAccountDetails
                    {
                        AccountId = @event.SourceId,
                        EncryptedRefreshToken = @event.EncryptedRefreshToken
                    });
                }
                else
                {
                    payPalAccountDetails.EncryptedRefreshToken = @event.EncryptedRefreshToken;
                }

                context.SaveChanges();
            }
        }

        public void Handle(PayPalAccountUnlinked @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveWhere<PayPalAccountDetails>(x => x.AccountId == @event.SourceId);
                context.SaveChanges();
            }
        }
    }
}