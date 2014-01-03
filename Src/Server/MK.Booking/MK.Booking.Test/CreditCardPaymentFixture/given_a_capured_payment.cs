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
    public class given_a_captured_payment
    {
        [SetUp]
        public void Setup()
        {
            _orderId = Guid.NewGuid();
            _paymentId = Guid.NewGuid();
            _authCode = "123456";

            _sut = new EventSourcingTestHelper<CreditCardPayment>();
            _sut.Setup(new CreditCardPaymentCommandHandler(_sut.Repository));
            _sut.Given(new CreditCardPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = _orderId,
                TransactionId = "the transaction",
                Amount = 12.34m
            });
            _sut.Given(new CreditCardPaymentCaptured {SourceId = _paymentId});
        }

        private EventSourcingTestHelper<CreditCardPayment> _sut;
        private Guid _orderId;
        private Guid _paymentId;
        private string _authCode;

        [Test]
        public void when_capturing_the_payment_again()
        {
            Assert.Throws<InvalidOperationException>(() => _sut.When(new CaptureCreditCardPayment
            {
                PaymentId = _paymentId,
                AuthorizationCode = _authCode,
            }));
        }
    }
}