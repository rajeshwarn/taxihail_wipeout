using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Enumeration;
using NUnit.Framework;
using apcurium.MK.Booking.CommandHandlers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Common.Tests;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.Events;

namespace apcurium.MK.Booking.Test.CreditCardPaymentFixture
{
    [TestFixture]
    public class given_no_payment
    {

        private EventSourcingTestHelper<CreditCardPayment> sut;

        [SetUp]
        public void Setup()
        {
            sut = new EventSourcingTestHelper<CreditCardPayment>();
            this.sut.Setup(new CreditCardPaymentCommandHandler(this.sut.Repository));
        }

        [Test]
        public void when_initiating_a_payment()
        {
            const string transactionId = "transaction123456";
            Guid orderId = Guid.NewGuid();
            Guid paymentId = Guid.NewGuid();
            var token = Guid.NewGuid().ToString();
            decimal amount = 12.34m;
            sut.When(new InitiateCreditCardPayment
            {
                PaymentId = paymentId,
                TransactionId = transactionId,
                Amount = amount,
                OrderId = orderId,
                CardToken = token,
                Provider = PaymentProvider.CMT
            });

            var @event = sut.ThenHasSingle<CreditCardPaymentInitiated>();

            Assert.AreEqual(paymentId, @event.SourceId);
            Assert.AreEqual(orderId, @event.OrderId);
            Assert.AreEqual(transactionId, @event.TransactionId);
            Assert.AreEqual(amount, @event.Amount);
            Assert.AreEqual(token, @event.CardToken);
        }
    }
}