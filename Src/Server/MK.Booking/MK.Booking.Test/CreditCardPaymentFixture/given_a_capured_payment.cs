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

namespace apcurium.MK.Booking.Test.CreditCardPaymentFixture
{
    [TestFixture]
    public class given_a_captured_payment
    {
        private EventSourcingTestHelper<CreditCardPayment> sut;
        private Guid _orderId;
        private Guid _paymentId;
        private string _authCode;

        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();
            _paymentId = Guid.NewGuid();
            _authCode = "123456";

            sut = new EventSourcingTestHelper<CreditCardPayment>();
            sut.Setup(new CreditCardPaymentCommandHandler(this.sut.Repository));
            sut.Given(new CreditCardPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                TransactionId = "the transaction",                
                Amount = 12.34m
            });
            sut.Given(new CreditCardPaymentCaptured() { SourceId = _paymentId });
        }

        [Test]
        public void when_capturing_the_payment_again()
        {
            Assert.Throws<InvalidOperationException>(() => sut.When(new CaptureCreditCardPayment
            {
                PaymentId = _paymentId,
                AuthorizationCode = _authCode,
            }));

        }
    }
}
