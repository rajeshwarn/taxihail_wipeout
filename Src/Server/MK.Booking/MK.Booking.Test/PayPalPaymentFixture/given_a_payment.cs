#region

using System;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.PayPalPaymentFixture
{
    [TestFixture]
    public class given_a_payment
    {
        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();
            _paymentId = Guid.NewGuid();

            _sut = new EventSourcingTestHelper<PayPalPayment>();
            _sut.Setup(new PayPalPaymentCommandHandler(_sut.Repository));
            _sut.Given(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                Token = "the token",
                Amount = 12.34m
            });
        }

        private EventSourcingTestHelper<PayPalPayment> _sut;
        private Guid _orderId;
        private Guid _paymentId;

        [Test]
        public void when_cancelling_the_payment()
        {
            _sut.When(new CancelPayPalExpressCheckoutPayment
            {
                PaymentId = _paymentId,
            });

            var @event = _sut.ThenHasSingle<PayPalExpressCheckoutPaymentCancelled>();
        }

        [Test]
        public void when_completing_the_payment()
        {
            _sut.When(new CompletePayPalExpressCheckoutPayment
            {
                PaymentId = _paymentId,
                PayPalPayerId = "payerid",
                TransactionId = "the transaction"
            });

            var @event = _sut.ThenHasSingle<PayPalExpressCheckoutPaymentCompleted>();
            Assert.AreEqual("payerid", @event.PayPalPayerId);
            Assert.AreEqual("the transaction", @event.TransactionId);
            Assert.AreEqual("the token", @event.Token);
            Assert.AreEqual(12.34m, @event.Amount);
            Assert.AreEqual(_orderId, @event.OrderId);
        }

        [Test]
        public void when_logging_failed_cancellation()
        {
            _sut.When(new LogCancellationFailurePayPalPayment
            {
                PaymentId = _paymentId,
                Reason = "message"
            });

            var @event = _sut.ThenHasSingle<PayPalPaymentCancellationFailed>();
            Assert.AreEqual("message", @event.Reason);
        }
    }
}