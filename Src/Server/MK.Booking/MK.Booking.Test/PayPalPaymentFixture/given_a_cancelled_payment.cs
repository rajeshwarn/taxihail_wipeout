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
    public class given_a_cancelled_payment
    {
        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();
            _paymentId = Guid.NewGuid();

            sut = new EventSourcingTestHelper<PayPalPayment>();
            sut.Setup(new PayPalPaymentCommandHandler(sut.Repository));
            sut.Given(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                Token = "the token",
                Amount = 12.34m
            });
            sut.Given(new PayPalExpressCheckoutPaymentCancelled {SourceId = _paymentId});
        }

        private EventSourcingTestHelper<PayPalPayment> sut;
        private Guid _orderId;
        private Guid _paymentId;

        [Test]
        public void when_completing_the_payment()
        {
            Assert.Throws<InvalidOperationException>(() => sut.When(new CompletePayPalExpressCheckoutPayment
            {
                PaymentId = _paymentId,
            }));
        }
    }
}