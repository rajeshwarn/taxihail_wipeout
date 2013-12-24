#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Booking.Test.Integration.PayPalPaymentFixture
{
// ReSharper disable once InconsistentNaming
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected PayPalExpressCheckoutPaymentDetailsGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new PayPalExpressCheckoutPaymentDetailsGenerator(() => new BookingDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_payment : given_a_view_model_generator
    {
        [Test]
        public void when_payment_initiated_then_dto_populated()
        {
            var paymentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            Sut.Handle(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = paymentId,
                Amount = 12.34m,
                Token = "the token",
                OrderId = orderId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderPaymentDetail>(paymentId);
                Assert.NotNull(dto);
                Assert.AreEqual(orderId, dto.OrderId);
                Assert.AreEqual(12.34m, dto.Amount);
                Assert.AreEqual("the token", dto.PayPalToken);
                Assert.AreEqual(false, dto.IsCancelled);
                Assert.AreEqual(false, dto.IsCompleted);
            }
        }
    }

    [TestFixture]
    public class given_a_payment : given_a_view_model_generator
    {
        [SetUp]
        public void Setup()
        {
            _paymentId = Guid.NewGuid();
            Sut.Handle(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = Guid.NewGuid(),
                Amount = 12.34m,
                Token = "the token",
            });
        }

        private Guid _paymentId;


        [Test]
        public void when_payment_cancelled_then_dto_updated()
        {
            Sut.Handle(new PayPalExpressCheckoutPaymentCancelled
            {
                SourceId = _paymentId,
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderPaymentDetail>(_paymentId);
                Assert.NotNull(dto);
                Assert.AreEqual(true, dto.IsCancelled);
                Assert.AreEqual(false, dto.IsCompleted);
                Assert.AreEqual(PaymentType.PayPal, dto.Type);
                Assert.AreEqual(PaymentProvider.PayPal, dto.Provider);
            }
        }

        [Test]
        public void when_payment_completed_then_dto_updated()
        {
            Sut.Handle(new PayPalExpressCheckoutPaymentCompleted
            {
                SourceId = _paymentId,
                PayPalPayerId = "thepayer",
                TransactionId = "thetransaction"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OrderPaymentDetail>(_paymentId);
                Assert.NotNull(dto);
                Assert.AreEqual(false, dto.IsCancelled);
                Assert.AreEqual(true, dto.IsCompleted);
                Assert.AreEqual("thepayer", dto.PayPalPayerId);
                Assert.AreEqual("thetransaction", dto.TransactionId);
                Assert.AreEqual(PaymentType.PayPal, dto.Type);
                Assert.AreEqual(PaymentProvider.PayPal, dto.Provider);
            }
        }
    }
}