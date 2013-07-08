using System;
using System.Collections.Generic;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Domain
{
    public class PayPalPayment: EventSourced
    {
        private bool _cancelled;
        private bool _completed;
        private Guid _orderId;

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
            this.LoadFrom(history);
        }

        public PayPalPayment(Guid id, Guid orderId, string token, decimal amount) : this(id)
        {
            if(token == null) throw new InvalidOperationException("token cannot be null");

            this.Update(new PayPalExpressCheckoutPaymentInitiated
            {
                OrderId = orderId,
                Token = token,
                Amount = amount
            });
        }

        public void Cancel()
        {
            if (_completed) throw new InvalidOperationException("Payment is completed");
            this.Update(new PayPalExpressCheckoutPaymentCancelled());
        }

        public void Complete(string payerId)
        {
            if(_cancelled) throw new InvalidOperationException("Payment is cancelled");
            this.Update(new PayPalExpressCheckoutPaymentCompleted
            {
                PayPalPayerId = payerId,
                OrderId = _orderId,
            });
        }

        private void OnPayPalExpressCheckoutPaymentCancelled(PayPalExpressCheckoutPaymentCancelled obj)
        {
            this._cancelled = true;
        }

        private void OnPayPalExpressCheckoutPaymentCompleted(PayPalExpressCheckoutPaymentCompleted obj)
        {
            this._completed = true;
        }

        private void OnPayPalExpressCheckoutPaymentInitiated(PayPalExpressCheckoutPaymentInitiated obj)
        {
            _orderId = obj.OrderId;
        }
    }
}
