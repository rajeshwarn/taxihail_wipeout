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
    public class given_a_completed_payment
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
            _sut.Given(new PayPalExpressCheckoutPaymentCompleted {SourceId = _paymentId});
        }

        private EventSourcingTestHelper<PayPalPayment> _sut;
        private Guid _orderId;
        private Guid _paymentId;

        [Test]
        public void when_cancelling_the_payment()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new CancelPayPalExpressCheckoutPayment
            {
                PaymentId = _paymentId,
            }));
        }
    }
}