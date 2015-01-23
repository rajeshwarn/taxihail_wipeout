#region

using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Events;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Domain
{
    public class PayPalPayment : EventSourced
    {
        private decimal _amount;
        private decimal _tip;
        private decimal _tax;
        private decimal _meter;
        private bool _cancelled;
        private bool _completed;
        private Guid _orderId;
        private string _token;

        protected PayPalPayment(Guid id)
            : base(id)
        {
            Handles<PayPalPaymentCancellationFailed>(NoAction);
        }

        public PayPalPayment(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public PayPalPayment(Guid id, Guid orderId, string token, decimal amount, decimal meter, decimal tip, decimal tax) : this(id)
        {
            if (token == null)
            {
                throw new InvalidOperationException("token cannot be null");
            }

            //Update(new PayPalExpressCheckoutPaymentInitiated
            //{
            //    OrderId = orderId,
            //    Token = token,
            //    Amount = amount,
            //    Meter = meter,
            //    Tip = tip,
            //    Tax = tax
            //});
        }

        public void LogCancellationError(string reason)
        {
            Update(new PayPalPaymentCancellationFailed
            {
                Reason = reason
            });
        }
    }
}