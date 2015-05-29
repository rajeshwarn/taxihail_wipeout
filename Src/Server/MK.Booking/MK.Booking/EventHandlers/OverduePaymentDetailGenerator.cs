using System;
using System.Data.SqlTypes;
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
                    DateTime? transactionDate = null;

                    // Make sure transaction date is a valid SQL date time
                    if (@event.TransactionDate.HasValue
                        && @event.TransactionDate.Value > SqlDateTime.MinValue
                        && @event.TransactionDate.Value < SqlDateTime.MaxValue)
                    {
                        transactionDate = @event.TransactionDate.Value;
                    }

                    context.Save(new OverduePaymentDetail
                    {
                        OrderId = @event.OrderId,
                        IBSOrderId = @event.IBSOrderId,
                        AccountId = @event.SourceId,
                        OverdueAmount = @event.Amount,
                        TransactionId = @event.TransactionId,
                        TransactionDate = transactionDate,
                        ContainBookingFees = @event.IsBookingFee,
                        ContainStandaloneFees = @event.IsCancellationFee || @event.IsNoShowFee
                    });
                }
                else
                {
                    if (!@event.IsBookingFee)
                    {
                        overduePayment.TransactionId = @event.TransactionId;
                    }

                    if (@event.IBSOrderId.HasValue)
                    {
                        overduePayment.IBSOrderId = @event.IBSOrderId;
                    }

                    overduePayment.ContainBookingFees = overduePayment.ContainBookingFees || @event.IsBookingFee;
                    overduePayment.ContainStandaloneFees = overduePayment.ContainStandaloneFees || @event.IsCancellationFee || @event.IsNoShowFee; // possible? I don't think so
                    overduePayment.OverdueAmount += @event.Amount;
                    context.Save(overduePayment);
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
