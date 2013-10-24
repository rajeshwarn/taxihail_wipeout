using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Enumeration;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;

namespace apcurium.MK.Booking.Test.Integration
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected PayPalExpressCheckoutPaymentDetailsGenerator sut;
        protected List<ICommand> commands = new List<ICommand>();

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
               .Callback<Envelope<ICommand>>(x => this.commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
               .Callback<IEnumerable<Envelope<ICommand>>>(x => this.commands.AddRange(x.Select(e => e.Body)));

            this.sut = new PayPalExpressCheckoutPaymentDetailsGenerator(() => new BookingDbContext(dbName));
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
            this.sut.Handle(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = paymentId,
                Amount = 12.34m,
                Token = "the token",
                OrderId = orderId,
                
            });

            using (var context = new BookingDbContext(dbName))
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
        private Guid _paymentId;
        [SetUp]
        public void Setup()
        {
            _paymentId = Guid.NewGuid();
            this.sut.Handle(new PayPalExpressCheckoutPaymentInitiated
            {
                SourceId = _paymentId,
                OrderId = Guid.NewGuid(),
                Amount = 12.34m,
                Token = "the token",
            });
        }


        [Test]
        public void when_payment_cancelled_then_dto_updated()
        {
            this.sut.Handle(new PayPalExpressCheckoutPaymentCancelled
            {
                SourceId = _paymentId,
            });

            using (var context = new BookingDbContext(dbName))
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
            this.sut.Handle(new PayPalExpressCheckoutPaymentCompleted
            {
                SourceId = _paymentId,
                PayPalPayerId = "thepayer",
                TransactionId = "thetransaction"
            });

            using (var context = new BookingDbContext(dbName))
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
