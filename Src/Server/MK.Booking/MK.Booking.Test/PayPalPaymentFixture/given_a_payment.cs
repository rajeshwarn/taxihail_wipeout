using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Test.PayPalPaymentFixture
{
    [TestFixture]
    public class given_a_payment
    {
        private EventSourcingTestHelper<PayPalPayment> sut;
        private Guid _orderId;
        private Guid _paymentId;

        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();
            _paymentId = Guid.NewGuid();

            sut = new EventSourcingTestHelper<PayPalPayment>();
            sut.Setup(new PayPalPaymentCommandHandler(this.sut.Repository));
            sut.Given(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                Token = "the token",
                Amount = 12.34m
            });
        }

        [Test]
        public void when_cancelling_the_payment()
        {
            sut.When(new CancelPayPalExpressCheckoutPayment
            {
                PaymentId = _paymentId,
            });

            var @event = sut.ThenHasSingle<PayPalExpressCheckoutPaymentCancelled>();

        }

        [Test]
        public void when_completing_the_payment()
        {
            sut.When(new CompletePayPalExpressCheckoutPayment
            {
                PaymentId = _paymentId,
                PayPalPayerId = "payerid",
                TransactionId = "the transaction"
            });

            var @event = sut.ThenHasSingle<PayPalExpressCheckoutPaymentCompleted>();
            Assert.AreEqual("payerid", @event.PayPalPayerId);
            Assert.AreEqual("the transaction", @event.TransactionId);
            Assert.AreEqual("the token", @event.Token);
            Assert.AreEqual(12.34m, @event.Amount);
            Assert.AreEqual(_orderId, @event.OrderId);
            

        }
    }
}
