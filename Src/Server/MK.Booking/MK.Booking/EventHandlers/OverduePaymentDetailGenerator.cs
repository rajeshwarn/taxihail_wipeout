using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
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
                        ContainBookingFees = @event.FeeType == FeeTypes.Booking,
                        ContainStandaloneFees = @event.FeeType == FeeTypes.Cancellation || @event.FeeType == FeeTypes.NoShow
                    });
                }
                else
                {
                    if (@event.FeeType != FeeTypes.Booking)
                    {
                        overduePayment.TransactionId = @event.TransactionId;
                    }

                    if (@event.IBSOrderId.HasValue)
                    {
                        overduePayment.IBSOrderId = @event.IBSOrderId;
                    }

                    overduePayment.ContainBookingFees = overduePayment.ContainBookingFees
                        || @event.FeeType == FeeTypes.Booking;
                    overduePayment.ContainStandaloneFees = overduePayment.ContainStandaloneFees
                        || @event.FeeType == FeeTypes.Cancellation
                        || @event.FeeType == FeeTypes.NoShow; // is such a thing even possible? I don't think so

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
