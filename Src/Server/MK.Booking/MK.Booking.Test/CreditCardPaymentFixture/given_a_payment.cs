#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.CreditCardPaymentFixture
{
    [TestFixture]
    public class given_a_payment
    {
        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();
            _paymentId = Guid.NewGuid();

            _sut = new EventSourcingTestHelper<CreditCardPayment>();
            _sut.Setup(new CreditCardPaymentCommandHandler(_sut.Repository));
            _sut.Given(new CreditCardPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                TransactionId = "the transaction",
                Amount = _preAuthAmount
            });
        }

        private EventSourcingTestHelper<CreditCardPayment> _sut;
        private Guid _orderId;
        private Guid _paymentId;
        private readonly decimal _preAuthAmount = 25m;

        [Test]
        public void when_capturing_the_payment()
        {
            var accountId = Guid.NewGuid();

            _sut.When(new CaptureCreditCardPayment
            {
                PaymentId = _paymentId,
                MeterAmount = 20,
                Amount = 24,
                TipAmount = 2,
                TaxAmount = 2,
                AccountId = accountId,
                TransactionId = "123",
                IsForPrepaidOrder = true,
                IsSettlingOverduePayment = true,
                NewCardToken = "fjff"
            });

            var @event = _sut.ThenHasSingle<CreditCardPaymentCaptured_V2>();
            Assert.AreEqual("123", @event.TransactionId);
            Assert.AreEqual(24, @event.Amount);
            Assert.AreEqual(20, @event.Meter);
            Assert.AreEqual(2, @event.Tip);
            Assert.AreEqual(2, @event.Tax);
            Assert.AreEqual(_orderId, @event.OrderId);
            Assert.AreEqual(accountId, @event.AccountId);
            Assert.AreEqual("fjff", @event.NewCardToken);
            Assert.AreEqual(true, @event.IsSettlingOverduePayment);
            Assert.AreEqual(true, @event.IsForPrepaidOrder);
        }

        [Test]
        public void when_cancellation_failed()
        {
            string message = "bouh";
            _sut.When(new LogCreditCardError
            {
                Reason = message,
                PaymentId = _paymentId
            });

            var @event = _sut.ThenHasSingle<CreditCardErrorThrown>();
            Assert.AreEqual(message, @event.Reason);
            Assert.AreEqual(_paymentId, @event.SourceId);
        }
    }
}