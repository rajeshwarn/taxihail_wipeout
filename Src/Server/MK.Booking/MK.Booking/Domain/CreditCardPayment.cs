using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.EventSourcing;

namespace apcurium.MK.Booking.Domain
{
    public class CreditCardPayment : EventSourced
    {
        private bool _isCaptured;
        private Guid _orderId;
        private string _transactionId;

        protected CreditCardPayment(Guid id)
            : base(id)
        {
            Handles<CreditCardPaymentInitiated>(OnCreditCardPaymentInitiated);
            Handles<CreditCardPaymentCaptured_V2>(OnCreditCardPaymentCaptured);
            Handles<CreditCardErrorThrown>(OnCreditCardPaymentCancellationFailed);
        }

        public CreditCardPayment(Guid id, IEnumerable<IVersionedEvent> history)
            : this(id)
        {
            LoadFrom(history);
        }

        public CreditCardPayment(Guid id, Guid orderId, string transactionId, decimal amount, decimal meter, decimal tip, 
            string cardToken, PaymentProvider provider)
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
                Provider = provider
            });
        }

        public void Capture(PaymentProvider provider, decimal amount, decimal meterAmount, decimal tipAmount, decimal taxAmount, string authorizationCode, string transactionId, bool isNoShowFee, Guid? promotionUsed, decimal amountSavedByPromotion, string newCardToken, Guid accountId, bool isSettlingOverduePayment)
        {
            if (_isCaptured)
            {
                throw new InvalidOperationException("Payment is already captured");
            }

            Update(new CreditCardPaymentCaptured_V2
            {
                OrderId = _orderId,
                TransactionId = transactionId.HasValue() ? transactionId : _transactionId,
                AuthorizationCode = authorizationCode,
                Amount = amount,
                Meter = meterAmount,
                Tip = tipAmount,
                Tax = taxAmount,
                Provider = provider,
                IsNoShowFee = isNoShowFee,
                IsSettlingOverduePayment = isSettlingOverduePayment,
                PromotionUsed = promotionUsed,
                AmountSavedByPromotion = amountSavedByPromotion,
                AccountId = accountId,
                NewCardToken = newCardToken
            });
        }

        public void ErrorThrown(string reason, Guid accountId)
        {
            Update(new CreditCardErrorThrown
            {
                Reason = reason,
                AccountId = accountId
            });
        }

        private void OnCreditCardPaymentInitiated(CreditCardPaymentInitiated obj)
        {
            _orderId = obj.OrderId;
            _transactionId = obj.TransactionId;
        }

        private void OnCreditCardPaymentCaptured(CreditCardPaymentCaptured_V2 obj)
        {
            _isCaptured = true;

            if (obj.TransactionId.HasValue())
            {
                _transactionId = obj.TransactionId;
            }
        }

        private void OnCreditCardPaymentCancellationFailed(CreditCardErrorThrown obj)
        {
           //flag as not cancelled?
        }
    }
}