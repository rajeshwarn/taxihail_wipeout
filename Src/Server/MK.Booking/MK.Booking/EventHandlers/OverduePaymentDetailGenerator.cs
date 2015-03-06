using System;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OverduePaymentDetailGenerator :
        IEventHandler<OverduePaymentLogged>,
        IEventHandler<OverduePaymentSettled>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public OverduePaymentDetailGenerator(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(OverduePaymentLogged @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var overduePayment = context.Find<OverduePaymentDetail>(@event.OrderId);
                if (overduePayment == null)
                {
                    context.Save(new OverduePaymentDetail
                    {
                        OrderId = @event.OrderId,
                        IBSOrderId = @event.IBSOrderId,
                        AccountId = @event.SourceId,
                        OverdueAmount = @event.Amount,
                        TransactionId = @event.TransactionId,
                        TransactionDate = @event.TransactionDate
                    });
                }
            }
        }

        public void Handle(OverduePaymentSettled @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var overduePayment = context.Find<OverduePaymentDetail>(@event.OrderId);
                if (overduePayment != null)
                {
                    overduePayment.IsPaid = true;
                    context.Save(overduePayment);
                }
            }
        }
    }
}
