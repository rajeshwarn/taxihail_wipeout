using System;
using System.Collections.Generic;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Domain
{
    public class CreditCardPayment: EventSourced
    {
        private Guid _orderId;
        private string _transactionId;
        private string _authorizationCode;        
        private decimal _amount;
        private decimal _meter;
        private decimal _tip;
        private bool _isCaptured;

        protected CreditCardPayment(Guid id)
            : base(id)
        {
            Handles<CreditCardPaymentInitiated>(OnCreditCardPaymentInitiated);
            Handles<CreditCardPaymentCaptured>(OnCreditCardPaymentCaptured);
        }

        public CreditCardPayment(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {               
            this.LoadFrom(history);
        }

        public CreditCardPayment(Guid id, Guid orderId, string transactionId, decimal amount, decimal meter, decimal tip, string cardToken, PaymentProvider provider)
            : this(id)
        {
            if (transactionId == null) throw new InvalidOperationException("transactionId cannot be null");

            this.Update(new CreditCardPaymentInitiated
            {
                OrderId = orderId,
                TransactionId = transactionId,
                Amount = amount,
                Meter = meter,
                Tip = tip,
                CardToken = cardToken,                
                Provider = provider, 
            });
        }

        public void Capture( PaymentProvider provider, string authorizationCode)
        {
            if (_isCaptured)
            {
                throw new InvalidOperationException("Payment is already captured");
            }

            this.Update(new CreditCardPaymentCaptured
            {
                OrderId = _orderId,
                TransactionId = _transactionId,
                AuthorizationCode = authorizationCode,
                Amount = _amount,
                Meter = _meter,
                Tip = _tip,
                Provider = provider
                
            });
        }


        private void OnCreditCardPaymentInitiated(CreditCardPaymentInitiated obj)
        {
            _orderId = obj.OrderId;
            _transactionId = obj.TransactionId;
            _amount = obj.Amount;
            _meter = obj.Meter;
            _tip = obj.Tip;
            
        }

        private void OnCreditCardPaymentCaptured(CreditCardPaymentCaptured obj)
        {
            _isCaptured = true;
            _authorizationCode = obj.AuthorizationCode;
        }
    }
}
