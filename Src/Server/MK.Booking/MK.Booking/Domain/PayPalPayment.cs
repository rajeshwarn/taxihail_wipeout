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
        private bool _cancelled;
        private bool _completed;
        private Guid _orderId;
        private string _token;

        protected PayPalPayment(Guid id)
            : base(id)
        {
            Handles<PayPalExpressCheckoutPaymentInitiated>(OnPayPalExpressCheckoutPaymentInitiated);
            Handles<PayPalExpressCheckoutPaymentCancelled>(OnPayPalExpressCheckoutPaymentCancelled);
            Handles<PayPalExpressCheckoutPaymentCompleted>(OnPayPalExpressCheckoutPaymentCompleted);
        }

        public PayPalPayment(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public PayPalPayment(Guid id, Guid orderId, string token, decimal amount, decimal meter, decimal tip) : this(id)
        {
            if (token == null) throw new InvalidOperationException("token cannot be null");

            Update(new PayPalExpressCheckoutPaymentInitiated
            {
                OrderId = orderId,
                Token = token,
                Amount = amount,
                Meter = meter,
                Tip = tip,
            });
        }

        public void Cancel()
        {
            if (_completed) throw new InvalidOperationException("Payment is completed");
            Update(new PayPalExpressCheckoutPaymentCancelled());
        }

        public void Complete(string transactionId, string payerId)
        {
            if (_cancelled) throw new InvalidOperationException("Payment is cancelled");
            Update(new PayPalExpressCheckoutPaymentCompleted
            {
                PayPalPayerId = payerId,
                TransactionId = transactionId,
                OrderId = _orderId,
                Token = _token,
                Amount = _amount,
            });
        }

        private void OnPayPalExpressCheckoutPaymentCancelled(PayPalExpressCheckoutPaymentCancelled obj)
        {
            _cancelled = true;
        }

        private void OnPayPalExpressCheckoutPaymentCompleted(PayPalExpressCheckoutPaymentCompleted obj)
        {
            _completed = true;
        }

        private void OnPayPalExpressCheckoutPaymentInitiated(PayPalExpressCheckoutPaymentInitiated obj)
        {
            _orderId = obj.OrderId;
            _token = obj.Token;
            _amount = obj.Amount;
        }
    }
}