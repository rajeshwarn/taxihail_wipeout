#region

using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Enumeration;
using Infrastructure.EventSourcing;

#endregion

namespace apcurium.MK.Booking.Domain
{
    public class CreditCardPayment : EventSourced
    {
        private decimal _amount;
        private bool _isCaptured;
        private decimal _meter;
        private Guid _orderId;
        private decimal _tip;
        private string _transactionId;
        private bool _isNoShowFee;

        protected CreditCardPayment(Guid id)
            : base(id)
        {
            Handles<CreditCardPaymentInitiated>(OnCreditCardPaymentInitiated);
            Handles<CreditCardPaymentCaptured>(OnCreditCardPaymentCaptured);
            Handles<CreditCardErrorThrown>(OnCreditCardPaymentCancellationFailed);
        }

        public CreditCardPayment(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public CreditCardPayment(Guid id, Guid orderId, string transactionId, decimal amount, decimal meter, decimal tip,
            string cardToken, PaymentProvider provider, bool isNoShowFee)
            : this(id)
        {
            if (transactionId == null) throw new InvalidOperationException("transactionId cannot be null");

            Update(new CreditCardPaymentInitiated
            {
                OrderId = orderId,
                TransactionId = transactionId,
                Amount = amount,
                Meter = meter,
                Tip = tip,
                CardToken = cardToken,
                Provider = provider,
                IsNoShowFee = isNoShowFee
            });
        }

        public void Capture(PaymentProvider provider, decimal? amount, decimal? meterAmount, decimal? tipAmount, string authorizationCode)
        {
            if (_isCaptured)
            {
                throw new InvalidOperationException("Payment is already captured");
            }

            if (_amount == 0 && _meter == 0)
            {
                _amount = amount.Value;
                _meter = meterAmount.Value;
                _tip = tipAmount.Value;
            }

            Update(new CreditCardPaymentCaptured
            {
                OrderId = _orderId,
                TransactionId = _transactionId,
                AuthorizationCode = authorizationCode,
                Amount = _amount,
                Meter = _meter,
                Tip = _tip,
                Provider = provider,
                IsNoShowFee = _isNoShowFee
            });
        }

        public void ErrorThrown(string reason)
        {
            Update(new CreditCardErrorThrown
            {
                Reason = reason
            });
        }

        private void OnCreditCardPaymentInitiated(CreditCardPaymentInitiated obj)
        {
            _orderId = obj.OrderId;
            _transactionId = obj.TransactionId;
            _amount = obj.Amount;
            _meter = obj.Meter;
            _tip = obj.Tip;
            _isNoShowFee = obj.IsNoShowFee;
        }

        private void OnCreditCardPaymentCaptured(CreditCardPaymentCaptured obj)
        {
            _isCaptured = true;
        }

        private void OnCreditCardPaymentCancellationFailed(CreditCardErrorThrown obj)
        {
           //flag as not cancelled?
        }
    }
}