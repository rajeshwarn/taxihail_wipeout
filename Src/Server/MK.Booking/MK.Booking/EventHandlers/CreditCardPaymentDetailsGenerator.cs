using System;
using Infrastructure.Messaging.Handling;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.EventHandlers
{
    public class CreditCardPaymentDetailsGenerator:
        IEventHandler<CreditCardPaymentInitiated>,
        IEventHandler<CreditCardPaymentCaptured>

    {
        readonly Func<BookingDbContext> _contextFactory;

        public CreditCardPaymentDetailsGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(CreditCardPaymentInitiated @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new CreditCardPaymentDetail
                                 {
                                     PaymentId = @event.SourceId,
                                     Amount = @event.Amount,
                                     TransactionId = @event.TransactionId,
                                     OrderId = @event.OrderId,
                                     CardToken = @event.CardToken,
                                     IsCaptured = false,
                                 });
            }
        }

        public void Handle(CreditCardPaymentCaptured @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payment = context.Set<CreditCardPaymentDetail>().Find(@event.SourceId);
                if (payment == null) throw new InvalidOperationException("Payment not found");

                payment.IsCaptured = true;
                context.SaveChanges();
            }
        }
    }
}