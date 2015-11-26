using System;
using System.Data.SqlTypes;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.Projections;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers
{
    public class OverduePaymentDetailGenerator :
        IEventHandler<OverduePaymentLogged>,
        IEventHandler<OverduePaymentSettled>
    {
        private readonly IProjectionSet<OverduePaymentDetail> _overduePaymentProjectionSet;

        public OverduePaymentDetailGenerator(IProjectionSet<OverduePaymentDetail> overduePaymentProjectionSet)
        {
            _overduePaymentProjectionSet = overduePaymentProjectionSet;
        }

        public void Handle(OverduePaymentLogged @event)
        {
            if (!_overduePaymentProjectionSet.Exists(@event.OrderId))
            {
                DateTime? transactionDate = null;

                // Make sure transaction date is a valid SQL date time
                if (@event.TransactionDate.HasValue
                    && @event.TransactionDate.Value > SqlDateTime.MinValue
                    && @event.TransactionDate.Value < SqlDateTime.MaxValue)
                {
                    transactionDate = @event.TransactionDate.Value;
                }

                _overduePaymentProjectionSet.Add(new OverduePaymentDetail
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
                _overduePaymentProjectionSet.Update(@event.OrderId, overduePayment =>
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
                });
            }
        }

        public void Handle(OverduePaymentSettled @event)
        {
            _overduePaymentProjectionSet.Update(@event.OrderId, overduePayment =>
            {
                overduePayment.IsPaid = true;
            });
        }
    }
}
