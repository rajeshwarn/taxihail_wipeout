#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CreditCardPaymentFixture
{
    [TestFixture]
    public class given_no_payment
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<CreditCardPayment>();
            _sut.Setup(new CreditCardPaymentCommandHandler(_sut.Repository));
        }

        private EventSourcingTestHelper<CreditCardPayment> _sut;

        [Test]
        public void when_initiating_a_payment()
        {
            const string transactionId = "transaction123456";
            var orderId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            var token = Guid.NewGuid().ToString();
            var amount = 12.34m;
            _sut.When(new InitiateCreditCardPayment
            {
                PaymentId = paymentId,
                TransactionId = transactionId,
                Amount = amount,
                OrderId = orderId,
                CardToken = token,
                Provider = PaymentProvider.Cmt
            });

            var @event = _sut.ThenHasSingle<CreditCardPaymentInitiated>();

            Assert.AreEqual(paymentId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(transactionId, @event.TransactionId);
            Assert.AreEqual(amount, @event.Amount);
            Assert.AreEqual(token, @event.CardToken);
        }
    }
}