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
    public class given_no_payment
    {
        [SetUp]
        public void Setup()
        {
            _sut = new EventSourcingTestHelper<PayPalPayment>();
            _sut.Setup(new PayPalPaymentCommandHandler(_sut.Repository));
        }

        private EventSourcingTestHelper<PayPalPayment> _sut;

        [Test]
        public void when_initiating_a_payment()
        {
            const string token = "Payment Token";
            var orderId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            var amount = 12.34m;
            _sut.When(new InitiatePayPalExpressCheckoutPayment
            {
                OrderId = orderId,
                PaymentId = paymentId,
                Token = token,
                Amount = amount,
            });

            var @event = _sut.ThenHasSingle<PayPalExpressCheckoutPaymentInitiated>();

            Assert.AreEqual(paymentId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(token, @event.Token);
            Assert.AreEqual(amount, @event.Amount);
        }
    }
}