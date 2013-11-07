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
        private string _token;
        private decimal _amount;

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

        public PayPalPayment(Guid id, Guid orderId, string token, decimal amount, decimal meter, decimal tip) : this(id)
        {
            if(token == null) throw new InvalidOperationException("token cannot be null");

            this.Update(new PayPalExpressCheckoutPaymentInitiated
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
            this.Update(new PayPalExpressCheckoutPaymentCancelled());
        }

        public void Complete(string transactionId, string payerId)
        {
            if(_cancelled) throw new InvalidOperationException("Payment is cancelled");
            this.Update(new PayPalExpressCheckoutPaymentCompleted
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
            this._cancelled = true;
        }

        private void OnPayPalExpressCheckoutPaymentCompleted(PayPalExpressCheckoutPaymentCompleted obj)
        {
            this._completed = true;
        }

        private void OnPayPalExpressCheckoutPaymentInitiated(PayPalExpressCheckoutPaymentInitiated obj)
        {
            _orderId = obj.OrderId;
            _token = obj.Token;
            _amount = obj.Amount;
        }
    }
}
