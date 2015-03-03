using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.EventHandlers;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging;
using Moq;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.OverduePaymentFixture
{
    public class given_a_view_model_generator : given_a_read_model_database
    {
        protected List<ICommand> Commands = new List<ICommand>();
        protected OverduePaymentDetailGenerator Sut;

        public given_a_view_model_generator()
        {
            var bus = new Mock<ICommandBus>();
            bus.Setup(x => x.Send(It.IsAny<Envelope<ICommand>>()))
                .Callback<Envelope<ICommand>>(x => Commands.Add(x.Body));
            bus.Setup(x => x.Send(It.IsAny<IEnumerable<Envelope<ICommand>>>()))
                .Callback<IEnumerable<Envelope<ICommand>>>(x => Commands.AddRange(x.Select(e => e.Body)));

            Sut = new OverduePaymentDetailGenerator(() => new BookingDbContext(DbName));
        }
    }

    [TestFixture]
    public class given_no_overdue_payment : given_a_view_model_generator
    {
        [Test]
        public void when_overdue_payment_logged_then_dto_populated()
        {
            var orderId = Guid.NewGuid();
            var transactionDate = DateTime.Now;

            Sut.Handle(new OverduePaymentLogged
            {
                OrderId = orderId,
                IBSOrderId = 4455,
                Amount = 42.35m,
                TransactionDate = transactionDate,
                TransactionId = "1337"
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OverduePaymentDetail>(orderId);
                Assert.NotNull(dto);

                Assert.AreEqual(orderId, dto.OrderId);
                Assert.AreEqual(4455, dto.IBSOrderId);
                Assert.AreEqual(42.35m, dto.OverdueAmount);
                Assert.AreEqual("1337", dto.TransactionId);
                Assert.AreEqual(false, dto.IsPaid);
            }
        }
    }

    [TestFixture]
    public class given_an_overdue_payment : given_a_view_model_generator
    {
        readonly Guid _orderId = Guid.NewGuid();

        public given_an_overdue_payment()
        {
            Sut.Handle(new OverduePaymentLogged
            {
                OrderId = _orderId,
                IBSOrderId = 4455,
                Amount = 42.35m,
                TransactionDate = DateTime.Now,
                TransactionId = "1337"
            });
        }

        [Test]
        public void when_overdue_payment_settled_then_dto_updated()
        {
            Sut.Handle(new OverduePaymentSettled
            {
                OrderId = _orderId
            });

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Find<OverduePaymentDetail>(_orderId);
                Assert.NotNull(dto);

                Assert.AreEqual(_orderId, dto.OrderId);
                Assert.AreEqual(4455, dto.IBSOrderId);
                Assert.AreEqual(42.35m, dto.OverdueAmount);
                Assert.AreEqual("1337", dto.TransactionId);
                Assert.AreEqual(true, dto.IsPaid);
            }
        }
    }
}
