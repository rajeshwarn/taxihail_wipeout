
using System;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Test
{
    [TestFixture]
    public class given_no_payment
    {

        private EventSourcingTestHelper<PayPalPayment> sut;

        [SetUp]
        public void Setup()
        {
            sut = new EventSourcingTestHelper<PayPalPayment>();
            this.sut.Setup(new PayPalPaymentCommandHandler(this.sut.Repository));
        }

        [Test]
        public void when_initiating_a_payment()
        {
            const string token = "Payment Token";
            Guid orderId = Guid.NewGuid();
            Guid paymentId = Guid.NewGuid();
            decimal amount = 12.34m;
            sut.When(new InitiatePayPalExpressCheckoutPayment
            {
                OrderId = orderId,
                PaymentId = paymentId,
                Token = token,
                Amount = amount,
            });

            var @event = sut.ThenHasSingle<PayPalExpressCheckoutPaymentInitiated>();

            Assert.AreEqual(@event.SourceId, paymentId);
            Assert.AreEqual(@event.OrderId, orderId);
            Assert.AreEqual(@event.Token, token);
            Assert.AreEqual(@event.Amount, amount);
        }
        
    }
}
