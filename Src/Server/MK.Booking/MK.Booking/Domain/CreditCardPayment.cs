﻿#region

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

        public void Capture(PaymentProvider provider, decimal? amount, decimal? meterAmount, decimal? tipAmount, string authorizationCode, bool isNoShowFee)
        {
            if (_isCaptured)
            {
                throw new InvalidOperationException("Payment is already captured");
            }

            // Replace preath placeholder with actual amounts
            if (_amount == 0 && _meter == 0)
            {
                if (amount.HasValue)
                    _amount = amount.Value;

                if (meterAmount.HasValue)
                    _meter = meterAmount.Value;

                if (tipAmount.HasValue)
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
                IsNoShowFee = isNoShowFee
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