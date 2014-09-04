#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
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
                Amount = 12.34m
            });
        }

        private EventSourcingTestHelper<CreditCardPayment> _sut;
        private Guid _orderId;
        private Guid _paymentId;

        [Test]
        public void when_capturing_the_payment()
        {
            _sut.When(new CaptureCreditCardPayment
            {
                PaymentId = _paymentId,
            });

            var @event = _sut.ThenHasSingle<CreditCardPaymentCaptured>();
            Assert.AreEqual("the transaction", @event.TransactionId);
            Assert.AreEqual(12.34m, @event.Amount);
            Assert.AreEqual(_orderId, @event.OrderId);
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